from common41 import *
lock();entry=load(HERE/'entry/source-preimages-v1.json')['source_pins']
paths=['BESHStat.Excel.Common/src/Results/UdfAgreementReportPreparation38.vb','BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb','BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb','BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb']
for p in paths:assert pin(p)['sha256']==entry[p]
texts={p:(ROOT/p).read_text(encoding='utf-8-sig') for p in paths}
def replace(p,a,b,count=1):
    assert texts[p].count(a)==count,'REPLACEMENT_COUNT:'+p
    texts[p]=texts[p].replace(a,b)
agreement='''
        '''+"''' <summary>Fitted typed model; host option conversion and its ordering remain at the caller.</summary>"+'''
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

        '''+"''' <summary>Deferred seed readers preserve constructor-before-reader exception precedence.</summary>"+'''
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
'''
replace(paths[0],'    End Class\nEnd Namespace',agreement+'    End Class\nEnd Namespace')
host=paths[1];policy='Global.BESHStatNG.ExcelCommon.UdfAgreementReportPreparation38.'
replace(host,'''                Dim mdl As New Agreement.WeightedDemingRegression(data.X, data.Y, names(0), names(1), opts)
                mdl.alpha = opts.Alpha
                If opts.VarianceModel = Agreement.DemingVarianceModel.ConstantLambda Then mdl.Lambda = opts.Lambda
                mdl.Fit(Nothing, seed)''','''                Dim mdl = '''+policy+'''FitDeming(data.X, data.Y, names(0), names(1), opts, seed, True)''')
replace(host,'''                Dim mdl As New Agreement.WeightedDemingRegression(data.X, data.Y, "Reference", "Test", opts)
                mdl.Fit(Nothing, seed)''','''                Dim mdl = '''+policy+'''FitDeming(data.X, data.Y, "Reference", "Test", opts, seed, False)''')
replace(host,'''                Dim mdl As New Agreement.BlandAltmanAgreement(input.X, input.Y, names(0), names(1), opts)
                mdl.Fit(seed)''','''                Dim fitResult As Agreement.BlandAltmanResult = Nothing
                Dim mdl = '''+policy+'''FitBlandAltman(input.X, input.Y, names(0), names(1), opts, Function() seed, fitResult)''')
replace(host,'''                Dim mdl As New Agreement.BlandAltmanAgreement(input.X, input.Y, "Reference", "Test", opts)
                Dim res = mdl.Fit(ParseOptionalSeed(randomSeed))''','''                Dim res As Agreement.BlandAltmanResult = Nothing
                Dim mdl = '''+policy+'''FitBlandAltman(input.X, input.Y, "Reference", "Test", opts, Function() ParseOptionalSeed(randomSeed), res)''',2)
replace(host,'''                Dim mdl As New Agreement.LinConcordanceCorrelation(Matrix.GetColumnFrom2Darray(mat, 0), Matrix.GetColumnFrom2Darray(mat, 1), names(0), names(1), opts)
                mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))''','''                Dim fitResult As Agreement.LinConcordanceResult = Nothing
                Dim mdl = '''+policy+'''FitLinConcordance(Matrix.GetColumnFrom2Darray(mat, 0), Matrix.GetColumnFrom2Darray(mat, 1), names(0), names(1), opts, Function() ParseOptionalSeed(randomSeed), fitResult)''')
replace(host,'''                Dim mdl As New Agreement.LinConcordanceCorrelation(Matrix.GetColumnFrom2Darray(mat, 0), Matrix.GetColumnFrom2Darray(mat, 1), "Reference", "Test", opts)
                Dim res = mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))''','''                Dim res As Agreement.LinConcordanceResult = Nothing
                Dim mdl = '''+policy+'''FitLinConcordance(Matrix.GetColumnFrom2Darray(mat, 0), Matrix.GetColumnFrom2Darray(mat, 1), "Reference", "Test", opts, Function() ParseOptionalSeed(randomSeed), res)''')
