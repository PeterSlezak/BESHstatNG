Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.CausalInference

Namespace ExcelCommon
    Public Enum PsmUdfTableKind38
        Unknown
        RunSummary
        Options
        ImportSummary
        SampleSize
        ScoreModel
        RowAudit
        Matches
        MatchedData
        Balance
        Effect
        DoublyRobust
        WeightDiagnostics
        OverlapSummary
        OverlapBins
        LovePlot
        Subclass
        CemStrata
        CemWeights
        Warnings
    End Enum

    Public NotInheritable Class UdfPsmDataPreparation38
        Private Sub New()
        End Sub

        Public Shared Function CreateFitOptionsPrepared() As PsmComprehensiveFitOptions
            Dim fitOptions As New PsmComprehensiveFitOptions()
            fitOptions.StandardOptions = New PsmOptions()
            Return fitOptions
        End Function

        Public Shared Function CompleteFitOptionsPrepared(fitOptions As PsmComprehensiveFitOptions) As PsmComprehensiveFitOptions
            UdfPsmPreparation38.NormalizeFitOptions(fitOptions)
            PsmMethodCapabilities.ValidateFitOptions(fitOptions)
            Return fitOptions
        End Function

        Public Shared Function ResolveTableKindPrepared(requested As String) As PsmUdfTableKind38
            Dim name As String = PropensityScoreUDFsOptions38.NormalizeOptionKey(requested)
            Select Case LegacyUdfText38.CaseToken(name, New String() {"summary", "runsummary", "options", "import", "dataimport", "sample", "samplesize", "samplesizesummary", "scoremodel", "model", "scores", "audit", "rowaudit", "weights", "matches", "matchedpairs", "matcheddata", "matcheddataset", "balance", "diagnostics", "effect", "effects", "dr", "aipw", "doublyrobust", "weightdiagnostics", "overlap", "overlapsummary", "overlapbins", "overlaphistogram", "loveplot", "loveplotdata", "subclasses", "subclassification", "cemstrata", "cemweights", "warnings"})
                Case "summary", "runsummary"
                    Return PsmUdfTableKind38.RunSummary
                Case "options"
                    Return PsmUdfTableKind38.Options
                Case "import", "dataimport"
                    Return PsmUdfTableKind38.ImportSummary
                Case "sample", "samplesize", "samplesizesummary"
                    Return PsmUdfTableKind38.SampleSize
                Case "scoremodel", "model"
                    Return PsmUdfTableKind38.ScoreModel
                Case "scores", "audit", "rowaudit", "weights"
                    Return PsmUdfTableKind38.RowAudit
                Case "matches", "matchedpairs"
                    Return PsmUdfTableKind38.Matches
                Case "matcheddata", "matcheddataset"
                    Return PsmUdfTableKind38.MatchedData
                Case "balance", "diagnostics"
                    Return PsmUdfTableKind38.Balance
                Case "effect", "effects"
                    Return PsmUdfTableKind38.Effect
                Case "dr", "aipw", "doublyrobust"
                    Return PsmUdfTableKind38.DoublyRobust
                Case "weightdiagnostics"
                    Return PsmUdfTableKind38.WeightDiagnostics
                Case "overlap", "overlapsummary"
                    Return PsmUdfTableKind38.OverlapSummary
                Case "overlapbins", "overlaphistogram"
                    Return PsmUdfTableKind38.OverlapBins
                Case "loveplot", "loveplotdata"
                    Return PsmUdfTableKind38.LovePlot
                Case "subclasses", "subclassification"
                    Return PsmUdfTableKind38.Subclass
                Case "cemstrata"
                    Return PsmUdfTableKind38.CemStrata
                Case "cemweights"
                    Return PsmUdfTableKind38.CemWeights
                Case "warnings"
                    Return PsmUdfTableKind38.Warnings
                Case Else
                    Return PsmUdfTableKind38.Unknown
            End Select
        End Function

        Public Shared Function BuildImportSummaryPrepared(rows As IList(Of Object())) As Object(,)
            Dim table(rows.Count, 1) As Object
            table(0, 0) = "Item"
            table(0, 1) = "Value"
            For i As Integer = 0 To rows.Count - 1
                table(i + 1, 0) = rows(i)(0)
                table(i + 1, 1) = rows(i)(1)
            Next
            Return table
        End Function

        Public Shared Function BuildModelNamesPrepared(treatmentKey As String, covariateNames() As String, p As Integer) As String()
            Dim modelNames(p) As String
            modelNames(0) = treatmentKey
            For j As Integer = 0 To p - 1
                modelNames(j + 1) = covariateNames(j)
            Next
            Return modelNames
        End Function

        Public Shared Function BuildModelInputPrepared(treatmentCol(,) As Object, covariateMatrix(,) As Object, n As Integer, p As Integer) As Object(,)
            Dim modelInput(n - 1, p) As Object
            For i As Integer = 0 To n - 1
                modelInput(i, 0) = treatmentCol(i, 0)
                For j As Integer = 0 To p - 1
                    modelInput(i, j + 1) = covariateMatrix(i, j)
                Next
            Next
            Return modelInput
        End Function


        ''' <summary>Selects existing Core table builders; never owns fitting, handles or caches.</summary>
        Public Shared Function BuildNamedTable(result As PsmComprehensiveResult, fitOptions As PsmComprehensiveFitOptions,
                                              readInput As Func(Of PsmInputData), readRowIds As Func(Of Integer()),
                                              requested As String, readImportSummary As Func(Of Object(,))) As Object(,)
            Dim name As Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38 = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.ResolveTableKindPrepared(requested)
            Select Case name
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.RunSummary
                    Return PsmComprehensiveTables.RunSummaryTable(result)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.Options
                    Return PsmFrontEndTables.OptionsTable(fitOptions)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.ImportSummary
                    Return readImportSummary()
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.SampleSize
                    Return PsmFrontEndTables.SampleSizeTable(result.Result)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.ScoreModel
                    Return PsmBackendTables.ScoreModelTable(result.Result)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.RowAudit
                    Return PsmFrontEndTables.RowLevelAuditTable(readInput(), result.Result, readRowIds())
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.Matches
                    Return PsmBackendTables.MatchesTable(result.Result, readInput())
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.MatchedData
                    Return PsmFrontEndTables.MatchedDatasetTable(readInput(), result.Result, readRowIds())
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.Balance
                    Return PsmBackendTables.BalanceTable(result.Result)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.Effect
                    Return PsmBackendTables.EffectTable(result.Result)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.DoublyRobust
                    Return PsmAdvancedTables.DoublyRobustEffectTable(result.DoublyRobustResult)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.WeightDiagnostics
                    Return PsmAdvancedTables.WeightDiagnosticsTable(result.WeightDiagnostics)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.OverlapSummary
                    Return PsmAdvancedTables.OverlapSummaryTable(result.OverlapDiagnostics)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.OverlapBins
                    Return PsmAdvancedTables.OverlapBinsTable(result.OverlapDiagnostics)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.LovePlot
                    Return PsmAdvancedTables.LovePlotTable(result.LovePlotRows)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.Subclass
                    Return PsmFrontEndTables.SubclassTable(result.Result)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.CemStrata
                    Return PsmAdvancedMatchingTables.CoarsenedExactStrataTable(result.CoarsenedExactResult)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.CemWeights
                    Return PsmAdvancedMatchingTables.CoarsenedExactWeightsTable(readInput(), result.CoarsenedExactResult)
                Case Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.Warnings
                    Return PsmComprehensiveTables.WarningsTable(result)
                Case Else
                    Return PsmResult.EmptyTable("Unknown PSM table: " & If(requested, String.Empty))
            End Select
        End Function

    End Class
End Namespace
