Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Spc23Fixture
    Public Class Input
        Public Chart As SpcChartType
        Public Data As Double(,)
        Public Names As String()
        Public Ids As String()
        Public Labels As String()
        Public Sequences As Double()
        Public Mean As Double()
        Public Covariance As Double(,)
        Public FirstRow As Integer
        Public Grouped As Boolean
        Public Options As Spc23LegacyPreparation.RawOptions
        Public Legacy As Spc23LegacyPreparation
        Public Context As Spc23LegacyPreparation.InputRowContext
        Public Map As SpcMultivariatePointMap
    End Class

    Public Shared Function Options(raw As Spc23LegacyPreparation.RawOptions) As SpcMultivariateOptions
        Return New SpcMultivariateOptions(missingValuePolicy:=raw.MissingValuePolicy, controlLimitAlpha:=raw.Alpha, useLowerHotellingLimit:=raw.LowerLimit,
            covarianceRegularization:=raw.Ridge, allowPseudoInverse:=raw.AllowPseudo, pcaUseCorrelationMatrix:=raw.Correlation,
            pcaComponentCount:=If(raw.FixedComponents, CType(raw.Components, Integer?), Nothing), pcaCumulativeVariance:=raw.CumulativePercent / 100.0R,
            mewmaLambda:=raw.Lambda, mewmaControlLimit:=If(raw.SuppliedMewma, CType(raw.MewmaLimit, Double?), Nothing), mcusumReferenceValue:=raw.K, mcusumDecisionInterval:=raw.H,
            generalizedVarianceSigmaMultiplier:=If(raw.SuppliedGv, CType(raw.GvSigma, Double?), Nothing), resetAtStageBoundary:=raw.ResetStage, resetAtPhaseBoundary:=raw.ResetPhase, resetAfterSignal:=raw.ResetSignal,
            requestLabel:=raw.RequestLabel, sequenceGapBehavior:=raw.Gap)
    End Function

    Public Shared Function InputFor(id As Integer) As Input
        Dim mode = id \ 6, chart = Spc23Data.Charts(id Mod 6)
        Dim x = Spc23Data.Measurements(), n = x.GetLength(0)
        Dim grouped = chart = SpcChartType.GeneralizedVariance OrElse (chart = SpcChartType.HotellingT2 AndAlso mode Mod 2 = 1)
        If mode = 53 Then
            x = New Double(,) {{50, 100, 20}} : n = 1
        End If
        Dim ids = If(grouped, Enumerable.Range(0, n).Select(Function(i) "G" & (i \ If(mode = 54, 2, 5)).ToString(CultureInfo.InvariantCulture)).ToArray(), Nothing)
        If mode = 17 AndAlso grouped Then ids = Enumerable.Range(0, n).Select(Function(i) If(i Mod 2 = 0, " A ", "a")).ToArray()
        Dim pointIndices = If(grouped, Enumerable.Range(0, n).Select(Function(i) If(mode = 17, i Mod 2, i \ If(mode = 54, 2, 5))).ToArray(), Enumerable.Range(0, n).ToArray())
        Dim labels = pointIndices.Select(Function(i) "same Ω").ToArray(), sequences = pointIndices.Select(Function(i) CDbl(i Mod 7 - 2)).ToArray()
        If mode = 6 Then labels = Nothing
        If mode = 7 Then sequences = Nothing
        If mode = 8 AndAlso sequences IsNot Nothing Then sequences(0) = Double.NaN
        If mode = 18 AndAlso grouped Then ids(2) = " "
        If mode = 19 AndAlso grouped Then labels(1) = "different"
        If mode = 20 AndAlso grouped Then sequences(1) += 0.5R
        If mode = 33 OrElse mode = 56 Then x(Math.Min(5, n - 1), 1) = Double.NaN
        If mode = 34 Then x(Math.Min(5, n - 1), 1) = Double.PositiveInfinity
        If mode = 35 OrElse mode = 36 Then
            For row = 0 To n - 1 : For column = 0 To 2 : x(row, column) = If(mode = 35, Double.NaN, 1.0R) : Next : Next
        End If
        If mode = 37 Then
            For row = 0 To n - 1 : x(row, 2) = x(row, 0) + x(row, 1) : Next
        End If
        Dim names = New String() {"Temperature_C", "Pressure_kPa", "Flow_L_min"}
        If mode = 9 Then names = {" A ", "A", "Ω"}
        If mode = 10 Then names = {"", " ", "μ"}
        If mode = 11 Then
            For row = 0 To n - 1
                Dim temp = x(row, 0) : x(row, 0) = x(row, 2) : x(row, 2) = temp
            Next
            Array.Reverse(names)
        End If
        Dim raw As New Spc23LegacyPreparation.RawOptions With {
            .ModelSource = CType(mode Mod 2, SpcMultivariateModelSource), .MissingValuePolicy = CType((mode \ 2) Mod 2, SpcMissingValuePolicy),
            .Alpha = If(mode Mod 3 = 0, 0.0027R, 0.01R), .LowerLimit = chart = SpcChartType.HotellingT2 AndAlso Not grouped AndAlso mode Mod 2 = 0,
            .AllowPseudo = chart <> SpcChartType.GeneralizedVariance AndAlso mode Mod 3 <> 1, .Ridge = If(mode Mod 5 = 0, 0.00001R, 0.0R),
            .Correlation = mode Mod 2 = 1, .FixedComponents = mode Mod 3 = 0, .Components = 1, .CumulativePercent = If(mode Mod 4 = 0, 90.0R, 95.0R),
            .Lambda = If(mode Mod 5 = 0, 0.1R, 0.2R), .SuppliedMewma = mode Mod 2 = 1, .MewmaLimit = 12.0R,
            .K = If(mode Mod 3 = 0, 0.5R, 0.25R), .H = If(mode Mod 3 = 0, 5.5R, 4.0R), .SuppliedGv = mode Mod 2 = 1, .GvSigma = 3.0R,
            .ResetStage = mode Mod 2 = 0, .ResetPhase = mode Mod 3 <> 0, .ResetSignal = mode Mod 4 = 1, .Gap = CType(mode Mod 2, SpcSequenceGapBehavior), .RequestLabel = " multivariate Ω "}
        Dim mean As Double() = If(raw.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters AndAlso chart <> SpcChartType.GeneralizedVariance, New Double() {50, 100, 20}, Nothing)
        Dim covariance As Double(,) = If(raw.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters, New Double(,) {{1, 0.2R, 0.1R}, {0.2R, 2, 0.15R}, {0.1R, 0.15R, 0.8R}}, Nothing)
        If mode >= 29 AndAlso mode <= 32 Then
            raw.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters
            mean = If(chart = SpcChartType.GeneralizedVariance, Nothing, New Double() {50, 100, 20})
            covariance = New Double(,) {{1, 0.2R, 0.1R}, {0.2R, 2, 0.15R}, {0.1R, 0.15R, 0.8R}}
            If mode = 29 Then mean = {1, 2}
            If mode = 30 Then covariance = New Double(,) {{1, 0}, {0, 1}}
            If mode = 31 Then covariance(1, 1) = -1
            If mode = 32 Then covariance(1, 1) = Double.NaN
        End If
        Select Case mode
            Case 38 : raw.FixedComponents = True : raw.Components = 0
            Case 39 : raw.FixedComponents = True : raw.Components = 4
            Case 40 : raw.CumulativePercent = 0
            Case 41 : raw.CumulativePercent = 101
            Case 42 : raw.Lambda = 0
            Case 43 : raw.Lambda = 1
            Case 44 : raw.Lambda = 0.000001R
            Case 45 : raw.SuppliedMewma = True : raw.MewmaLimit = -1
            Case 46 : raw.SuppliedGv = True : raw.GvSigma = 0
            Case 47 : raw.Alpha = 0
            Case 48 : raw.Alpha = 1
            Case 49 : raw.Ridge = -0.01R
            Case 50 : raw.K = -0.1R
            Case 51 : raw.H = 0
            Case 52 : raw.ModelSource = SpcMultivariateModelSource.EstimateFromPhaseI : mean = Nothing : covariance = Nothing
            Case 56 : raw.MissingValuePolicy = SpcMissingValuePolicy.OmitPoint
        End Select
        Return New Input With {.Chart = chart, .Data = x, .Names = names, .Ids = ids, .Labels = labels, .Sequences = sequences, .FirstRow = If(mode Mod 2 = 0, 1, 2), .Grouped = grouped, .Options = raw, .Mean = mean, .Covariance = covariance, .Legacy = New Spc23LegacyPreparation()}
    End Function

    Public Shared Sub FillGrids(input As Input, mode As Integer)
        Dim f = input.Legacy, count = input.Context.PointCount
        If mode <> 57 Then
            If count < 2 OrElse mode = 52 Then
                f.AddStageRow("Baseline", 1, count, If(mode = 52, SpcPhase.PhaseII, SpcPhase.PhaseI))
            Else
                Dim split = count * 2 \ 3
                f.AddStageRow("Baseline", 1, split, SpcPhase.PhaseI)
                f.AddStageRow(If(mode = 21, "baseline", "Monitoring"), If(mode = 22, split, split + If(mode = 23, 2, 1)), If(mode = 24, count + 1, count), SpcPhase.PhaseII)
            End If
        End If
        Dim point = If(mode = 25, 0, If(mode = 26, count + 1, Math.Min(3, count)))
        Dim scope = If(mode = 27, "invalid", If(mode Mod 3 = 0, "Parameter estimation", If(mode Mod 3 = 1, "Signal evaluation", "Parameter estimation and signal evaluation")))
        f.dgvExclusions.Rows.Add(New Spc23LegacyPreparation.DataGridViewRow(point, scope, "reason Ω"))
        If mode = 28 Then
            f.dgvExclusions.Rows.Add(New Spc23LegacyPreparation.DataGridViewRow(point, "Signal evaluation", "second"))
            f.dgvExclusions.Rows.Add(New Spc23LegacyPreparation.DataGridViewRow(point, "Parameter estimation", "reason Ω"))
        End If
        If mode = 55 Then
            For i = 1 To count
                f.dgvExclusions.Rows.Add(New Spc23LegacyPreparation.DataGridViewRow(i, "Parameter estimation", "all"))
            Next
        End If
    End Sub

    'This is the native parsing seam's exact call order, source-checked against Ui22.
    Public Shared Function Assignments(form As Spc23LegacyPreparation, count As Integer) As SpcMultivariatePointAssignments
        Dim builder As New SpcMultivariateStageBuilder(count)
        For Each row In form.dgvStages.Rows
            If row.IsNewRow OrElse Spc23LegacyPreparation.GridRowIsBlank(row) Then Continue For
            Dim id = Spc23LegacyPreparation.CellText(row, form.colStageID.Index)
            builder.BeginStage(id)
            Dim first = Spc23LegacyPreparation.RequiredIntegerCell(row, form.colStageFirstPoint.Index, "First point")
            Dim last = Spc23LegacyPreparation.RequiredIntegerCell(row, form.colStageLastPoint.Index, "Last point")
            builder.ValidateRange(id, first, last)
            Dim phase = SpcMultivariatePhasePolicy.ParsePhase(Spc23LegacyPreparation.CellText(row, form.colStagePhase.Index))
            builder.Assign(id, first, last, phase)
        Next
        Return builder.Finish()
    End Function
    Public Shared Function Exclusions(form As Spc23LegacyPreparation, count As Integer) As SpcMultivariatePointExclusions
        Dim builder As New SpcMultivariateExclusionBuilder(count)
        For Each row In form.dgvExclusions.Rows
            If row.IsNewRow OrElse Spc23LegacyPreparation.GridRowIsBlank(row) Then Continue For
            Dim point = Spc23LegacyPreparation.RequiredIntegerCell(row, form.colExclusionPoint.Index, "Exclusion point")
            Dim index = builder.CoreIndex(point)
            builder.IncludeScope(index, Spc23LegacyPreparation.ParseExclusionScope(Spc23LegacyPreparation.CellText(row, form.colExclusionScope.Index)))
            builder.IncludeReason(index, Spc23LegacyPreparation.CellText(row, form.colExclusionReason.Index))
        Next
        Return builder.Finish()
    End Function

    Public Shared Function Prepare(input As Input, portable As Boolean) As SpcMultivariateRequest
        Dim n = input.Data.GetLength(0), first = input.FirstRow
        Dim labels = If(input.Labels Is Nothing, Nothing, CType(input.Labels.Clone(), String())), sequence = If(input.Sequences Is Nothing, Nothing, CType(input.Sequences.Clone(), Double()))
        Dim phases As SpcPhase(), stages As String(), scopes As SpcExclusionScope(), reasons As String(), sources As Integer()
        If portable Then
            input.Map = SpcMultivariatePointMapPolicy.FromWorksheetRows(first, first + n - 1, input.Grouped, input.Ids)
            If input.Grouped Then input.Map.NormalizeGroupedMetadata(labels, sequence)
            Dim rows = input.Map.Expand(Assignments(input.Legacy, input.Map.PointCount), Exclusions(input.Legacy, input.Map.PointCount))
            phases = rows.Phases.ToArray() : stages = rows.StageIds.ToArray() : scopes = rows.ExclusionScopes.ToArray() : reasons = rows.ExclusionReasons.ToArray() : sources = rows.SourceRowIndices.ToArray()
            Dim model As New SpcMultivariateModelIntent(input.Names, input.Options.ModelSource, input.Mean, input.Covariance)
            Return SpcMultivariateRequestPolicy.Create(input.Chart, input.Data, model, Options(input.Options), input.Ids, labels, phases, stages, sequence, sources, scopes, reasons)
        End If
        Dim context = Spc23LegacyPreparation.Map(first, first + n - 1, input.Grouped, input.Ids)
        If input.Grouped Then Spc23LegacyPreparation.NormalizeGroupedMetadata(labels, sequence, context)
        Dim oldAssignments = input.Legacy.BuildPointAssignments(context.PointCount), oldExclusions = input.Legacy.BuildPointExclusions(context.PointCount)
        Dim expanded = Spc23LegacyPreparation.Expand(context, oldAssignments, oldExclusions)
        Return Spc23LegacyPreparation.Compose(input.Chart, input.Data, input.Names, input.Ids, labels, expanded.Phases, expanded.Stages, sequence, expanded.SourceRows, expanded.Scopes, expanded.Reasons, input.Mean, input.Covariance, input.Options)
    End Function

    Public Shared Function CompareResult(result As SpcMultivariateFitResult) As XElement
        Dim context = SpcMultivariateResultAxisPolicy.Project(result)
        Assert.AreSame(result.Model, context.Model)
        Assert.AreEqual(Spc23Fixture.Hash(result.Panels), Spc23Fixture.Hash(context.Panels.ToArray()))
        Assert.AreEqual(Spc23Fixture.Hash(result.Diagnostics), Spc23Fixture.Hash(context.Diagnostics.ToArray()))
        For Each onlySignal In {False, True}
            Dim before = Spc23LegacyPreparation.SelectedDiagnostics(result, onlySignal), after = context.SelectDiagnostics(onlySignal)
            Assert.AreEqual(before.Count, after.Count)
            For i = 0 To before.Count - 1 : Assert.AreSame(before(i), after(i)) : Next
        Next
        Dim node As New XElement("result", New XAttribute("chart", result.Request.ChartType), New XAttribute("effective_dimension", context.Model.EffectiveDimension), New XAttribute("pseudo_inverse", context.Model.UsedPseudoInverse), New XAttribute("retained", context.Model.RetainedComponentCount), New XAttribute("baseline_observations", context.Model.BaselineObservationCount), New XAttribute("baseline_groups", context.Model.BaselineSubgroupCount), New XAttribute("covariance_df", context.Model.CovarianceDegreesOfFreedom), New XAttribute("model_source", context.Model.Source), New XElement("model_sha256", Spc23Fixture.Hash(result.Model)), New XElement("diagnostics_sha256", Spc23Fixture.Hash(result.Diagnostics)), New XElement("warnings", result.Warnings.Select(Function(w) New XElement("warning", w))))
        CollectionAssert.AreEqual(result.Request.VariableNames, context.VariableNames.ToArray())
        node.Add(New XElement("variable_names", context.VariableNames.Select(Function(name) New XElement("name", name))))
        For Each panel In context.Panels
            Dim oldInput = Spc23LegacyPreparation.RenderingInput(panel), newInput = SpcMultivariateResultAxisPolicy.RenderingInput(panel)
            Assert.AreEqual(Spc23Fixture.Hash(oldInput), Spc23Fixture.Hash(newInput))
            Dim p As New XElement("panel", New XAttribute("type", panel.PanelType), New XAttribute("display", panel.DisplayName), New XAttribute("value_axis", panel.ValueAxisTitle), New XAttribute("signals", panel.Signals.Length), New XElement("rendering_input_sha256", Spc23Fixture.Hash(newInput)))
            For Each point In panel.Points
                p.Add(New XElement("point", New XAttribute("index", point.PointIndex), New XAttribute("display", point.PointIndex + 1), New XAttribute("label", point.Label), New XAttribute("stage", point.StageId), New XAttribute("phase", point.Phase), New XAttribute("sequence", Spc21Fixture.Text(point.SequenceValue)), New XAttribute("value", Bits(point.Value)), New XAttribute("cl", Bits(point.CenterLine)), New XAttribute("lcl", Bits(point.LowerControlLimit)), New XAttribute("ucl", Bits(point.UpperControlLimit)), New XAttribute("finite", point.HasFiniteValue), New XAttribute("signalled", point.IsSignalled), New XAttribute("explicit_exclusion", point.IsExplicitlyExcluded), New XAttribute("estimation", point.IncludedInParameterEstimation), New XAttribute("rules", point.IncludedInRuleEvaluation), New XAttribute("scope", point.ExclusionScope), New XAttribute("reason", point.ExclusionReason), New XAttribute("rule_numbers", String.Join(",", point.SignalRuleNumbers)), New XElement("source_rows", String.Join(",", point.SourceRowIndices))))
            Next
            For Each signal In panel.Signals
                p.Add(New XElement("signal", New XAttribute("rule", signal.RuleNumber), New XAttribute("stage", signal.StageId), New XAttribute("terminal", signal.TerminalPointIndex), New XAttribute("first", signal.WindowStartPointIndex), New XAttribute("last", signal.WindowEndPointIndex), New XAttribute("contributors", String.Join(",", signal.ContributingPointIndices)), New XAttribute("marked", String.Join(",", signal.MarkedPointIndices)), signal.Message))
            Next
            node.Add(p)
        Next
        For Each diagnostic In context.Diagnostics
            node.Add(New XElement("diagnostic", New XAttribute("point", diagnostic.PointIndex), New XAttribute("sources", String.Join(",", diagnostic.SourceRowIndices)), New XAttribute("label", diagnostic.Label), New XAttribute("stage", diagnostic.StageId), New XAttribute("phase", diagnostic.Phase), New XAttribute("state_length", If(diagnostic.StateVector Is Nothing, -1, diagnostic.StateVector.Length)), New XAttribute("scores_length", If(diagnostic.ComponentScores Is Nothing, -1, diagnostic.ComponentScores.Length)), New XAttribute("residual_length", If(diagnostic.ResidualVector Is Nothing, -1, diagnostic.ResidualVector.Length)), New XAttribute("contribution_length", If(diagnostic.Contributions Is Nothing, -1, diagnostic.Contributions.Length)), New XElement("optional_arrays_sha256", Spc23Fixture.Hash(New Object() {diagnostic.ObservationVector, diagnostic.StateVector, diagnostic.ComponentScores, diagnostic.ResidualVector, diagnostic.Contributions, diagnostic.SubgroupCovariance, diagnostic.ContributionTotal, diagnostic.ContributionBasis}))))
        Next
        Dim tables As New Spc23LegacyPreparation()
        Dim summaries = tables.BuildSummaryTables(result) : summaries.AddRange(tables.BuildModelTables(result)) : summaries.Add(tables.BuildChartDataTable(result)) : summaries.Add(tables.BuildSignalsTable(result)) : summaries.AddRange(tables.BuildAuditTables(result))
        For Each onlySignal In {False, True}
            tables.SignalOnly = onlySignal
            node.Add(New XElement("diagnostic_tables", New XAttribute("signal_only", onlySignal), New XAttribute("selected", context.SelectDiagnostics(onlySignal).Count), Spc21Fixture.TableHash(tables.BuildDiagnosticTables(result))))
        Next
        node.Add(New XElement("tables_sha256", Spc21Fixture.TableHash(summaries)))
        Return node
    End Function
    Public Shared Function Bits(value As Double) As String
        Return BitConverter.DoubleToInt64Bits(value).ToString("X16", CultureInfo.InvariantCulture)
    End Function
    Public Shared Function Hash(value As Object) As String
        Dim builder As New StringBuilder()
        Encode(builder, value)
        Using digest = Security.Cryptography.SHA256.Create()
            Return BitConverter.ToString(digest.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()))).Replace("-", "").ToLowerInvariant()
        End Using
    End Function
    Private Shared Sub Encode(builder As StringBuilder, value As Object)
        If value Is Nothing Then
            builder.Append("null;") : Return
        End If
        Dim t = value.GetType() : builder.Append(t.FullName).Append(":"c)
        If TypeOf value Is Double Then
            builder.Append(Bits(CDbl(value))).Append(";"c) : Return
        End If
        If TypeOf value Is String OrElse t.IsPrimitive OrElse t.IsEnum OrElse TypeOf value Is Decimal Then
            Dim text = Spc21Fixture.Text(value) : builder.Append(text.Length).Append(":"c).Append(text).Append(";"c) : Return
        End If
        If TypeOf value Is Array Then
            Dim a = DirectCast(value, Array)
            For i = 0 To a.Rank - 1 : builder.Append(a.GetLength(i)).Append(","c) : Next
        End If
        If TypeOf value Is Collections.IEnumerable Then
            For Each item As Object In DirectCast(value, Collections.IEnumerable) : Encode(builder, item) : Next
        Else
            For Each p In t.GetProperties().Where(Function(v) v.CanRead AndAlso v.GetIndexParameters().Length = 0).OrderBy(Function(v) v.Name, StringComparer.Ordinal)
                builder.Append(p.Name) : Encode(builder, p.GetValue(value, Nothing))
            Next
            For Each f In t.GetFields().OrderBy(Function(v) v.Name, StringComparer.Ordinal)
                builder.Append(f.Name) : Encode(builder, f.GetValue(value))
            Next
        End If
        builder.Append(";"c)
    End Sub
    Public Shared Sub Emit(node As XElement)
        Console.WriteLine("04A23_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(node.ToString(SaveOptions.DisableFormatting))))
    End Sub
End Class
