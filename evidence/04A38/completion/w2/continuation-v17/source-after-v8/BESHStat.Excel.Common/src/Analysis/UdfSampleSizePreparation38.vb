Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.SampleSizeCalc

Namespace ExcelCommon
    Public Enum UdfSampleSizeError38
        Value
        Numeric
    End Enum

    Public Enum UdfSampleSizeHypothesis38
        Invalid
        Superiority
        NonInferiority
        Equivalence
    End Enum

    ''' <summary>Ordered UDF planning orchestration over opaque host arguments. Readers retain host conversion policy.</summary>
    Public NotInheritable Class UdfSampleSizePreparation38
        Private Sub New()
        End Sub

        Private Shared Function Failed(ByRef errorKind As UdfSampleSizeError38, value As UdfSampleSizeError38) As UdfReportProjection38
            errorKind = value
            Return Nothing
        End Function

        Public Shared Function PrepareUnpaired(meanDifference As Object, _
            commonSd As Object, _
            controlToExperimentalRatio As Object, _
            alpha As Object, _
            beta As Object, _
            hypothesisType As Object, _
            margin As Object, _
            readNumber As Func(Of Object, Double?), _
            readHypothesis As Func(Of Object, UdfSampleSizeHypothesis38), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim diff As Double? = readNumber(meanDifference)
            Dim sd As Double? = readNumber(commonSd)
            Dim kappa As Double? = readNumber(controlToExperimentalRatio)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)

            If Not diff.HasValue OrElse Not sd.HasValue OrElse Not kappa.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If sd.Value <= 0 OrElse kappa.Value <= 0 OrElse Not isOpenUnitInterval(a.Value) OrElse Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim hypothesis As UdfSampleSizeHypothesis38 = readHypothesis(hypothesisType)
            If hypothesis = UdfSampleSizeHypothesis38.Invalid Then Return Failed(errorKind, UdfSampleSizeError38.Value)

            Select Case hypothesis
                Case UdfSampleSizeHypothesis38.Superiority
                    If diff.Value = 0 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                    Dim result As UnpairedTTestSampleSizeResult = SampleSizeCalculator.CalculateUnpairedTTest(diff.Value, sd.Value, kappa.Value, a.Value, b.Value)
                    Return SampleSizeUDFsReports38.MakeTwoGroupTable("Required subjects", result.NumberOfControls, result.NumberOfExperimental)

                Case UdfSampleSizeHypothesis38.NonInferiority
                    Dim m As Double? = readNumber(margin)
                    If Not m.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                    If m.Value <= 0 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)

                    Dim result As UnpairedTTestSampleSizeResult =
                        SampleSizeCalculator.CalculateNonInferiorityUnpairedTTest(diff.Value,
                                                                                 -m.Value,
                                                                                 sd.Value,
                                                                                 kappa.Value,
                                                                                 a.Value,
                                                                                 b.Value)
                    Return SampleSizeUDFsReports38.MakeTwoGroupTable("Required subjects", result.NumberOfControls, result.NumberOfExperimental)

                Case UdfSampleSizeHypothesis38.Equivalence
                    Dim m As Double? = readNumber(margin)
                    If Not m.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                    If m.Value <= 0 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)

                    Dim result As EquivalenceUnpairedTTestSampleSizeResult =
                        SampleSizeCalculator.CalculateEquivalenceUnpairedTTest(diff.Value,
                                                                              -m.Value,
                                                                              m.Value,
                                                                              sd.Value,
                                                                              kappa.Value,
                                                                              a.Value,
                                                                              b.Value)
                    Return SampleSizeUDFsReports38.MakeEquivalenceTwoGroupTable(result)

                Case Else
                    Return Failed(errorKind, UdfSampleSizeError38.Value)
            End Select
        End Function

        Public Shared Function PrepareSingleProportion(anticipatedProportion As Object, _
            nullProportion As Object, _
            alpha As Object, _
            beta As Object, _
            readNumber As Func(Of Object, Double?), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            isClosedUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim prop As Double? = readNumber(anticipatedProportion)
            Dim h0 As Double? = readNumber(nullProportion)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)

            If Not prop.HasValue OrElse Not h0.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If Not isClosedUnitInterval(prop.Value) OrElse Not isClosedUnitInterval(h0.Value) OrElse prop.Value = h0.Value OrElse Not isOpenUnitInterval(a.Value) OrElse Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim result As SingleProportionSampleSizeResult = SampleSizeCalculator.CalculateSingleProportion(prop.Value, h0.Value, a.Value, b.Value)
            Return SampleSizeUDFsReports38.MakeMetricValueTable("Required subjects", result.NumberOfSubjects)
        End Function

        Public Shared Function PrepareIndependentProportions(controlProportion As Object, _
            experimentalProportion As Object, _
            controlToExperimentalRatio As Object, _
            alpha As Object, _
            beta As Object, _
            hypothesisType As Object, _
            margin As Object, _
            readNumber As Func(Of Object, Double?), _
            readHypothesis As Func(Of Object, UdfSampleSizeHypothesis38), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            isClosedUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim cProp As Double? = readNumber(controlProportion)
            Dim eProp As Double? = readNumber(experimentalProportion)
            Dim kappa As Double? = readNumber(controlToExperimentalRatio)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)

            If Not cProp.HasValue OrElse Not eProp.HasValue OrElse Not kappa.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If Not isClosedUnitInterval(cProp.Value) OrElse Not isClosedUnitInterval(eProp.Value) OrElse kappa.Value <= 0 OrElse Not isOpenUnitInterval(a.Value) OrElse Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim hypothesis As UdfSampleSizeHypothesis38 = readHypothesis(hypothesisType)
            If hypothesis = UdfSampleSizeHypothesis38.Invalid Then Return Failed(errorKind, UdfSampleSizeError38.Value)

            Select Case hypothesis
                Case UdfSampleSizeHypothesis38.Superiority
                    If cProp.Value = eProp.Value Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                    Dim result As IndependentProportionsSampleSizeResult = SampleSizeCalculator.CalculateIndependentProportions(cProp.Value, eProp.Value, kappa.Value, a.Value, b.Value)
                    Return SampleSizeUDFsReports38.MakeIndependentProportionsTable(result)

                Case UdfSampleSizeHypothesis38.NonInferiority
                    Dim m As Double? = readNumber(margin)
                    If Not m.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                    If m.Value <= 0 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                    Dim result As IndependentProportionsSampleSizeResult =
                        SampleSizeCalculator.CalculateNonInferiorityIndependentProportions(cProp.Value,
                                                                                          eProp.Value,
                                                                                          -m.Value,
                                                                                          kappa.Value,
                                                                                          a.Value,
                                                                                          b.Value)
                    Return SampleSizeUDFsReports38.MakeIndependentProportionsTable(result)

                Case UdfSampleSizeHypothesis38.Equivalence
                    Dim m As Double? = readNumber(margin)
                    If Not m.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                    If m.Value <= 0 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                    Dim result As EquivalenceIndependentProportionsSampleSizeResult =
                        SampleSizeCalculator.CalculateEquivalenceIndependentProportions(cProp.Value,
                                                                                       eProp.Value,
                                                                                       -m.Value,
                                                                                       m.Value,
                                                                                       kappa.Value,
                                                                                       a.Value,
                                                                                       b.Value)
                    Return SampleSizeUDFsReports38.MakeEquivalenceIndependentProportionsTable(result)

                Case Else
                    Return Failed(errorKind, UdfSampleSizeError38.Value)
            End Select
        End Function

        Public Shared Function PrepareLogRank(hazardRatio As Object, _
            controlEventProportion As Object, _
            experimentalEventProportion As Object, _
            controlToExperimentalRatio As Object, _
            alpha As Object, _
            beta As Object, _
            twoSided As Object, _
            readNumber As Func(Of Object, Double?), _
            readOptionalBool As Func(Of Object, Boolean, Boolean), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim hr As Double? = readNumber(hazardRatio)
            Dim pControl As Double? = readNumber(controlEventProportion)
            Dim pExperimental As Double? = readNumber(experimentalEventProportion)
            Dim kappa As Double? = readNumber(controlToExperimentalRatio)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)

            If Not hr.HasValue OrElse Not pControl.HasValue OrElse Not pExperimental.HasValue OrElse Not kappa.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If hr.Value <= 0 OrElse hr.Value = 1.0 OrElse
               Not isOpenUnitInterval(pControl.Value) OrElse
               Not isOpenUnitInterval(pExperimental.Value) OrElse
               kappa.Value <= 0 OrElse
               Not isOpenUnitInterval(a.Value) OrElse
               Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim isTwoSided As Boolean = readOptionalBool(twoSided, True)
            Dim result As LogRankSampleSizeResult = SampleSizeCalculator.CalculateLogRankSampleSize(hr.Value,
                                                                                                    pControl.Value,
                                                                                                    pExperimental.Value,
                                                                                                    kappa.Value,
                                                                                                    a.Value,
                                                                                                    b.Value,
                                                                                                    isTwoSided)
            Return SampleSizeUDFsReports38.MakeLogRankPlanningTable(result)
        End Function

        Public Shared Function PrepareCoxBinary(hazardRatio As Object, _
            controlToExperimentalRatio As Object, _
            alpha As Object, _
            beta As Object, _
            rSquaredWithOtherCovariates As Object, _
            overallEventProportion As Object, _
            twoSided As Object, _
            readNumber As Func(Of Object, Double?), _
            isMissing As Func(Of Object, Boolean), _
            readOptionalBool As Func(Of Object, Boolean, Boolean), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim hr As Double? = readNumber(hazardRatio)
            Dim kappa As Double? = readNumber(controlToExperimentalRatio)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)

            If Not hr.HasValue OrElse Not kappa.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If hr.Value <= 0 OrElse hr.Value = 1.0 OrElse kappa.Value <= 0 OrElse Not isOpenUnitInterval(a.Value) OrElse Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim r2 As Double = 0.0
            If Not isMissing(rSquaredWithOtherCovariates) Then
                Dim parsedR2 As Double? = readNumber(rSquaredWithOtherCovariates)
                If Not parsedR2.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                If parsedR2.Value < 0 OrElse parsedR2.Value >= 1 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                r2 = parsedR2.Value
            End If

            Dim overallP As Double = Double.NaN
            If Not isMissing(overallEventProportion) Then
                Dim parsedOverallP As Double? = readNumber(overallEventProportion)
                If Not parsedOverallP.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                If Not isOpenUnitInterval(parsedOverallP.Value) Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                overallP = parsedOverallP.Value
            End If

            Dim isTwoSided As Boolean = readOptionalBool(twoSided, True)
            Dim result As CoxEventCountPlanningResult = SampleSizeCalculator.CalculateCoxEventCountBinaryCovariate(hr.Value,
                                                                                                                   kappa.Value,
                                                                                                                   a.Value,
                                                                                                                   b.Value,
                                                                                                                   r2,
                                                                                                                   overallP,
                                                                                                                   isTwoSided)
            Return SampleSizeUDFsReports38.MakeCoxPlanningTable("Binary covariate", result)
        End Function

        Public Shared Function PrepareCoxContinuous(hazardRatioPerUnit As Object, _
            covariateSd As Object, _
            alpha As Object, _
            beta As Object, _
            rSquaredWithOtherCovariates As Object, _
            overallEventProportion As Object, _
            twoSided As Object, _
            readNumber As Func(Of Object, Double?), _
            isMissing As Func(Of Object, Boolean), _
            readOptionalBool As Func(Of Object, Boolean, Boolean), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim hr As Double? = readNumber(hazardRatioPerUnit)
            Dim sdX As Double? = readNumber(covariateSd)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)

            If Not hr.HasValue OrElse Not sdX.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If hr.Value <= 0 OrElse hr.Value = 1.0 OrElse sdX.Value <= 0 OrElse Not isOpenUnitInterval(a.Value) OrElse Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim r2 As Double = 0.0
            If Not isMissing(rSquaredWithOtherCovariates) Then
                Dim parsedR2 As Double? = readNumber(rSquaredWithOtherCovariates)
                If Not parsedR2.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                If parsedR2.Value < 0 OrElse parsedR2.Value >= 1 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                r2 = parsedR2.Value
            End If

            Dim overallP As Double = Double.NaN
            If Not isMissing(overallEventProportion) Then
                Dim parsedOverallP As Double? = readNumber(overallEventProportion)
                If Not parsedOverallP.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                If Not isOpenUnitInterval(parsedOverallP.Value) Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                overallP = parsedOverallP.Value
            End If

            Dim isTwoSided As Boolean = readOptionalBool(twoSided, True)
            Dim result As CoxEventCountPlanningResult = SampleSizeCalculator.CalculateCoxEventCountContinuousCovariate(hr.Value,
                                                                                                                       sdX.Value,
                                                                                                                       a.Value,
                                                                                                                       b.Value,
                                                                                                                       r2,
                                                                                                                       overallP,
                                                                                                                       isTwoSided)
            Return SampleSizeUDFsReports38.MakeCoxPlanningTable("Continuous covariate", result)
        End Function

        Public Shared Function PrepareIcc(nullIcc As Object, _
            alternativeIcc As Object, _
            observationsPerSubject As Object, _
            alpha As Object, _
            beta As Object, _
            readNumber As Func(Of Object, Double?), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim rho0 As Double? = readNumber(nullIcc)
            Dim rho1 As Double? = readNumber(alternativeIcc)
            Dim m As Double? = readNumber(observationsPerSubject)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)

            If Not rho0.HasValue OrElse Not rho1.HasValue OrElse Not m.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If rho0.Value < 0 OrElse rho0.Value >= 1 OrElse
               rho1.Value < 0 OrElse rho1.Value >= 1 OrElse
               rho1.Value <= rho0.Value OrElse
               m.Value < 2 OrElse m.Value <> Math.Truncate(m.Value) OrElse
               Not isOpenUnitInterval(a.Value) OrElse
               Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim result As IccHypothesisTestSampleSizeResult = SampleSizeCalculator.CalculateIccHypothesisTestSampleSize(rho0.Value,
                                                                                                                        rho1.Value,
                                                                                                                        CInt(m.Value),
                                                                                                                        a.Value,
                                                                                                                        b.Value)
            Return SampleSizeUDFsReports38.MakeIccPlanningTable(result)
        End Function

        Public Shared Function PrepareBlandAltman(sdDifference As Object, _
            desiredHalfWidth As Object, _
            alpha As Object, _
            loaMultiplier As Object, _
            readNumber As Func(Of Object, Double?), _
            isMissing As Func(Of Object, Boolean), _
            isOpenUnitInterval As Func(Of Double, Boolean), _
            ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim sd As Double? = readNumber(sdDifference)
            Dim halfWidth As Double? = readNumber(desiredHalfWidth)
            Dim a As Double? = readNumber(alpha)

            If Not sd.HasValue OrElse Not halfWidth.HasValue OrElse Not a.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If sd.Value <= 0 OrElse halfWidth.Value <= 0 OrElse Not isOpenUnitInterval(a.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If

            Dim z As Double = 1.96
            If Not isMissing(loaMultiplier) Then
                Dim parsedMultiplier As Double? = readNumber(loaMultiplier)
                If Not parsedMultiplier.HasValue Then Return Failed(errorKind, UdfSampleSizeError38.Value)
                If parsedMultiplier.Value <= 0 Then Return Failed(errorKind, UdfSampleSizeError38.Numeric)
                z = parsedMultiplier.Value
            End If

            Dim result As BlandAltmanAgreementStudyPlanningResult = SampleSizeCalculator.CalculateBlandAltmanLoASampleSize(sd.Value,
                                                                                                                             halfWidth.Value,
                                                                                                                             a.Value,
                                                                                                                             z)
            Return SampleSizeUDFsReports38.MakeBlandAltmanPlanningTable(result)
        End Function
    End Class
End Namespace
