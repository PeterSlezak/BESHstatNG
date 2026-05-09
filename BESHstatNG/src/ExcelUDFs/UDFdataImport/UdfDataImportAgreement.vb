Option Explicit On
Option Strict On

Imports System
Imports System.Linq
Imports ExcelDna.Integration

' Agreement and method-comparison data import helpers for worksheet UDFs.
' Kept in a separate partial module so callers can use the shared UdfDataImport facade.
Partial Friend Module UdfDataImport

    Friend Function TryGetAlignedNumericWithOptionalCategory(x As Object,
                                                               y As Object,
                                                               category As Object,
                                                               requireCategory As Boolean) As (X As Double(), Y As Double(), Category As Object(), DetectedNames As String(), [Error] As ExcelError?)
        Dim ax As Object(,) = Get2D(x)
        Dim ay As Object(,) = Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim hasHeaderX As Boolean = LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = LooksLikeSingleColumnHeader(ay)
        If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        Dim startRow As Integer = If(hasHeaderX, 1, 0)
        Dim names() As String = {
                If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Reference"),
                If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Test")
            }

        Dim ac As Object(,) = Nothing
        Dim useCategory As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(category)
        If useCategory Then
            ac = Get2D(category)
            If ac Is Nothing OrElse ac.GetLength(1) <> 1 OrElse ac.GetLength(0) <> ax.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim hasHeaderC As Boolean = LooksLikeSingleColumnHeader(ac)
            If hasHeaderC <> hasHeaderX Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        End If

        Dim xv As New List(Of Double)
        Dim yv As New List(Of Double)
        Dim cv As New List(Of Object)
        For r As Integer = startRow To ax.GetLength(0) - 1
            Dim dx = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ax(r, 0))
            Dim dy = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ay(r, 0))
            If dx.HasValue AndAlso dy.HasValue Then
                xv.Add(dx.Value)
                yv.Add(dy.Value)
                If useCategory Then
                    Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(ac(r, 0))
                    If s = "" Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                    cv.Add(s)
                End If
            End If
        Next
        If requireCategory AndAlso Not useCategory Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        Dim catOut() As Object = If(useCategory, cv.ToArray(), Nothing)
        Return (xv.ToArray(), yv.ToArray(), catOut, names, Nothing)
    End Function

    Friend Function TryGetCategoryList(arg As Object, ByRef categories() As Object) As Boolean
        categories = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return False

        If TypeOf arg Is String Then
            Dim s As String = Convert.ToString(arg).Trim()
            If s = "" Then Return False
            Dim parts = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries).Select(Function(t) CType(t.Trim(), Object)).ToArray()
            If parts.Length = 0 Then Return False
            categories = parts
            Return True
        End If

        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return False
        Dim vals As New List(Of Object)
        If arr.GetLength(0) = 1 Then
            For j As Integer = 0 To arr.GetLength(1) - 1
                Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, j))
                If s <> "" Then vals.Add(s)
            Next
        ElseIf arr.GetLength(1) = 1 Then
            For i As Integer = 0 To arr.GetLength(0) - 1
                Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(i, 0))
                If s <> "" Then vals.Add(s)
            Next
        Else
            Return False
        End If
        If vals.Count = 0 Then Return False
        categories = vals.ToArray()
        Return True
    End Function

    Friend Function TryGetPairedCategoricalColumns(x As Object, y As Object) As (X As Object(), Y As Object(), DetectedNames As String(), [Error] As ExcelError?)
        Dim err As ExcelError? = Nothing
        Dim ax As Object(,) = Get2D(x)
        Dim ay As Object(,) = Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim hasHeaderX As Boolean = LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = LooksLikeSingleColumnHeader(ay)
        If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim names() As String = {
            If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Rater 1"),
            If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Rater 2")
        }
        Dim startRow As Integer = If(hasHeaderX, 1, 0)
        Dim xs As New List(Of Object)
        Dim ys As New List(Of Object)
        For r As Integer = startRow To ax.GetLength(0) - 1
            Dim sx As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(ax(r, 0))
            Dim sy As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(ay(r, 0))
            If sx <> "" AndAlso sy <> "" Then
                xs.Add(sx)
                ys.Add(sy)
            End If
        Next
        If xs.Count = 0 Then Return (Nothing, Nothing, names, ExcelError.ExcelErrorNum)
        Return (xs.ToArray(), ys.ToArray(), names, Nothing)
    End Function

    Friend Function TryGetAlignedDemingInputs(x As Object, y As Object, sdX As Object, sdY As Object) As (X As Double(), Y As Double(), SDx As Double(), SDy As Double(), DetectedNames As String(), [Error] As ExcelError?)
        Dim ax As Object(,) = Get2D(x)
        Dim ay As Object(,) = Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim hasHeaderX As Boolean = LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = LooksLikeSingleColumnHeader(ay)
        If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        Dim startRow As Integer = If(hasHeaderX, 1, 0)
        Dim names() As String = {
            If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Reference"),
            If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Test")
        }

        Dim useSdx As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(sdX)
        Dim useSdy As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(sdY)
        Dim asx As Object(,) = Nothing
        Dim asy As Object(,) = Nothing
        If useSdx Then
            asx = Get2D(sdX)
            If asx Is Nothing OrElse asx.GetLength(1) <> 1 OrElse asx.GetLength(0) <> ax.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim hasHeaderSdx As Boolean = LooksLikeSingleColumnHeader(asx)
            If hasHeaderSdx <> hasHeaderX Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        End If
        If useSdy Then
            asy = Get2D(sdY)
            If asy Is Nothing OrElse asy.GetLength(1) <> 1 OrElse asy.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim hasHeaderSdy As Boolean = LooksLikeSingleColumnHeader(asy)
            If hasHeaderSdy <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        End If

        Dim xv As New List(Of Double)
        Dim yv As New List(Of Double)
        Dim sdxv As New List(Of Double)
        Dim sdyv As New List(Of Double)

        For r As Integer = startRow To ax.GetLength(0) - 1
            Dim dx = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ax(r, 0))
            Dim dy = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ay(r, 0))
            If dx.HasValue AndAlso dy.HasValue Then
                xv.Add(dx.Value)
                yv.Add(dy.Value)
                If useSdx Then
                    Dim sx = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(asx(r, 0))
                    If Not sx.HasValue Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                    sdxv.Add(sx.Value)
                End If
                If useSdy Then
                    Dim sy = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(asy(r, 0))
                    If Not sy.HasValue Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                    sdyv.Add(sy.Value)
                End If
            End If
        Next

        Dim sdxOut() As Double = If(useSdx, sdxv.ToArray(), Nothing)
        Dim sdyOut() As Double = If(useSdy, sdyv.ToArray(), Nothing)
        Return (xv.ToArray(), yv.ToArray(), sdxOut, sdyOut, names, Nothing)
    End Function


    Friend Function TryGetOneWayIccGroups(input As Object, ByRef groups()() As Double) As Boolean
        groups = Nothing
        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim lastCol As Integer = FindLastNonBlankCol(arr, lastRow)
        If lastCol < 0 Then Return False

        Dim numericCols As Integer() = Enumerable.Range(0, lastCol + 1).ToArray()
        Dim hasHeader As Boolean = LooksLikeHeaderRow(arr, numericCols)
        Dim startRow As Integer = If(hasHeader, 1, 0)
        If startRow > lastRow Then Return False

        Dim out As New List(Of Double())
        For r As Integer = startRow To lastRow
            Dim rowVals As New List(Of Double)
            Dim sawAnyCell As Boolean = False
            For c As Integer = 0 To lastCol
                Dim cell As Object = arr(r, c)
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then Continue For
                sawAnyCell = True
                Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(cell)
                If Not d.HasValue Then Return False
                rowVals.Add(d.Value)
            Next
            If sawAnyCell AndAlso rowVals.Count > 0 Then out.Add(rowVals.ToArray())
        Next

        If out.Count < 2 Then Return False
        groups = out.ToArray()
        Return True
    End Function

    Private Function FindLastNonBlankCol(arr As Object(,), lastRow As Integer) As Integer
        For c As Integer = arr.GetLength(1) - 1 To 0 Step -1
            For r As Integer = 0 To lastRow
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, c)) Then Return c
            Next
        Next
        Return -1
    End Function

    ''' <summary>
    ''' Parses an agreement-method alpha argument, returning the supplied default when the worksheet argument is missing.
    ''' </summary>
    Friend Function ParseAlphaOrDefault(arg As Object, defaultValue As Double) As Double
        Dim a As Double = defaultValue
        If TryParseAlpha(arg, a) Then Return a
        Throw New ArgumentException("alpha must be in the open interval (0, 1).")
    End Function

    Friend Function ParseAgreementCiMethod(arg As Object, defaultValue As Agreement.AgreementCiMethod) As Agreement.AgreementCiMethod
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "ANALYTICAL", "ANALYTIC"
                Return Agreement.AgreementCiMethod.Analytical
            Case "JACKKNIFE", "JACK"
                Return Agreement.AgreementCiMethod.Jackknife
            Case "BOOTSTRAP", "PERCENTILE", "BOOTSTRAPPERCENTILE", "BOOTSTRAP_PERCENTILE"
                Return Agreement.AgreementCiMethod.BootstrapPercentile
            Case "BCA", "BOOTSTRAPBCA", "BOOTSTRAP_BCA"
                Return Agreement.AgreementCiMethod.BootstrapBCa
            Case Else
                Throw New ArgumentException("Unsupported ciMethod. Use analytical, jackknife, bootstrap, or bca.")
        End Select
    End Function

    Friend Function ParseBlandAltmanMode(arg As Object, defaultValue As Agreement.RepeatedBlandAltmanMode) As Agreement.RepeatedBlandAltmanMode
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "AUTO"
                Return Agreement.RepeatedBlandAltmanMode.Auto
            Case "SIMPLE", "PAIRS", "SIMPLEPAIRS"
                Return Agreement.RepeatedBlandAltmanMode.SimplePairs
            Case "REPEATED", "REPEATEDBYSUBJECT", "SUBJECT"
                Return Agreement.RepeatedBlandAltmanMode.RepeatedBySubject
            Case Else
                Throw New ArgumentException("Unsupported Bland–Altman mode. Use auto, simple, or repeated.")
        End Select
    End Function

    Friend Function ParseBlandAltmanScale(arg As Object, defaultValue As Agreement.BlandAltmanScale) As Agreement.BlandAltmanScale
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "RAW", "RAWDIFFERENCE", "DIFF", "DIFFERENCE"
                Return Agreement.BlandAltmanScale.RawDifference
            Case "MEANPCT", "PERCENTOFMEAN", "PCTMEAN", "PERCENTMEAN"
                Return Agreement.BlandAltmanScale.PercentOfMean
            Case "REFPCT", "PERCENTOFREFERENCE", "PCTREF", "PERCENTREF", "REFERENCE"
                Return Agreement.BlandAltmanScale.PercentOfReference
            Case "TESTPCT", "PERCENTOFTEST", "PCTTEST", "PERCENTTEST"
                Return Agreement.BlandAltmanScale.PercentOfTest
            Case "LOGRATIO", "LOG", "RATIO"
                Return Agreement.BlandAltmanScale.LogRatio
            Case Else
                Throw New ArgumentException("Unsupported Bland–Altman scale.")
        End Select
    End Function

    Friend Function ParseBlandAltmanXAxisMode(arg As Object, defaultValue As Agreement.BlandAltmanXAxisMode) As Agreement.BlandAltmanXAxisMode
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "MEAN", "MEANOFMETHODS"
                Return Agreement.BlandAltmanXAxisMode.MeanOfMethods
            Case "REFERENCE", "REF", "X"
                Return Agreement.BlandAltmanXAxisMode.ReferenceMethod
            Case "TEST", "Y"
                Return Agreement.BlandAltmanXAxisMode.TestMethod
            Case Else
                Throw New ArgumentException("Unsupported Bland–Altman xAxis.")
        End Select
    End Function

    Friend Function ParseBlandAltmanPlotMode(arg As Object, defaultValue As Agreement.RepeatedBlandAltmanPlotMode) As Agreement.RepeatedBlandAltmanPlotMode
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "ALL", "OBS", "ALLOBSERVATIONS"
                Return Agreement.RepeatedBlandAltmanPlotMode.AllObservations
            Case "MEANS", "SUBJECTMEANS", "MEANSONLY"
                Return Agreement.RepeatedBlandAltmanPlotMode.SubjectMeansOnly
            Case "BOTH", "ALLANDMEANS", "ALLOBSERVATIONSANDSUBJECTMEANS"
                Return Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans
            Case Else
                Throw New ArgumentException("Unsupported Bland–Altman plotMode.")
        End Select
    End Function

    Friend Function ParseKappaWeighting(arg As Object, defaultValue As Agreement.KappaWeightingScheme) As Agreement.KappaWeightingScheme
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "UNWEIGHTED", "COHEN", "NONE"
                Return Agreement.KappaWeightingScheme.Unweighted
            Case "LINEAR"
                Return Agreement.KappaWeightingScheme.Linear
            Case "QUADRATIC"
                Return Agreement.KappaWeightingScheme.Quadratic
            Case "CICCHETTI", "CICCHETTIALLISON", "CA"
                Return Agreement.KappaWeightingScheme.CicchettiAllison
            Case "FLEISS", "FLEISSCOHEN", "FC"
                Return Agreement.KappaWeightingScheme.FleissCohen
            Case "CUSTOM"
                Return Agreement.KappaWeightingScheme.Custom
            Case Else
                Throw New ArgumentException("Unsupported kappa weighting scheme.")
        End Select
    End Function

    Friend Function ParseDemingVarianceModel(arg As Object, defaultValue As Agreement.DemingVarianceModel) As Agreement.DemingVarianceModel
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "LAMBDA", "CONSTANTLAMBDA", "CONSTANT"
                Return Agreement.DemingVarianceModel.ConstantLambda
            Case "POINTWISE", "KNOWNPOINTWISESD", "SD", "POINTWISESD"
                Return Agreement.DemingVarianceModel.KnownPointwiseSD
            Case "CV", "CONSTANTCV"
                Return Agreement.DemingVarianceModel.ConstantCV
            Case Else
                Throw New ArgumentException("Unsupported Deming varianceModel.")
        End Select
    End Function

    Friend Function ParseOptionalNullableDouble(arg As Object) As Double
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return Double.NaN
        Return Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalDouble(arg, Double.NaN)
    End Function

    Friend Function ParseOptionalSeed(arg As Object) As Integer
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return Integer.MinValue
        Return Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalInt(arg, Integer.MinValue)
    End Function

    Friend Function ParseIccModel(arg As Object, defaultValue As String) As String
        Dim s As String = NormalizeToken(arg)
        If s = "" Then Return defaultValue
        Select Case s
            Case "ICC11", "11"
                Return "ICC11"
            Case "ICC1K", "1K"
                Return "ICC1K"
            Case "ICC21", "21"
                Return "ICC21"
            Case "ICC2K", "2K"
                Return "ICC2K"
            Case "ICC31", "31"
                Return "ICC31"
            Case "ICC3K", "3K"
                Return "ICC3K"
            Case Else
                Throw New ArgumentException("Unsupported ICC model. Use ICC11, ICC1K, ICC21, ICC2K, ICC31, or ICC3K.")
        End Select
    End Function

    Friend Function IsOneWayIcc(modelCode As String) As Boolean
        Return modelCode = "ICC11" OrElse modelCode = "ICC1K"
    End Function

    Friend Function DescribeBlandAltmanScale(scale As Agreement.BlandAltmanScale) As String
        Select Case scale
            Case Agreement.BlandAltmanScale.RawDifference
                Return "Raw difference"
            Case Agreement.BlandAltmanScale.PercentOfMean
                Return "% of paired mean"
            Case Agreement.BlandAltmanScale.PercentOfReference
                Return "% of reference method"
            Case Agreement.BlandAltmanScale.PercentOfTest
                Return "% of test method"
            Case Agreement.BlandAltmanScale.LogRatio
                Return "Log ratio"
            Case Else
                Return scale.ToString()
        End Select
    End Function


    ''' <summary>
    ''' Builds and fits Bland-Altman options from worksheet UDF arguments after importing paired method data.
    ''' </summary>
    Friend Function FitBlandAltmanFromUdfArgs(x As Object,
                                              y As Object,
                                              subjectIds As Object,
                                              alpha As Object,
                                              mode As Object,
                                              scale As Object,
                                              xAxis As Object,
                                              ciMethod As Object,
                                              bootstrapReplicates As Object,
                                              useT As Object,
                                              minSubjects As Object,
                                              minPairsPerSubject As Object,
                                              excludeSingletonSubjects As Object,
                                              allowFallbackToSimple As Object,
                                              checkProportionalBias As Object,
                                              plotMode As Object,
                                              randomSeed As Object,
                                              varNames As Object,
                                              ByRef names() As String) As Agreement.BlandAltmanResult

        Dim input = TryGetAlignedNumericWithOptionalCategory(x, y, subjectIds, requireCategory:=False)
        If input.Error.HasValue Then Throw New ArgumentException("Inputs must be aligned one-column ranges with matching row counts.")
        If input.X Is Nothing OrElse input.X.Length < 2 Then Throw New ArgumentException("At least two usable numeric pairs are required.")

        names = Global.BESHStatNG.WorksheetFunctions.ParametricUDFs.ResolveNames(varNames, input.DetectedNames, 2, "Method")

        Dim opts As New Agreement.BlandAltmanOptions With {
            .Alpha = ParseAlphaOrDefault(alpha, 0.05),
            .Mode = ParseBlandAltmanMode(mode, Agreement.RepeatedBlandAltmanMode.Auto),
            .Scale = ParseBlandAltmanScale(scale, Agreement.BlandAltmanScale.RawDifference),
            .XAxisMode = ParseBlandAltmanXAxisMode(xAxis, Agreement.BlandAltmanXAxisMode.MeanOfMethods),
            .CiMethod = ParseAgreementCiMethod(ciMethod, Agreement.AgreementCiMethod.Analytical),
            .BootstrapReplicates = Math.Max(200, Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalInt(bootstrapReplicates, 2000)),
            .UseTDistribution = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalBool(useT, True),
            .MinSubjects = Math.Max(1, Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalInt(minSubjects, 2)),
            .MinPairsPerSubject = Math.Max(1, Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalInt(minPairsPerSubject, 2)),
            .ExcludeSingletonSubjects = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalBool(excludeSingletonSubjects, True),
            .AllowFallbackToSimple = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalBool(allowFallbackToSimple, True),
            .CheckProportionalBias = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalBool(checkProportionalBias, True),
            .PlotMode = ParseBlandAltmanPlotMode(plotMode, Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans)
        }

        If input.Category IsNot Nothing Then opts.SubjectIds = input.Category

        Dim seed As Integer = ParseOptionalSeed(randomSeed)
        Dim mdl As New Agreement.BlandAltmanAgreement(input.X, input.Y, names(0), names(1), opts)
        Return mdl.Fit(seed)
    End Function

    Friend Function BuildBlandAltmanPlotDataTable(res As Agreement.BlandAltmanResult) As Object(,)
        Dim nObs As Integer = If(res.PlotX Is Nothing, 0, res.PlotX.Length)
        Dim nMeans As Integer = If(res.SubjectMeanPlotX Is Nothing, 0, res.SubjectMeanPlotX.Length)
        Dim rows As Integer = Math.Max(nObs, nMeans) + 1
        Dim out(rows - 1, 3) As Object
        out(0, 0) = "PlotX"
        out(0, 1) = "PlotY"
        out(0, 2) = "SubjectMeanX"
        out(0, 3) = "SubjectMeanY"
        For i As Integer = 0 To rows - 2
            If i < nObs Then
                out(i + 1, 0) = res.PlotX(i)
                out(i + 1, 1) = res.PlotY(i)
            Else
                out(i + 1, 0) = ""
                out(i + 1, 1) = ""
            End If
            If i < nMeans Then
                out(i + 1, 2) = res.SubjectMeanPlotX(i)
                out(i + 1, 3) = res.SubjectMeanPlotY(i)
            Else
                out(i + 1, 2) = ""
                out(i + 1, 3) = ""
            End If
        Next
        Return out
    End Function

End Module
