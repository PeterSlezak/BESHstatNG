Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace CausalInference

    ''' <summary>
    ''' Converts PSM backend/front-end output matrices into ResultTable objects so the GUI
    ''' can use the same WriteResults / ProcessListofResultTables formatting path as GLM,
    ''' GEE, LMM and MMRM outputs.
    ''' </summary>
    ''' <remarks>
    ''' This class intentionally has no Excel COM dependency.  It only builds ResultTable
    ''' instances and can be reused by the GUI, UDF layer or tests that need formatted
    ''' result sections.
    ''' </remarks>
    Public NotInheritable Class PsmFormattedResultTables
        Private Sub New()
        End Sub

        Public Shared Function AnalyzedInputDataTable(input As PsmInputData,
                                                      Optional sourceRowIds As Integer() = Nothing) As Global.BESHStatNG.ResultTable
            Return MatrixToResultTable("Analyzed input data", BuildAnalyzedInputDataMatrix(input, sourceRowIds),
                                       "Rows shown here are the rows retained after the DataObj import/alignment step and used by the PSM backend. Source Row is the original worksheet row number.")
        End Function

        Public Shared Function GeneralResultTables(fitResult As PsmComprehensiveResult,
                                                   fitOptions As PsmComprehensiveFitOptions,
                                                   dataImportSummary As Object(,)) As List(Of Global.BESHStatNG.ResultTable)
            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()

            tables.Add(MatrixToResultTable("Propensity score analysis - run summary", PsmComprehensiveTables.RunSummaryTable(fitResult)))
            tables.Add(MatrixToResultTable("Analysis options", PsmFrontEndTables.OptionsTable(fitOptions)))
            tables.Add(MatrixToResultTable("Data import summary", dataImportSummary))
            tables.Add(MatrixToResultTable("Sample-size summary", PsmFrontEndTables.SampleSizeTable(If(fitResult Is Nothing, Nothing, fitResult.Result))))
            tables.Add(MatrixToResultTable("Effect estimates", PsmBackendTables.EffectTable(If(fitResult Is Nothing, Nothing, fitResult.Result))))
            tables.Add(MatrixToResultTable("Effect sensitivity summary", PsmFrontEndTables.EffectSensitivitySummaryTable(fitResult)))
            tables.Add(MatrixToResultTable("Propensity score model", PsmBackendTables.ScoreModelTable(If(fitResult Is Nothing, Nothing, fitResult.Result))))
            tables.Add(MatrixToResultTable("Warnings", PsmComprehensiveTables.WarningsTable(fitResult)))

            Return tables
        End Function

        Public Shared Function DiagnosticsTables(input As PsmInputData,
                                                 fitResult As PsmComprehensiveResult,
                                                 Optional includeDefaultSensitivity As Boolean = True) As List(Of Global.BESHStatNG.ResultTable)
            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()
            Dim result As PsmResult = If(fitResult Is Nothing, Nothing, fitResult.Result)

            tables.Add(MatrixToResultTable("Balance diagnostics", PsmBackendTables.BalanceTable(result)))
            tables.Add(MatrixToResultTable("Doubly robust AIPW estimate", PsmAdvancedTables.DoublyRobustEffectTable(If(fitResult Is Nothing, Nothing, fitResult.DoublyRobustResult))))
            tables.Add(MatrixToResultTable("Weight diagnostics", PsmAdvancedTables.WeightDiagnosticsTable(If(fitResult Is Nothing, Nothing, fitResult.WeightDiagnostics))))
            tables.Add(MatrixToResultTable("Overlap summary", PsmAdvancedTables.OverlapSummaryTable(If(fitResult Is Nothing, Nothing, fitResult.OverlapDiagnostics))))
            tables.Add(MatrixToResultTable("Overlap histogram bins", PsmAdvancedTables.OverlapBinsTable(If(fitResult Is Nothing, Nothing, fitResult.OverlapDiagnostics))))
            tables.Add(MatrixToResultTable("Love-plot data", PsmAdvancedTables.LovePlotTable(If(fitResult Is Nothing, Nothing, fitResult.LovePlotRows))))
            tables.Add(MatrixToResultTable("Subclassification strata", PsmFrontEndTables.SubclassTable(result)))

            If includeDefaultSensitivity Then
                tables.AddRange(DefaultSensitivityTables(input, fitResult))
            End If

            Return tables
        End Function

        Public Shared Function RowAuditTables(input As PsmInputData,
                                              fitResult As PsmComprehensiveResult,
                                              Optional sourceRowIds As Integer() = Nothing) As List(Of Global.BESHStatNG.ResultTable)
            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()
            tables.Add(MatrixToResultTable("Row-level scores, weights and inclusion audit",
                                           PsmFrontEndTables.RowLevelAuditTable(input, If(fitResult Is Nothing, Nothing, fitResult.Result), sourceRowIds),
                                           "This audit table is useful for checking the exact analysis rows, score estimates, inclusion flags, weights and match status."))
            Return tables
        End Function

        Public Shared Function MatchedPairsTables(input As PsmInputData,
                                                  fitResult As PsmComprehensiveResult) As List(Of Global.BESHStatNG.ResultTable)
            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()
            tables.Add(MatrixToResultTable("Matched pairs", PsmBackendTables.MatchesTable(If(fitResult Is Nothing, Nothing, fitResult.Result), input)))
            Return tables
        End Function

        Public Shared Function MatchedDatasetTables(input As PsmInputData,
                                                    fitResult As PsmComprehensiveResult,
                                                    Optional sourceRowIds As Integer() = Nothing) As List(Of Global.BESHStatNG.ResultTable)
            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()
            tables.Add(MatrixToResultTable("Matched analysis dataset",
                                           PsmFrontEndTables.MatchedDatasetTable(input, If(fitResult Is Nothing, Nothing, fitResult.Result), sourceRowIds),
                                           "Rows with positive matching weights. This table is separated from the general results because it may be large."))
            Return tables
        End Function

        Public Shared Function CoarsenedExactTables(input As PsmInputData,
                                                    fitResult As PsmComprehensiveResult) As List(Of Global.BESHStatNG.ResultTable)
            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()
            If fitResult Is Nothing OrElse fitResult.CoarsenedExactResult Is Nothing Then Return tables

            tables.Add(MatrixToResultTable("Coarsened exact matching strata", PsmAdvancedMatchingTables.CoarsenedExactStrataTable(fitResult.CoarsenedExactResult)))
            tables.Add(MatrixToResultTable("Coarsened exact matching weights", PsmAdvancedMatchingTables.CoarsenedExactWeightsTable(input, fitResult.CoarsenedExactResult)))
            Return tables
        End Function

        Public Shared Function DefaultSensitivityTables(input As PsmInputData,
                                                        fitResult As PsmComprehensiveResult) As List(Of Global.BESHStatNG.ResultTable)
            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()
            If fitResult Is Nothing OrElse fitResult.Result Is Nothing OrElse fitResult.Result.Matches Is Nothing OrElse fitResult.Result.Matches.Count = 0 Then Return tables
            If input Is Nothing OrElse input.Outcome Is Nothing Then Return tables

            Try
                Dim rosenbaum As PsmRosenbaumSensitivityResult = PsmSensitivityAnalysis.RosenbaumMatchedPairs(input,
                                                                                                               fitResult.Result.Matches,
                                                                                                               maxGamma:=3.0,
                                                                                                               gammaStep:=0.1,
                                                                                                               alpha:=0.05,
                                                                                                               alternative:=PsmSensitivityAlternative.TwoSided)
                tables.Add(MatrixToResultTable("Rosenbaum matched-pair sensitivity summary", PsmSensitivityTables.RosenbaumSummaryTable(rosenbaum)))
                tables.Add(MatrixToResultTable("Rosenbaum matched-pair sensitivity by Gamma", PsmSensitivityTables.RosenbaumTable(rosenbaum)))
            Catch ex As Exception
                tables.Add(MatrixToResultTable("Rosenbaum matched-pair sensitivity", PsmResult.EmptyTable("Sensitivity analysis could not be computed: " & ex.Message)))
            End Try

            Return tables
        End Function

        Public Shared Function MatrixToResultTable(title As String,
                                                   matrix As Object(,),
                                                   Optional footnote As String = Nothing) As Global.BESHStatNG.ResultTable
            Dim rt As New Global.BESHStatNG.ResultTable()
            If Not String.IsNullOrWhiteSpace(title) Then rt.AddTitle(title)

            If matrix Is Nothing Then
                Dim body(0, 0) As Object
                body(0, 0) = "No rows available"
                rt.SetBody(body)
                Return rt
            End If

            Dim rowCount As Integer = matrix.GetLength(0)
            Dim colCount As Integer = matrix.GetLength(1)
            If rowCount <= 0 OrElse colCount <= 0 Then
                Dim body(0, 0) As Object
                body(0, 0) = "No rows available"
                rt.SetBody(body)
                Return rt
            End If

            If rowCount = 1 Then
                Dim body(rowCount - 1, colCount - 1) As Object
                For c As Integer = 0 To colCount - 1
                    body(0, c) = matrix(0, c)
                Next
                rt.SetBody(body)
            Else
                Dim header(colCount - 1) As String
                For c As Integer = 0 To colCount - 1
                    header(c) = If(matrix(0, c) Is Nothing, "", CStr(matrix(0, c)))
                Next
                rt.AddHeaderTopRow(header)

                Dim body(rowCount - 2, colCount - 1) As Object
                For r As Integer = 1 To rowCount - 1
                    For c As Integer = 0 To colCount - 1
                        body(r - 1, c) = matrix(r, c)
                    Next
                Next
                rt.SetBody(body)

                AddPValueColumns(rt, header)
            End If

            If Not String.IsNullOrWhiteSpace(footnote) Then rt.AddFootnote(footnote)
            Return rt
        End Function

        Private Shared Sub AddPValueColumns(table As Global.BESHStatNG.ResultTable, headers As String())
            If table Is Nothing OrElse headers Is Nothing Then Return

            For c As Integer = 0 To headers.Length - 1
                Dim h As String = If(headers(c), String.Empty).Trim().ToLowerInvariant()
                If h = "p" OrElse h = "p-value" OrElse h = "p value" OrElse h.Contains("p-value") OrElse h.Contains("p value") Then
                    table.AddPvalueToFormat(c + 1)
                End If
            Next
        End Sub

        Private Shared Function BuildAnalyzedInputDataMatrix(input As PsmInputData,
                                                             Optional sourceRowIds As Integer() = Nothing) As Object(,)
            If input Is Nothing OrElse input.RowCount = 0 Then Return PsmResult.EmptyTable("No analyzed input data available")

            Dim n As Integer = input.RowCount
            Dim p As Integer = input.CovariateCount
            Dim hasOutcome As Boolean = input.Outcome IsNot Nothing
            Dim hasSuppliedScores As Boolean = input.SuppliedPropensityScores IsNot Nothing
            Dim hasExact As Boolean = input.ExactGroupLabels IsNot Nothing

            Dim fixedCols As Integer = 4
            If hasOutcome Then fixedCols += 1
            If hasSuppliedScores Then fixedCols += 1
            If hasExact Then fixedCols += 1

            Dim totalCols As Integer = fixedCols + p
            Dim table(n, totalCols - 1) As Object
            Dim col As Integer = 0

            table(0, col) = "Source Row" : col += 1
            table(0, col) = "Analysis Row" : col += 1
            table(0, col) = "ID" : col += 1
            table(0, col) = "Treatment" : col += 1
            If hasOutcome Then table(0, col) = "Outcome" : col += 1
            If hasSuppliedScores Then table(0, col) = "Supplied Propensity Score" : col += 1
            If hasExact Then table(0, col) = "Exact Group" : col += 1
            For j As Integer = 0 To p - 1
                table(0, col) = input.GetCovariateName(j)
                col += 1
            Next

            For i As Integer = 0 To n - 1
                col = 0
                table(i + 1, col) = If(sourceRowIds IsNot Nothing AndAlso sourceRowIds.Length = n, sourceRowIds(i), i + 1) : col += 1
                table(i + 1, col) = i + 1 : col += 1
                table(i + 1, col) = input.GetId(i) : col += 1
                table(i + 1, col) = input.Treatment(i) : col += 1
                If hasOutcome Then table(i + 1, col) = input.Outcome(i) : col += 1
                If hasSuppliedScores Then table(i + 1, col) = input.SuppliedPropensityScores(i) : col += 1
                If hasExact Then table(i + 1, col) = input.ExactGroupLabels(i) : col += 1
                For j As Integer = 0 To p - 1
                    table(i + 1, col) = input.Covariates(i, j)
                    col += 1
                Next
            Next

            Return table
        End Function
    End Class

End Namespace
