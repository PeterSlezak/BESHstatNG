Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    ''' <summary>Input preparation only: no fitting, matching, control limits or chart rendering.</summary>
    Public NotInheritable Class AdvancedInputParsers
        Private Sub New()
        End Sub

        Public Shared Function ParseMatrix(input As SpreadsheetRangeData,
                  Optional options As SpreadsheetMatrixParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetMatrixInput)
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            Return Finish(Read(input, If(options, New SpreadsheetMatrixParseOptions()), -1, -1, -1,
                               False, False, False, False, Nothing, issues), issues)
        End Function

        Public Shared Function ParseClassification(input As SpreadsheetRangeData,
                  Optional options As SpreadsheetClassificationParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetClassificationInput)
            Dim config As SpreadsheetClassificationParseOptions = If(options, New SpreadsheetClassificationParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If config.ClassColumnIndex < 0 OrElse config.MinimumClasses < 2 Then
                Add(issues, Nothing, "classification.options", "A class column and at least two classes are required.")
                Return Finish(Of SpreadsheetClassificationInput)(Nothing, issues)
            End If
            Dim matrix As SpreadsheetMatrixInput = Read(input, config, config.ClassColumnIndex, -1, -1,
                False, False, False, False, Nothing, issues, config.IgnoreCategoryCase)
            If matrix Is Nothing Then Return Finish(Of SpreadsheetClassificationInput)(Nothing, issues)
            Dim levels As New List(Of SpreadsheetCategoryValue)()
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each row As SpreadsheetAdvancedRow In matrix.Rows
                Dim level As SpreadsheetCategoryValue = row.LabelLevel
                If seen.Add(level.Key) Then levels.Add(level)
            Next
            If levels.Count < config.MinimumClasses Then Add(issues, Nothing, "classification.classes", "Too few distinct class labels among retained rows.")
            Return Finish(New SpreadsheetClassificationInput(matrix, levels), issues)
        End Function

        Public Shared Function ParsePsm(input As SpreadsheetRangeData,
                  Optional options As SpreadsheetPsmParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetPsmInput)
            Dim config As SpreadsheetPsmParseOptions = If(options, New SpreadsheetPsmParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If config.TreatmentColumnIndex < 0 OrElse config.TreatedLevel Is Nothing OrElse config.ControlLevel Is Nothing OrElse
               config.TreatedLevel.Key = config.ControlLevel.Key Then
                Add(issues, Nothing, "psm.options", "Distinct explicit treatment and control levels and a treatment column are required.")
                Return Finish(Of SpreadsheetPsmInput)(Nothing, issues)
            End If
            Dim matrix As SpreadsheetMatrixInput = Read(input, config, config.TreatmentColumnIndex, -1, -1,
                False, False, False, False, config, issues)
            If matrix Is Nothing Then Return Finish(Of SpreadsheetPsmInput)(Nothing, issues)
            Dim treated As Integer = 0
            For Each row As SpreadsheetAdvancedRow In matrix.Rows
                If row.IsTreated.Value Then treated += 1
            Next
            If treated = 0 OrElse treated = matrix.RowCount Then Add(issues, Nothing, "psm.arms", "Both treatment arms must occur among complete rows.")
            Return Finish(New SpreadsheetPsmInput(matrix, config.TreatedLevel, config.ControlLevel), issues)
        End Function

        Public Shared Function ParseSpc(input As SpreadsheetRangeData,
                  Optional options As SpreadsheetSpcParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetSpcInput)
            Dim config As SpreadsheetSpcParseOptions = If(options, New SpreadsheetSpcParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            Dim matrix As SpreadsheetMatrixInput = Read(input, config, -1, config.SubgroupColumnIndex, config.OrderColumnIndex,
                True, config.OrderColumnIndex >= 0, config.InterpretOrderAsDate, False, Nothing, issues)
            Return Finish(If(matrix Is Nothing, Nothing, New SpreadsheetSpcInput(matrix)), issues)
        End Function

        Public Shared Function ParseChartSource(input As SpreadsheetRangeData,
                  Optional options As SpreadsheetChartSourceParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetChartSourceInput)
            Dim config As SpreadsheetChartSourceParseOptions = If(options, New SpreadsheetChartSourceParseOptions())
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            Dim matrix As SpreadsheetMatrixInput = Read(input, config, -1, config.GroupColumnIndex, config.XColumnIndex,
                config.RequireNumericX OrElse config.RequireAscendingX OrElse config.InterpretXAsDate,
                config.RequireAscendingX, config.InterpretXAsDate, True, Nothing, issues)
            Return Finish(If(matrix Is Nothing, Nothing, New SpreadsheetChartSourceInput(matrix)), issues)
        End Function

        Private Shared Function Read(input As SpreadsheetRangeData, config As SpreadsheetMatrixParseOptions,
                    labelIndex As Integer, groupIndex As Integer, coordinateIndex As Integer,
                    numericCoordinate As Boolean, ascending As Boolean, asDate As Boolean, orderWithinGroups As Boolean,
                    treatment As SpreadsheetPsmParseOptions, issues As List(Of SpreadsheetValidationIssue),
                    Optional ignoreLabelCase As Boolean = False) As SpreadsheetMatrixInput
            If config.CellPolicy Is Nothing OrElse config.MinimumRows < 1 OrElse config.MinimumColumns < 1 OrElse
               (config.Missing <> SpreadsheetIncompleteObservationPolicy.Reject AndAlso config.Missing <> SpreadsheetIncompleteObservationPolicy.Skip) OrElse
               (config.Header <> SpreadsheetHeaderMode.Required AndAlso config.Header <> SpreadsheetHeaderMode.None) OrElse
               (asDate AndAlso coordinateIndex < 0) Then
                Add(issues, Nothing, "advanced.options", "Invalid policies, dimensions or date coordinate role; choose an explicit header mode.")
                Return Nothing
            End If
            Dim parsed As SpreadsheetTypedParseResult(Of SpreadsheetRectangularDataFrameInput) =
                RectangularCategoricalParsers.ParseDataFrame(input, New SpreadsheetDataFrameParseOptions With {.Header = config.Header})
            issues.AddRange(parsed.Validation.Issues)
            If Not parsed.Success Then Return Nothing
            Dim frame As SpreadsheetRectangularDataFrameInput = parsed.Data
            Dim used As New HashSet(Of Integer)()
            For Each index As Integer In New Integer() {labelIndex, groupIndex, coordinateIndex}
                If index < -1 OrElse index >= frame.ColumnCount OrElse (index >= 0 AndAlso Not used.Add(index)) Then
                    Add(issues, Nothing, "advanced.roles", "Roles must be distinct valid selected columns.")
                    Return Nothing
                End If
            Next
            Dim indices As New List(Of Integer)()
            If config.NumericColumnIndices Is Nothing Then
                For c As Integer = 0 To frame.ColumnCount - 1
                    If Not used.Contains(c) Then indices.Add(c)
                Next
            Else
                For Each c As Integer In config.NumericColumnIndices
                    If c < 0 OrElse c >= frame.ColumnCount OrElse Not used.Add(c) Then
                        Add(issues, Nothing, "advanced.roles", "Numeric columns must be unique and cannot overlap role columns.")
                        Return Nothing
                    End If
                    indices.Add(c)
                Next
            End If
            If indices.Count < config.MinimumColumns Then
                Add(issues, Nothing, "matrix.columns", "Too few numeric columns.")
                Return Nothing
            End If
            Dim columns As New List(Of SpreadsheetFrameColumn)()
            For Each c As Integer In indices
                columns.Add(frame.Columns(c))
            Next
            Dim rows As New List(Of SpreadsheetAdvancedRow)()
            Dim last As New Dictionary(Of String, Double)(StringComparer.Ordinal)
            For r As Integer = 0 To frame.RowCount - 1
                Dim values As New List(Of SpreadsheetNumericObservation)()
                Dim missingCell As SpreadsheetFrameCell = Nothing
                Dim invalid As Boolean = False
                For Each c As Integer In indices
                    Dim cell As SpreadsheetFrameCell = frame.Cell(r, c)
                    Dim evaluation As SpreadsheetCellEvaluation = Evaluate(cell, config, issues)
                    If evaluation Is Nothing Then
                        invalid = True
                    ElseIf evaluation.IsMissing Then
                        If missingCell Is Nothing Then missingCell = cell
                    ElseIf Not evaluation.HasNumericValue Then
                        Add(issues, cell, "cell.not-numeric", "A finite numeric measurement is required.")
                        invalid = True
                    Else
                        values.Add(New SpreadsheetNumericObservation(evaluation.NumericValue, cell.SelectedRow, cell.SelectedColumn,
                                                                    cell.WorksheetRow, cell.WorksheetColumn))
                    End If
                Next
                Dim label As SpreadsheetFrameCell = Nothing
                Dim group As SpreadsheetFrameCell = Nothing
                Dim coordinate As SpreadsheetFrameCell = Nothing
                Dim coordinateValue As Nullable(Of Double) = Nothing
                Dim coordinateDate As Nullable(Of DateTime) = Nothing
                Dim treated As Nullable(Of Boolean) = Nothing
                Dim labelLevel As SpreadsheetCategoryValue = Nothing
                For Each c As Integer In New Integer() {labelIndex, groupIndex, coordinateIndex}
                    If c < 0 Then Continue For
                    Dim cell As SpreadsheetFrameCell = frame.Cell(r, c)
                    Dim evaluation As SpreadsheetCellEvaluation = Evaluate(cell, config, issues)
                    If evaluation Is Nothing Then
                        invalid = True
                        Continue For
                    ElseIf evaluation.IsMissing Then
                        If missingCell Is Nothing Then missingCell = cell
                        Continue For
                    End If
                    If c = labelIndex Then
                        label = cell
                        labelLevel = New SpreadsheetCategoryValue(cell.ToCellValue(), ignoreLabelCase)
                        If treatment IsNot Nothing Then
                            Dim level As SpreadsheetCategoryValue = labelLevel
                            If level.Key = treatment.TreatedLevel.Key Then
                                treated = True
                            ElseIf level.Key = treatment.ControlLevel.Key Then
                                treated = False
                            Else
                                Add(issues, cell, "psm.treatment", "Treatment must match one of the two declared typed levels.")
                                invalid = True
                            End If
                        End If
                    ElseIf c = groupIndex Then
                        group = cell
                    Else
                        coordinate = cell
                        If numericCoordinate AndAlso Not evaluation.HasNumericValue Then
                            Add(issues, cell, "advanced.coordinate-type", "A finite numeric coordinate is required.")
                            invalid = True
                        ElseIf evaluation.HasNumericValue Then
                            coordinateValue = evaluation.NumericValue
                        End If
                        If asDate AndAlso coordinateValue.HasValue Then
                            Try
                                Dim dateEvaluation As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(
                                    cell.ToCellValue(), config.CellPolicy, input.DateSystem, True, input.DateSerialOrigin)
                                If Not dateEvaluation.HasDateValue Then Throw New ArgumentException("A verified date origin is required.")
                                coordinateDate = dateEvaluation.DateValue
                            Catch ex As ArgumentException
                                Add(issues, cell, "advanced.date", ex.Message)
                                invalid = True
                            End Try
                        End If
                    End If
                Next
                ' Check every role even when another field is missing; invalid data is never silently skipped.
                If invalid Then Continue For
                If missingCell IsNot Nothing Then
                    Add(issues, missingCell, "advanced.missing", "Incomplete source row.", config.Missing = SpreadsheetIncompleteObservationPolicy.Skip)
                    Continue For
                End If
                If coordinateIndex < 0 Then coordinateValue = CDbl(values(0).SelectedRow)
                If ascending Then
                    Dim key As String = If(group Is Nothing OrElse Not orderWithinGroups, String.Empty, New SpreadsheetCategoryValue(group.ToCellValue()).Key)
                    Dim previous As Double = 0
                    If last.TryGetValue(key, previous) AndAlso coordinateValue.Value <= previous Then
                        Add(issues, coordinate, "advanced.order", "Coordinates must strictly increase in source order within each group.")
                        Continue For
                    End If
                    last(key) = coordinateValue.Value
                End If
                rows.Add(New SpreadsheetAdvancedRow(values, label, group, coordinate, coordinateValue, coordinateDate, treated, labelLevel))
            Next
            If rows.Count < config.MinimumRows Then Add(issues, Nothing, "matrix.rows", "Too few complete source rows.")
            If config.RequireSquare AndAlso rows.Count <> columns.Count Then Add(issues, Nothing, "matrix.square", "The retained numeric matrix must be square.")
            If New SpreadsheetValidationReport(issues).HasErrors Then Return Nothing
            Return New SpreadsheetMatrixInput(frame, columns, rows, input.DateSerialOrigin)
        End Function

        Private Shared Function Evaluate(cell As SpreadsheetFrameCell, config As SpreadsheetMatrixParseOptions,
                                         issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetCellEvaluation
            Try
                Dim evaluation As SpreadsheetCellEvaluation = SpreadsheetCellValuePolicy.Evaluate(cell.ToCellValue(), config.CellPolicy)
                If evaluation.IsError AndAlso Not evaluation.IsMissing Then
                    Add(issues, cell, "cell.spreadsheet-error", "Spreadsheet error cannot fill an analysis role; native metadata remains in the source snapshot.")
                    Return Nothing
                End If
                Return evaluation
            Catch ex As ArgumentException
                Add(issues, cell, "cell.invalid-value", ex.Message)
                Return Nothing
            End Try
        End Function

        Private Shared Sub Add(issues As List(Of SpreadsheetValidationIssue), cell As SpreadsheetFrameCell,
                               code As String, message As String, Optional warning As Boolean = False)
            issues.Add(New SpreadsheetValidationIssue(code, If(warning, SpreadsheetValidationSeverity.Warning, SpreadsheetValidationSeverity.[Error]),
                message, code, If(cell Is Nothing, 0, cell.SelectedRow), If(cell Is Nothing, 0, cell.SelectedColumn),
                If(cell Is Nothing, 0, cell.WorksheetRow), If(cell Is Nothing, 0, cell.WorksheetColumn)))
        End Sub

        Private Shared Function Finish(Of T As SpreadsheetAnalysisInput)(data As T, issues As List(Of SpreadsheetValidationIssue)) As SpreadsheetTypedParseResult(Of T)
            Dim report As New SpreadsheetValidationReport(issues)
            Return New SpreadsheetTypedParseResult(Of T)(If(report.HasErrors, Nothing, data), report)
        End Function
    End Class
End Namespace