replace(host,'''                Dim mdl As New Agreement.WeightedKappaAgreement(cat.X, cat.Y, names(0), names(1), opts)
                mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))''','''                Dim fitResult As Agreement.KappaResult = Nothing
                Dim mdl = '''+policy+'''FitKappa(cat.X, cat.Y, names(0), names(1), opts, Function() ParseOptionalSeed(randomSeed), fitResult)''')
replace(host,'''                Dim mdl As New Agreement.WeightedKappaAgreement(cat.X, cat.Y, "Rater 1", "Rater 2", opts)
                Dim res = mdl.Fit(Nothing, ParseOptionalSeed(randomSeed))''','''                Dim res As Agreement.KappaResult = Nothing
                Dim mdl = '''+policy+'''FitKappa(cat.X, cat.Y, "Rater 1", "Rater 2", opts, Function() ParseOptionalSeed(randomSeed), res)''')
sizing='''        Public Shared Function PreparePaired(meanDifference As Object, sdDifference As Object, alpha As Object, beta As Object,
                                             readNumber As Func(Of Object, Double?), isOpenUnitInterval As Func(Of Double, Boolean),
                                             ByRef errorKind As UdfSampleSizeError38) As UdfReportProjection38
            Dim diff As Double? = readNumber(meanDifference)
            Dim sd As Double? = readNumber(sdDifference)
            Dim a As Double? = readNumber(alpha)
            Dim b As Double? = readNumber(beta)
            If Not diff.HasValue OrElse Not sd.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                Return Failed(errorKind, UdfSampleSizeError38.Value)
            End If
            If diff.Value = 0 OrElse sd.Value <= 0 OrElse Not isOpenUnitInterval(a.Value) OrElse Not isOpenUnitInterval(b.Value) Then
                Return Failed(errorKind, UdfSampleSizeError38.Numeric)
            End If
            Dim result As PairedTTestSampleSizeResult = SampleSizeCalculator.CalculatePairedTTest(diff.Value, sd.Value, a.Value, b.Value)
            Return SampleSizeUDFsReports38.MakeMetricValueTable("Required pairs", result.NumberOfPairs)
        End Function

'''
replace(paths[2],'        Public Shared Function PrepareUnpaired',sizing+'        Public Shared Function PrepareUnpaired')
before='''                Dim diff As Double? = TryGetDouble(meanDifference)
                Dim sd As Double? = TryGetDouble(sdDifference)
                Dim a As Double? = TryGetDouble(alpha)
                Dim b As Double? = TryGetDouble(beta)

                If Not diff.HasValue OrElse Not sd.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                    Return ExcelError.ExcelErrorValue
                End If
                If diff.Value = 0 OrElse sd.Value <= 0 OrElse Not IsOpenUnitInterval(a.Value) OrElse Not IsOpenUnitInterval(b.Value) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim result As PairedTTestSampleSizeResult = SampleSizeCalculator.CalculatePairedTTest(diff.Value, sd.Value, a.Value, b.Value)
                Return MakeMetricValueTable("Required pairs", result.NumberOfPairs)'''
replace(paths[3],before,'''                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report = Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PreparePaired(
                    meanDifference, sdDifference, alpha, beta, AddressOf TryGetDouble, AddressOf IsOpenUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)''')
for p in paths:
    with (ROOT/p).open('w',encoding='utf-8',newline='\r\n') as f:f.write(texts[p])
save('waves/wave2-source-v1.json',dict(files=[dict(before=dict(path=p,sha256=entry[p]),after=pin(p)) for p in paths],existing_services_extended=True,new_classes=0,agreement_seed_read_order_preserved=True,contingency='Existing Common handlers retained without source churn; characterization/investigation remains in scope.',ICC_GUI_validation_and_resampling_default_changes='UNAUTHORIZED_NOT_IMPLEMENTED',gates=GATES))
print('Wave2 four existing files; nine agreement parents and paired sizing consume existing typed services',flush=True)
