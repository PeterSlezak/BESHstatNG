Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    ''' <summary>Shape/identity/layout projection over existing Core results only.
    ''' Raw and fitted matrices are borrowed for immediate synchronous traversal and
    ''' are never stored in the detached projection. No estimation or geometry.</summary>
    Public NotInheritable Class MultivariateComponentProjectionPolicy
        Private Sub New()
        End Sub
        Private Shared Sub Issue(issues As List(Of RegressionDiagnostic), code As String, message As String)
            If issues.Count < 32 Then issues.Add(New RegressionDiagnostic(code, "projection", message, blocking:=False))
        End Sub
        Private Shared Function Observations(input As MultivariateInputIntent, count As Integer, kind As MultivariateSemanticAxis) As MultivariateAxisMap
            Dim rows = input.SourceRowIds, labels = input.RowLabels, items As New List(Of MultivariateAxisItem)
            For i = 0 To Math.Max(0, count) - 1
                Dim position As Nullable(Of Integer) = If(i < input.RowCount, CType(i, Nullable(Of Integer)), Nothing)
                Dim sourceId As Nullable(Of Integer) = If(position.HasValue AndAlso rows IsNot Nothing AndAlso i < rows.Length, CType(rows(i), Nullable(Of Integer)), Nothing)
                Dim label = If(labels IsNot Nothing AndAlso i < labels.Length, labels(i), Nothing)
                items.Add(New MultivariateAxisItem(i, Nothing, position, sourceId, displayLabel:=label,
                    state:=If(position.HasValue, MultivariateMappingState.Known, MultivariateMappingState.Unavailable)))
            Next
            Return New MultivariateAxisMap(kind, items)
        End Function
        Private Shared Function Variables(input As MultivariateInputIntent, count As Integer, kind As MultivariateSemanticAxis) As MultivariateAxisMap
            Dim columns = input.Columns, items As New List(Of MultivariateAxisItem)
            For j = 0 To Math.Max(0, count) - 1
                Dim known = j < columns.Count
                items.Add(New MultivariateAxisItem(j, If(known, columns(j).Name, Nothing),
                    If(known, CType(j, Nullable(Of Integer)), Nothing), sourceKey:=If(known, columns(j).SourceKey, Nothing),
                    state:=If(known, MultivariateMappingState.Known, MultivariateMappingState.Unavailable)))
            Next
            Return New MultivariateAxisMap(kind, items)
        End Function
        Private Shared Function NamedAxis(kind As MultivariateSemanticAxis, count As Integer, names() As String,
                                          Optional display() As String = Nothing) As MultivariateAxisMap
            Dim items As New List(Of MultivariateAxisItem)
            For j = 0 To Math.Max(0, count) - 1
                Dim known = names IsNot Nothing AndAlso j < names.Length
                items.Add(New MultivariateAxisItem(j, If(known, names(j), Nothing),
                    displayLabel:=If(display IsNot Nothing AndAlso j < display.Length, display(j), Nothing),
                    state:=If(known, MultivariateMappingState.Known, MultivariateMappingState.Unavailable)))
            Next
            Return New MultivariateAxisMap(kind, items)
        End Function
        Private Shared Sub Header(sections As List(Of MultivariateResultSection), role As MultivariateSectionRole, row As Integer, column As Integer,
                                  values() As String, axis As MultivariateSemanticAxis)
            sections.Add(New MultivariateResultSection(role, row, column, New MultivariateArrayShape(1, If(values Is Nothing, 0, values.Length), values IsNot Nothing),
                MultivariateSemanticAxis.PresentationHeaders, axis, values))
        End Sub
        Private Shared Sub Body(sections As List(Of MultivariateResultSection), role As MultivariateSectionRole, row As Integer, column As Integer,
                               shape As MultivariateArrayShape, rowAxis As MultivariateSemanticAxis, columnAxis As MultivariateSemanticAxis)
            sections.Add(New MultivariateResultSection(role, row, column, shape, rowAxis, columnAxis))
        End Sub

        Public Shared Function Pca(input As MultivariateInputIntent, model As PCA, options As MultivariatePcaOptions, original(,) As Object) As MultivariateComponentProjection
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            Dim issues As New List(Of RegressionDiagnostic)(input.Validation.Diagnostics), names = input.VariableNames
            Dim raw = MultivariateArrayShape.Matrix(original), standardized = MultivariateArrayShape.Matrix(model.StandardizedData)
            Dim scores = MultivariateArrayShape.Matrix(model.ReducedDataset), loadings = MultivariateArrayShape.Matrix(model.GetLoadings)
            Dim standardNames = If(names Is Nothing, Nothing, model.StandardizedVarNames(names))
            Dim pcNames = model.PCnames("Reduced_Data_PC"), reducedNames = model.StandardizedVarNames(pcNames)
            Dim axes As New List(Of MultivariateAxisMap) From {
                Observations(input, raw.Rows, MultivariateSemanticAxis.Observations), Observations(input, scores.Rows, MultivariateSemanticAxis.ScoreObservations),
                Variables(input, raw.Columns, MultivariateSemanticAxis.InputVariables), Variables(input, loadings.Rows, MultivariateSemanticAxis.LoadingVariables),
                NamedAxis(MultivariateSemanticAxis.RetainedComponents, scores.Columns, pcNames, reducedNames)}
            Dim eigenCount = If(model.Eigenvalues Is Nothing, 0, model.Eigenvalues.Length)
            Dim eigenIndices = If(eigenCount = 0, Array.Empty(Of Integer)(), model.XaxisComponents)
            axes.Add(NamedAxis(MultivariateSemanticAxis.EigenComponents, eigenCount, eigenIndices.Select(Function(i) i.ToString(CultureInfo.CurrentCulture)).ToArray()))
            If raw.Rows <> standardized.Rows OrElse raw.Columns <> standardized.Columns Then Issue(issues, "multivariate.pca.standardized.shape", "Raw and Core standardized axes differ or are unavailable.")
            If raw.Rows <> scores.Rows OrElse loadings.Rows <> raw.Columns OrElse loadings.Columns <> scores.Columns Then Issue(issues, "multivariate.pca.score.shape", "Core score/loading axes differ from the acquired input axes or are unavailable.")
            If options IsNot Nothing AndAlso (options.Retention = MultivariatePcaRetention.Eigenvalue OrElse options.Retention = MultivariatePcaRetention.Variance) Then Issue(issues, "C18-PCA-THRESHOLD-CAPTION-01", "Existing descriptive > caption and Core >= retention boundary remain unchanged.")
            Dim sections As New List(Of MultivariateResultSection), label = input.RowLabels, groups = input.GroupLabels
            Dim meta = If(groups Is Nothing, 1, 2), first = meta + 1
            axes.Add(NamedAxis(MultivariateSemanticAxis.ObservationIdentity, 1, {If(label Is Nothing, "Row ID", "Record ID")}))
            Header(sections, MultivariateSectionRole.IdentifierHeader, 1, 1, {If(label Is Nothing, "Row ID", "Record ID")}, MultivariateSemanticAxis.ObservationIdentity)
            Body(sections, MultivariateSectionRole.Identifiers, 2, 1, New MultivariateArrayShape(If(label Is Nothing, If(input.SourceRowIds Is Nothing, 0, input.SourceRowIds.Length), label.Length), 1, label IsNot Nothing OrElse input.SourceRowIds IsNot Nothing), MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.ObservationIdentity)
            If groups IsNot Nothing Then
                axes.Add(NamedAxis(MultivariateSemanticAxis.ObservationGrouping, 1, {"Group ID"}))
                Header(sections, MultivariateSectionRole.GroupHeader, 1, 2, {"Group ID"}, MultivariateSemanticAxis.ObservationGrouping)
                Body(sections, MultivariateSectionRole.Groups, 2, 2, New MultivariateArrayShape(groups.Length, 1), MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.ObservationGrouping)
            End If
            Header(sections, MultivariateSectionRole.OriginalHeader, 1, first, names, MultivariateSemanticAxis.InputVariables)
            Body(sections, MultivariateSectionRole.OriginalData, 2, first, raw, MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.InputVariables)
            Dim p = If(names Is Nothing, 0, names.Length)
            Header(sections, MultivariateSectionRole.StandardizedHeader, 1, first + p, standardNames, MultivariateSemanticAxis.InputVariables)
            Body(sections, MultivariateSectionRole.StandardizedData, 2, first + p, standardized, MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.InputVariables)
            Header(sections, MultivariateSectionRole.ReducedHeader, 1, first + 2 * p, reducedNames, MultivariateSemanticAxis.RetainedComponents)
            Body(sections, MultivariateSectionRole.ReducedScores, 2, first + 2 * p, scores, MultivariateSemanticAxis.ScoreObservations, MultivariateSemanticAxis.RetainedComponents)
            Dim plots As New List(Of MultivariatePlotDataProjection)
            For Each request In MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.PCA)
                Select Case request.Kind
                    Case MultivariatePlotKind.Scree
                        plots.Add(New MultivariatePlotDataProjection(request, MultivariateSemanticAxis.EigenComponents, MultivariateSemanticAxis.EigenComponents,
                            New MultivariateArrayShape(eigenCount, 1, model.Eigenvalues IsNot Nothing), New MultivariateArrayShape(eigenCount, 1, model.Eigenvalues IsNot Nothing), True))
                    Case MultivariatePlotKind.Loadings2D, MultivariatePlotKind.Loadings3D
                        plots.Add(New MultivariatePlotDataProjection(request, MultivariateSemanticAxis.LoadingVariables, MultivariateSemanticAxis.RetainedComponents, loadings, New MultivariateArrayShape(0, 0, False), True))
                    Case MultivariatePlotKind.Biplot
                        plots.Add(New MultivariatePlotDataProjection(request, MultivariateSemanticAxis.ScoreObservations, MultivariateSemanticAxis.LoadingVariables, scores, loadings, True))
                    Case Else
                        plots.Add(New MultivariatePlotDataProjection(request, MultivariateSemanticAxis.ScoreObservations, MultivariateSemanticAxis.RetainedComponents, scores, New MultivariateArrayShape(0, 0, False), True))
                End Select
            Next
            Return New MultivariateComponentProjection(input, axes, sections, plots, issues, options)
        End Function

        ''' <summary>Borrowed matrices are passed straight to the synchronous host callback.
        ''' The callback must not mutate them. No writer, workbook, or delegate is retained.</summary>
        Public Shared Sub VisitPca(projection As MultivariateComponentProjection, original(,) As Object, model As PCA,
                                   consume As Action(Of MultivariateResultSection, Object, Boolean))
            For Each section In projection.Sections
                Dim payload As Object = Nothing, tall = False
                Select Case section.Role
                    Case MultivariateSectionRole.Identifiers
                        payload = If(projection.Input.RowLabels Is Nothing, CType(projection.Input.SourceRowIds, Object), projection.Input.RowLabels) : tall = True
                    Case MultivariateSectionRole.Groups : payload = projection.Input.GroupLabels : tall = True
                    Case MultivariateSectionRole.OriginalData : payload = original
                    Case MultivariateSectionRole.StandardizedData : payload = model.StandardizedData
                    Case MultivariateSectionRole.ReducedScores : payload = model.ReducedDataset
                    Case MultivariateSectionRole.StandardizedHeader
                        ' Preserve the original failure point for absent names on malformed callers.
                        payload = If(section.Header, model.StandardizedVarNames(projection.Input.VariableNames))
                    Case Else : payload = section.Header
                End Select
                consume(section, payload, tall)
            Next
        End Sub

        Public Shared Function Mca(transform As MultivariateMcaTransformIntent, model As CA, original(,) As Object) As MultivariateComponentProjection
            If transform Is Nothing Then Throw New ArgumentNullException(NameOf(transform))
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            Dim input = transform.Input, issues As New List(Of RegressionDiagnostic)(input.Validation.Diagnostics)
            Dim raw = MultivariateArrayShape.Matrix(original), design = MultivariateArrayShape.Matrix(model.DesignMatrix), burt = MultivariateArrayShape.Matrix(model.BurtTable)
            Dim categories = model.ColumNames, blocks = model.BurtVarNames, displayCategories = model.rowNames
            Dim q = If(input.VariableNames Is Nothing, 0, input.VariableNames.Length)
            Dim axes As New List(Of MultivariateAxisMap) From {Observations(input, design.Rows, MultivariateSemanticAxis.Observations), Variables(input, q, MultivariateSemanticAxis.InputVariables)}
            Dim owners As New Dictionary(Of String, Nullable(Of Integer))(StringComparer.Ordinal), columns = input.Columns
            For j = 0 To columns.Count - 1
                Dim name = columns(j).Name
                If name Is Nothing Then Continue For
                If owners.ContainsKey(name) Then owners(name) = Nothing Else owners.Add(name, j)
            Next
            Dim catItems As New List(Of MultivariateAxisItem), blockItems As New List(Of MultivariateAxisItem), ambiguous = False
            For j = 0 To design.Columns - 1
                Dim blockName = If(blocks IsNot Nothing AndAlso j < blocks.Length, blocks(j), Nothing)
                Dim owner As Nullable(Of Integer) = Nothing, found = blockName IsNot Nothing AndAlso owners.TryGetValue(blockName, owner)
                Dim state = If(Not found, MultivariateMappingState.Unavailable, If(owner.HasValue, MultivariateMappingState.Known, MultivariateMappingState.Ambiguous))
                Dim key = If(owner.HasValue, columns(owner.Value).SourceKey, Nothing)
                Dim category = If(categories IsNot Nothing AndAlso j < categories.Length, categories(j), Nothing)
                catItems.Add(New MultivariateAxisItem(j, category, owner, sourceKey:=key, displayLabel:=category, state:=state))
                blockItems.Add(New MultivariateAxisItem(j, blockName, owner, sourceKey:=key, displayLabel:=If(displayCategories IsNot Nothing AndAlso j < displayCategories.Length, displayCategories(j), Nothing), state:=state))
                ambiguous = ambiguous OrElse state <> MultivariateMappingState.Known
            Next
            axes.Add(New MultivariateAxisMap(MultivariateSemanticAxis.Categories, catItems))
            axes.Add(New MultivariateAxisMap(MultivariateSemanticAxis.VariableCategoryBlocks, blockItems))
            axes.Add(Factors(model.FactorCount))
            If ambiguous Then Issue(issues, "C18-MCA-CATEGORY-OWNER-01", "Duplicate or unavailable variable names make category source ownership ambiguous; original Core ordering/headers remain intact and no source key is guessed.")
            If raw.Rows <> design.Rows OrElse burt.Rows <> design.Columns OrElse burt.Columns <> design.Columns Then Issue(issues, "multivariate.mca.shape", "Raw/indicator/Burt shapes differ from their expected axis relationship.")
            Dim sections As New List(Of MultivariateResultSection)
            axes.Add(NamedAxis(MultivariateSemanticAxis.ObservationIdentity, 1, {"Row ID"}))
            Header(sections, MultivariateSectionRole.IdentifierHeader, 1, 1, {"Row ID"}, MultivariateSemanticAxis.ObservationIdentity)
            Body(sections, MultivariateSectionRole.Identifiers, 3, 1, New MultivariateArrayShape(If(input.SourceRowIds Is Nothing, 0, input.SourceRowIds.Length), 1, input.SourceRowIds IsNot Nothing), MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.ObservationIdentity)
            Header(sections, MultivariateSectionRole.OriginalHeader, 1, 2, input.VariableNames, MultivariateSemanticAxis.InputVariables)
            Body(sections, MultivariateSectionRole.OriginalData, 3, 2, raw, MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.InputVariables)
            Header(sections, MultivariateSectionRole.DesignCaption, 1, q + 2, {"Design MatrixType:"}, MultivariateSemanticAxis.VariableCategoryBlocks)
            Header(sections, MultivariateSectionRole.DesignVariableHeader, 1, q + 3, blocks, MultivariateSemanticAxis.VariableCategoryBlocks)
            Header(sections, MultivariateSectionRole.DesignCategoryHeader, 2, q + 3, displayCategories, MultivariateSemanticAxis.Categories)
            Body(sections, MultivariateSectionRole.IndicatorData, 3, q + 3, design, MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.Categories)
            Dim rowCount = If(model.RowDistance Is Nothing, 0, model.RowDistance.Length), colCount = If(model.ColDistance Is Nothing, 0, model.ColDistance.Length)
            Dim plots As New List(Of MultivariatePlotDataProjection)
            For Each request In MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.MCA)
                If request.Kind = MultivariatePlotKind.Correspondence Then
                    plots.Add(New MultivariatePlotDataProjection(request, MultivariateSemanticAxis.Observations, MultivariateSemanticAxis.Categories,
                        New MultivariateArrayShape(rowCount, model.FactorCount), New MultivariateArrayShape(colCount, model.FactorCount), False))
                Else
                    plots.Add(New MultivariatePlotDataProjection(request, MultivariateSemanticAxis.Categories, MultivariateSemanticAxis.CorrespondenceFactors,
                        New MultivariateArrayShape(colCount, 1, request.Axis < model.FactorCount), New MultivariateArrayShape(0, 0, False), False))
                End If
            Next
            Return New MultivariateComponentProjection(input, axes, sections, plots, issues, mcaTransform:=transform, burtShape:=burt)
        End Function
        Public Shared Sub VisitMca(projection As MultivariateComponentProjection, original(,) As Object, model As CA,
                                   consume As Action(Of MultivariateResultSection, Object, Boolean))
            For Each section In projection.Sections
                Dim payload As Object = Nothing, tall = False
                Select Case section.Role
                    Case MultivariateSectionRole.Identifiers : payload = projection.Input.SourceRowIds : tall = True
                    Case MultivariateSectionRole.OriginalData : payload = original
                    Case MultivariateSectionRole.IndicatorData : payload = model.DesignMatrix
                    Case Else : payload = section.Header
                End Select
                consume(section, payload, tall)
            Next
        End Sub
        Private Shared Function Factors(count As Integer) As MultivariateAxisMap
            Return NamedAxis(MultivariateSemanticAxis.CorrespondenceFactors, count,
                Enumerable.Range(0, count).Select(Function(i) "Axis " & (i + 1).ToString(CultureInfo.CurrentCulture)).ToArray())
        End Function

        Public Shared Function SimpleCa(input As MultivariateContingencyInput, model As CA, counts(,) As Integer) As MultivariateComponentProjection
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            Dim shape = MultivariateArrayShape.Matrix(counts), rows = input.SourceRowIds
            Dim coreRows = model.rowNames, coreColumns = model.ColumNames, rowItems As New List(Of MultivariateAxisItem), columnItems As New List(Of MultivariateAxisItem)
            For i = 0 To shape.Rows - 1
                rowItems.Add(New MultivariateAxisItem(i, If(coreRows IsNot Nothing AndAlso i < coreRows.Length, coreRows(i), Nothing), i,
                    If(rows IsNot Nothing AndAlso i < rows.Length, CType(rows(i), Nullable(Of Integer)), Nothing)))
            Next
            For j = 0 To shape.Columns - 1
                columnItems.Add(New MultivariateAxisItem(j, If(coreColumns IsNot Nothing AndAlso j < coreColumns.Length, coreColumns(j), Nothing), j + input.FirstCountSourceColumn))
            Next
            Dim intent As New MultivariateInputIntent(MultivariateAnalysisKind.CA, shape.Rows, shape.Columns, coreColumns, rows, input.RowLabels)
            Dim axes As New List(Of MultivariateAxisMap) From {New MultivariateAxisMap(MultivariateSemanticAxis.ContingencyRows, rowItems), New MultivariateAxisMap(MultivariateSemanticAxis.ContingencyColumns, columnItems), Factors(model.FactorCount)}
            Dim sections = {New MultivariateResultSection(MultivariateSectionRole.ContingencyBody, 3, 2, shape, MultivariateSemanticAxis.ContingencyRows, MultivariateSemanticAxis.ContingencyColumns, coreColumns)}
            Dim plots As New List(Of MultivariatePlotDataProjection)
            For Each request In MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.CA)
                If request.Kind = MultivariatePlotKind.Correspondence Then
                    plots.Add(New MultivariatePlotDataProjection(request, MultivariateSemanticAxis.ContingencyRows, MultivariateSemanticAxis.ContingencyColumns,
                        New MultivariateArrayShape(shape.Rows, model.FactorCount), New MultivariateArrayShape(shape.Columns, model.FactorCount), True))
                Else
                    plots.Add(New MultivariatePlotDataProjection(request, If(request.RowContribution, MultivariateSemanticAxis.ContingencyRows, MultivariateSemanticAxis.ContingencyColumns),
                        MultivariateSemanticAxis.CorrespondenceFactors, New MultivariateArrayShape(If(request.RowContribution, shape.Rows, shape.Columns), 1, request.Axis < model.FactorCount), New MultivariateArrayShape(0, 0, False), request.RowContribution))
                End If
            Next
            Return New MultivariateComponentProjection(intent, axes, sections, plots, input.Validation.Diagnostics, contingency:=input)
        End Function
        Public Shared Function CaReport(projection As MultivariateComponentProjection, counts(,) As Integer, coreTables As IEnumerable(Of ResultTable)) As MultivariateReport
            Dim table As New ResultTable
            table.AddTitle("Analyzed contingency table") : table.SetBody(counts)
            table.AddHeaderLeftRow(projection.Axis(MultivariateSemanticAxis.ContingencyRows).Items.Select(Function(i) i.Name).ToArray())
            table.AddHeaderTopRow(projection.Axis(MultivariateSemanticAxis.ContingencyColumns).Items.Select(Function(i) i.Name).ToArray())
            Dim tables As New List(Of ResultTable) From {table} : tables.AddRange(coreTables)
            Return projection.ToReport(tables)
        End Function
    End Class
End Namespace
