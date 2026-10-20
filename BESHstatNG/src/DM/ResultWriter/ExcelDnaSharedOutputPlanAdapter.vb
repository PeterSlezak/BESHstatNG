Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure

' The form's complete shared-output orchestration, with only native host operations
' injected. Tests use real analysis results and the real projection/intent/actions gate.
Friend NotInheritable Class ExcelDnaSharedOutputHost
    Friend Property Acquire As Func(Of ExcelDnaSharedOutputRun, ExcelDnaResultWriter)
    Friend Property Target As Func(Of ExcelDnaResultWriter, SpreadsheetOutputTarget)
    Friend Property Identities As Func(Of ExcelDnaResultWriter, Tuple(Of String, String))
    Friend Property InspectCharts As Func(Of ExcelDnaResultWriter, WriteIntent, ExcelDnaChartSafetySnapshot)
    Friend Property ConfirmCharts As Func(Of ExcelDnaChartSafetySnapshot, Boolean)
    Friend Property Charts As Func(Of ExcelDnaResultWriter, Integer, IReadOnlyList(Of Action))
    Friend Property Inspect As Func(Of ExcelDnaResultWriter, WriteIntent, WriteInspection)
    Friend Property Confirm As Func(Of WriteInspection, Boolean)
End Class

Friend NotInheritable Class ExcelDnaSharedOutputRun
    Friend Property Stage As String = "precreation-plan"
    Friend Property CreationAttempted As Boolean
    Friend Property DestinationCreated As Boolean
    Friend Property RequestedTarget As SpreadsheetOutputTarget
    Friend Property Intent As WriteIntent
    Friend Property Writer As ExcelDnaResultWriter
    Friend Property Failure As Exception

    Friend Sub LogFailure(failure As Exception)
        Me.Failure = failure
        CoreServices.Logger.Error(failure, "Shared Windows output stage=" & Stage &
            "; creationAttempted=" & CreationAttempted.ToString() & "; destinationCreated=" & DestinationCreated.ToString())
    End Sub

    Friend Function Execute(result As SpreadsheetAnalysisResult, supplementary As List(Of ResultTable),
                            settings As PValuePresentationSettings, alpha As Double, host As ExcelDnaSharedOutputHost) As WriteResult
        Try
            If result Is Nothing OrElse supplementary Is Nothing OrElse host Is Nothing Then Throw New ArgumentNullException("shared output")
            Stage = "target-resolution"
            Writer = host.Acquire(Me)
            Stage = "shared-request-projection"
            Dim target = host.Target(Writer)
            RequestedTarget = target
            Dim document = OutputDocument.FromResult(result)
            Dim requests = ResultTableWriteRequestFactory.CreateMany(result.Tables, settings, alpha, target)
            For Each request In requests
                request.AutoFit = False
            Next
            Dim layout = OutputLayoutPlanner.Create(document, target, New OutputLayoutOptions(blankRowsBetweenTables:=1))
            Dim prepared = ExcelDnaSharedOutputPlanAdapter.Prepare(layout, requests, target, target.WorksheetName)
            Dim extras = supplementary.Select(Function(t) t.ToOutputModel()).ToList()
            Dim bounds = ExcelDnaSharedOutputPlanAdapter.PreflightBounds(prepared, extras)
            Stage = "chart-intents"
            Dim charts = host.Charts(Writer, bounds.EndColumn - bounds.StartColumn + 1)
            Stage = "intent-construction"
            Dim identities = host.Identities(Writer)
            Intent = ExcelDnaSharedOutputPlanAdapter.CreateWriteIntent(prepared, extras,
                charts.Select(Function(c, i) "native-chart-" & i.ToString(CultureInfo.InvariantCulture)), Nothing, identities.Item1, identities.Item2)
            Stage = "action-validation"
            Dim writes = ExcelDnaSharedOutputPlanAdapter.CreateWriteActions(prepared, Writer, supplementary, charts)
            ' SafeWrite's public preconditions precede its Try. Keep that contract intact;
            ' validate and call inside THIS host Try so failures are logged and classified.
            If writes.Count <> Intent.Regions.Count OrElse writes.Any(Function(a) a Is Nothing) Then Throw New ArgumentException("Shared write action/region mismatch.")
            Dim actions As New List(Of Action)()
            For Each write As Action In writes
                Dim captured = write
                actions.Add(Sub()
                                Stage = "execution"
                                Try
                                    captured()
                                Catch ex As Exception
                                    LogFailure(ex)
                                    Throw
                                End Try
                            End Sub)
            Next
            Dim inspections As Integer = 0
            Dim chartObservation As ExcelDnaChartSafetySnapshot = Nothing
            Dim inspect As Func(Of WriteInspection) = Function()
                                                       inspections += 1
                                                       Stage = If(inspections = 1, "occupancy-inspection", "occupancy-revalidation")
                                                       Try
                                                           Dim observed = host.Inspect(Writer, Intent)
                                                           If chartObservation IsNot Nothing Then
                                                               Stage = "chart-revalidation"
                                                               Dim currentCharts = host.InspectCharts(Writer, Intent)
                                                               If currentCharts Is Nothing Then Throw New InvalidOperationException("Missing shape inspection.")
                                                               LogCharts(currentCharts, charts.Count)
                                                               If currentCharts.ObservationIdentity <> chartObservation.ObservationIdentity OrElse
                                                                   currentCharts.ExistingShapeCount <> chartObservation.ExistingShapeCount Then
                                                                   Return New WriteInspection(Intent, OutputOccupancy.Rejected, "chart-changed", Nothing,
                                                                       explanation:="Existing chart objects changed. Inspect and confirm again.")
                                                               End If
                                                           End If
                                                           Return observed
                                                       Catch ex As Exception
                                                           LogFailure(ex)
                                                           Throw
                                                       End Try
                                                   End Function
            ' Read every planned cell before either kind of prompt. Cache only this first
            ' observation; SafeWrite always obtains a fresh inspection before any action.
            Dim firstObservation = inspect()
            If charts.Count > 0 AndAlso firstObservation IsNot Nothing AndAlso
                (firstObservation.State = OutputOccupancy.Empty OrElse firstObservation.State = OutputOccupancy.Occupied) Then
                Stage = "chart-inspection"
                chartObservation = host.InspectCharts(Writer, Intent)
                If chartObservation Is Nothing Then Throw New InvalidOperationException("Missing shape inspection.")
                LogCharts(chartObservation, charts.Count)
                If chartObservation.ExistingShapeCount > 0 Then
                    Stage = "chart-consent"
                    If Not host.ConfirmCharts(chartObservation) Then
                        Dim cancelled As New WriteResult(If(CreationAttempted, OutputWriteState.PartiallyWritten, OutputWriteState.CancelledByUser),
                            "output.chart-cancelled", If(CreationAttempted, "Chart addition cancelled; a created destination may remain. No rollback was attempted.",
                            "Chart addition cancelled. No output writes performed."), Intent, CreationAttempted,
                            confirmationRequired:=True, overwriteApproved:=False)
                        Writer.LastSharedWriteResult = cancelled
                        Return cancelled
                    End If
                End If
            End If
            Dim useCached As Boolean = True
            Dim outcome = SafeWrite.Execute(Intent, Function()
                                                       If useCached Then
                                                           useCached = False
                                                           Return firstObservation
                                                       End If
                                                       Return inspect()
                                                   End Function,
                Function(observed)
                    Stage = "consent"
                    Try
                        Return host.Confirm(observed)
                    Catch ex As Exception
                        LogFailure(ex)
                        Throw
                    End Try
                End Function, actions, destinationCreated:=CreationAttempted)
            If outcome.State = OutputWriteState.Completed AndAlso chartObservation IsNot Nothing AndAlso chartObservation.ExistingShapeCount > 0 Then
                outcome = New WriteResult(outcome.State, "output.completed.chart-overlap-approved",
                    "Output completed. Possible chart overlap with existing shapes was approved; exact placement was not predicted.",
                    Intent, outcome.MutationMayHaveOccurred, outcome.ConfirmedWritten.Select(Function(r) r.ArtifactId),
                    outcome.ConfirmationRequired, outcome.OverwriteApproved)
            End If
            Writer.LastSharedWriteResult = outcome
            Return outcome
        Catch ex As Exception
            LogFailure(ex)
            ' Minimal unresolved failure scope: no synthetic full intent before the
            ' established target preflight, and no fabricated physical sheet identity.
            Dim target = If(RequestedTarget, New SpreadsheetOutputTarget())
            Dim fallback As New SpreadsheetOutputTarget With {.Destination = target.Destination,
                .WorksheetName = If(String.IsNullOrWhiteSpace(target.WorksheetName), "Unresolved destination", target.WorksheetName),
                .StartRow = Math.Max(1, Math.Min(1048576, target.StartRow)), .StartColumn = Math.Max(1, Math.Min(16384, target.StartColumn))}
            Dim scope = If(Intent, New WriteIntent("unresolved-plan", "unresolved-target", fallback, New WriteRegion() {}))
            Dim rejected = TryCast(ex, HostCapabilityException)
            Dim outcome As New WriteResult(If(CreationAttempted, OutputWriteState.PartiallyWritten,
                If(rejected IsNot Nothing, OutputWriteState.RejectedBeforeWrite, OutputWriteState.FailedBeforeWrite)),
                If(CreationAttempted, WriteCodes.MutationUncertain, If(rejected IsNot Nothing, rejected.Code, WriteCodes.Failed)),
                If(CreationAttempted, If(DestinationCreated, "Output preparation failed; a created worksheet or workbook may remain. No rollback was attempted.",
                    "Destination creation was attempted and may have changed the workbook. No rollback was attempted."),
                    If(rejected IsNot Nothing, rejected.Message, "Output preparation failed before any write was started.")), scope, CreationAttempted)
            If Writer IsNot Nothing Then Writer.LastSharedWriteResult = outcome
            Return outcome
        End Try
    End Function

    Private Sub LogCharts(snapshot As ExcelDnaChartSafetySnapshot, plannedCount As Integer)
        CoreServices.Logger.Debug("Shared Windows output stage=" & Stage & "; role=append-only-shape; plannedCharts=" &
            plannedCount.ToString(CultureInfo.InvariantCulture) & "; existingShapes=" & snapshot.ExistingShapeCount.ToString(CultureInfo.InvariantCulture) &
            "; placement=host-dependent; exactCollisionFootprint=unverified")
    End Sub
End Class

' Bounded Windows bridge: no COM, live occupancy, or workbook-creation policy here.
Friend NotInheritable Class ExcelDnaSharedWriteCommand
    Private ReadOnly snapshot As OutputTableArtifact

    Friend Sub New(placement As OutputBlockPlacement, ordinal As Integer, request As ResultTableWriteRequest)
        ArtifactId = placement.ArtifactId
        SourceOrdinal = ordinal
        DocumentIndex = placement.Index
        Bounds = placement.Bounds
        Dim values(placement.RowCount - 1, placement.ColumnCount - 1) As Object
        For row As Integer = 0 To placement.RowCount - 1
            For column As Integer = 0 To placement.ColumnCount - 1
                values(row, column) = request.Values(row)(column)
            Next
        Next
        snapshot = New OutputTableArtifact(ArtifactId, New ResultTableOutputModel(values,
            request.HeaderTopRows, request.HeaderLeftColumns, request.FooterRows, request.PvalueColumns,
            request.TitleRows, request.IsResultTable,
            request.PvalueCells.Select(Function(p) New ResultTableCellAddress(p.BodyRow, p.BodyColumn))))
        PvalueNumberFormat = request.PvalueNumberFormat
        PvalueHighlightAlpha = request.PvalueHighlightAlpha
        AutoFit = request.AutoFit
    End Sub

    Public ReadOnly Property ArtifactId As String
    Public ReadOnly Property SourceOrdinal As Integer
    Public ReadOnly Property DocumentIndex As Integer
    Public ReadOnly Property Bounds As OutputRectangle
    Public ReadOnly Property PvalueNumberFormat As String
    Public ReadOnly Property PvalueHighlightAlpha As Double
    Public ReadOnly Property AutoFit As Boolean
    Public ReadOnly Property Model As ResultTableOutputModel
        Get
            Return snapshot.Model
        End Get
    End Property
End Class

Friend NotInheritable Class ExcelDnaPreparedSharedOutput
    Friend Sub New(layout As OutputLayoutPlan, commands As List(Of ExcelDnaSharedWriteCommand))
        Me.Layout = layout
        Me.Commands = New List(Of ExcelDnaSharedWriteCommand)(commands).AsReadOnly()
    End Sub

    Public ReadOnly Property Layout As OutputLayoutPlan
    Public ReadOnly Property Commands As IReadOnlyList(Of ExcelDnaSharedWriteCommand)

    ' The cursor marks the attempted block if execution fails. Prior successes stay written;
    ' even the failing block may have written values before formatting threw. No rollback.
    Public Sub Execute(target As SpreadsheetOutputTarget, worksheetName As String,
                       write As Action(Of ExcelDnaSharedWriteCommand), setRow As Action(Of Integer))
        ExcelDnaSharedOutputPlanAdapter.ValidateTarget(Layout, target, worksheetName)
        If write Is Nothing Then Throw New ArgumentNullException(NameOf(write))
        If setRow Is Nothing Then Throw New ArgumentNullException(NameOf(setRow))
        For Each command As ExcelDnaSharedWriteCommand In Commands
            setRow(command.Bounds.StartRow)
            write(command)
            setRow(command.Bounds.EndRow + 1)
        Next
    End Sub
End Class

Friend NotInheritable Class ExcelDnaSharedOutputPlanAdapter
    Friend Const MaxRows As Integer = 1048576
    Friend Const MaxColumns As Integer = 16384

    Private Sub New()
    End Sub

    ' Requests have no IDs in the legacy protocol. Production binds source ordinal N to
    ' table-N; custom documents must supply the exact ordered IDs explicitly. Compare the
    ' complete normalized payload/metadata as well, so stale/reordered requests fail closed.
    Public Shared Function Prepare(layout As OutputLayoutPlan, requests As IReadOnlyList(Of ResultTableWriteRequest),
                                   target As SpreadsheetOutputTarget, worksheetName As String,
                                   Optional tableIds As IReadOnlyList(Of String) = Nothing) As ExcelDnaPreparedSharedOutput
        ValidateTarget(layout, target, worksheetName)
        If requests Is Nothing Then Throw New ArgumentNullException(NameOf(requests))
        Dim tables = layout.Placements.Where(Function(p) p.Kind = OutputArtifactKind.Table).ToList()
        If tables.Count <> requests.Count OrElse (tableIds IsNot Nothing AndAlso tableIds.Count <> tables.Count) Then
            Throw New ArgumentException("Shared table/request identity count mismatch.")
        End If
        Dim commands As New List(Of ExcelDnaSharedWriteCommand)()
        Dim ids As New HashSet(Of String)(StringComparer.Ordinal)
        Dim ordinal As Integer = 0
        For index As Integer = 0 To layout.Placements.Count - 1
            Dim placement = layout.Placements(index)
            If placement.Index <> index OrElse Not ids.Add(placement.ArtifactId) Then Throw New ArgumentException("Invalid layout identity/order.")
            Select Case placement.Kind
                Case OutputArtifactKind.Diagnostic, OutputArtifactKind.Plot
                    If placement.IsPlaced Then Throw New ArgumentException("Nonspatial artifact has a cell placement.")
                    Continue For
                Case OutputArtifactKind.Table
                Case Else
                    Throw New ArgumentException("Unsupported output artifact.")
            End Select
            Dim id As String = If(tableIds Is Nothing, "table-" & ordinal.ToString(CultureInfo.InvariantCulture), tableIds(ordinal))
            If Not String.Equals(id, placement.ArtifactId, StringComparison.Ordinal) Then Throw New ArgumentException("Shared table ID/order mismatch.")
            Dim request = requests(ordinal)
            If request Is Nothing Then Throw New ArgumentException("Null shared request.")
            ValidateTarget(layout, request.OutputTarget, worksheetName)
            Dim table = DirectCast(placement.Artifact, OutputTableArtifact)
            Dim expected = ResultTableWriteRequestFactory.Create(table.Model, Nothing, 0.05, target)
            ValidatePayload(expected, request)
            If placement.RowCount > 0 AndAlso placement.ColumnCount > 0 Then
                Dim bounds = placement.Bounds
                If bounds Is Nothing OrElse bounds.StartColumn <> layout.StartColumn OrElse
                   CLng(bounds.EndRow) - bounds.StartRow + 1 <> placement.RowCount OrElse
                   CLng(bounds.EndColumn) - bounds.StartColumn + 1 <> placement.ColumnCount OrElse
                   bounds.EndRow >= MaxRows OrElse bounds.EndColumn > MaxColumns Then
                    ' Deliberately refuse a final-row write before any writes: the legacy cursor
                    ' must remain a valid worksheet row for subsequent tables/chart anchors.
                    Throw New ArgumentException("Shared bounds/shape invalid or no next worksheet row is available.")
                End If
                commands.Add(New ExcelDnaSharedWriteCommand(placement, ordinal, request))
            ElseIf placement.IsPlaced Then
                Throw New ArgumentException("Zero-area table has a cell placement.")
            End If
            ordinal += 1
        Next
        Return New ExcelDnaPreparedSharedOutput(layout, commands)
    End Function

    Friend Shared Sub ValidateTarget(layout As OutputLayoutPlan, target As SpreadsheetOutputTarget, worksheetName As String)
        If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If String.IsNullOrWhiteSpace(worksheetName) OrElse String.IsNullOrWhiteSpace(target.WorksheetName) OrElse
           Not String.Equals(worksheetName, layout.WorksheetName, StringComparison.Ordinal) OrElse
           Not String.Equals(target.WorksheetName, layout.WorksheetName, StringComparison.Ordinal) OrElse
           target.Destination <> layout.Destination OrElse target.StartRow <> layout.StartRow OrElse
           target.StartColumn <> layout.StartColumn OrElse target.StartRow < 1 OrElse target.StartRow > MaxRows OrElse
           target.StartColumn < 1 OrElse target.StartColumn > MaxColumns OrElse
           layout.Options.MaxRows > MaxRows OrElse layout.Options.MaxColumns > MaxColumns OrElse
           layout.Options.BlankRowsBetweenTables <> 1 Then
            Throw New ArgumentException("Shared output target/start/worksheet changed or is outside the Windows grid policy.")
        End If
    End Sub

    Private Shared Sub ValidatePayload(expected As ResultTableWriteRequest, actual As ResultTableWriteRequest)
        If actual.Values Is Nothing OrElse actual.Values.Count <> expected.Values.Count OrElse
           actual.HeaderTopRows <> expected.HeaderTopRows OrElse actual.HeaderLeftColumns <> expected.HeaderLeftColumns OrElse
           actual.TitleRows <> expected.TitleRows OrElse actual.FooterRows <> expected.FooterRows OrElse
           actual.IsResultTable <> expected.IsResultTable OrElse actual.PvalueColumns Is Nothing OrElse actual.PvalueCells Is Nothing OrElse
           Not actual.PvalueColumns.SequenceEqual(expected.PvalueColumns) OrElse actual.PvalueCells.Count <> expected.PvalueCells.Count Then
            Throw New ArgumentException("Shared request shape/metadata mismatch.")
        End If
        For row As Integer = 0 To expected.Values.Count - 1
            If actual.Values(row) Is Nothing OrElse actual.Values(row).Count <> expected.Values(row).Count Then Throw New ArgumentException("Shared request is not rectangular or has a different shape.")
            For column As Integer = 0 To expected.Values(row).Count - 1
                Dim left = expected.Values(row)(column)
                Dim right = actual.Values(row)(column)
                If Not Object.Equals(left, right) OrElse (left IsNot Nothing AndAlso right IsNot Nothing AndAlso left.GetType() <> right.GetType()) Then
                    Throw New ArgumentException("Shared request value/type/order mismatch.")
                End If
            Next
        Next
        For index As Integer = 0 To expected.PvalueCells.Count - 1
            Dim cell = actual.PvalueCells(index)
            If cell Is Nothing OrElse cell.BodyRow <> expected.PvalueCells(index).BodyRow OrElse cell.BodyColumn <> expected.PvalueCells(index).BodyColumn Then
                Throw New ArgumentException("Shared request body p-value annotation mismatch.")
            End If
        Next
    End Sub

    ' Conservative legacy AreaCheck envelope includes shared gaps and all supplementary
    ' matrices/gaps. Only the shared primary geometry comes from the canonical planner.
    Public Shared Function PreflightBounds(prepared As ExcelDnaPreparedSharedOutput,
                                           supplementary As IEnumerable(Of ResultTableOutputModel)) As OutputRectangle
        If prepared Is Nothing Then Throw New ArgumentNullException(NameOf(prepared))
        Return PreflightBounds(prepared.Layout, supplementary)
    End Function

    Friend Shared Function PreflightBounds(layout As OutputLayoutPlan,
                                           supplementary As IEnumerable(Of ResultTableOutputModel)) As OutputRectangle
        If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
        Dim used = layout.UsedBounds
        If used Is Nothing Then Throw New ArgumentException("This UI workflow requires occupied shared tables.")
        Dim rowLimit As Integer = Math.Min(MaxRows, layout.Options.MaxRows)
        Dim columnLimit As Integer = Math.Min(MaxColumns, layout.Options.MaxColumns)
        If used.EndRow >= rowLimit Then Throw New ArgumentException("Output leaves no next Windows cursor row.")
        Dim endRow As Long = used.EndRow
        Dim endColumn As Long = used.EndColumn
        If supplementary IsNot Nothing Then
            For Each model In supplementary
                If model Is Nothing OrElse model.RowCount < 1 OrElse model.ColumnCount < 1 Then Throw New ArgumentException("Invalid supplementary table.")
                endRow += 1L + model.RowCount
                endColumn = Math.Max(endColumn, CLng(used.StartColumn) + model.ColumnCount - 1L)
                If endRow >= rowLimit OrElse endColumn > columnLimit Then Throw New ArgumentException("Combined output exceeds the Windows grid or leaves no next cursor row.")
            Next
        End If
        Return New OutputRectangle(used.StartRow, used.StartColumn, CInt(endRow), CInt(endColumn))
    End Function

    Public Shared Function ConfirmOverwrite(occupied As Boolean, confirm As Func(Of Boolean)) As Boolean
        If confirm Is Nothing Then Throw New ArgumentNullException(NameOf(confirm))
        Return Not occupied OrElse confirm()
    End Function

    ' Exact supplementary cursor policy mirrors ProcessListofResultTables(..., True).
    ' Chart additions are ordered non-cell mutations; their conservative inspection guards
    ' are supplied separately by the Windows host, never substituted for table bounds.
    Friend Shared Function CreateWriteIntent(prepared As ExcelDnaPreparedSharedOutput,
                                             supplementary As IEnumerable(Of ResultTableOutputModel),
                                             chartIds As IEnumerable(Of String), guards As IEnumerable(Of OutputRectangle),
                                             workbookIdentity As String, worksheetIdentity As String) As WriteIntent
        If prepared Is Nothing Then Throw New ArgumentNullException(NameOf(prepared))
        Return CreateWriteIntent(prepared.Layout, supplementary, chartIds, guards, workbookIdentity, worksheetIdentity)
    End Function

    Friend Shared Function CreateWriteIntent(layout As OutputLayoutPlan,
                                             supplementary As IEnumerable(Of ResultTableOutputModel),
                                             chartIds As IEnumerable(Of String), guards As IEnumerable(Of OutputRectangle),
                                             workbookIdentity As String, worksheetIdentity As String) As WriteIntent
        If layout Is Nothing OrElse layout.UsedBounds Is Nothing Then Throw New ArgumentException("Shared output requires spatial tables.")
        Dim regions = layout.Placements.Where(Function(p) p.IsPlaced).Select(Function(p) New WriteRegion(p.ArtifactId, p.Bounds)).ToList()
        Dim lastRow As Integer = layout.UsedBounds.EndRow
        If supplementary IsNot Nothing Then
            Dim index As Integer = 0
            For Each model In supplementary
                If model Is Nothing OrElse model.RowCount < 1 OrElse model.ColumnCount < 1 Then Throw New ArgumentException("Invalid supplementary table.")
                Dim start As Long = CLng(lastRow) + 2
                Dim finish As Long = start + model.RowCount - 1L
                Dim right As Long = CLng(layout.StartColumn) + model.ColumnCount - 1L
                If finish >= MaxRows OrElse right > MaxColumns Then Throw New ArgumentException("Supplementary output exceeds grid.")
                regions.Add(New WriteRegion("supplementary-" & index.ToString(CultureInfo.InvariantCulture),
                    New OutputRectangle(CInt(start), layout.StartColumn, CInt(finish), CInt(right))))
                lastRow = CInt(finish) : index += 1
            Next
        End If
        If chartIds IsNot Nothing Then
            For Each id In chartIds
                regions.Add(New WriteRegion(id, Nothing, nonCellMutation:=True))
            Next
        End If
        Dim target As New SpreadsheetOutputTarget With {.Destination = layout.Destination,
            .WorksheetName = layout.WorksheetName, .StartRow = layout.StartRow, .StartColumn = layout.StartColumn}
        Return New WriteIntent(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"), target,
            regions, layout.Reservations, guards, workbookIdentity, worksheetIdentity)
    End Function

    Friend Shared Function CreateWriteActions(prepared As ExcelDnaPreparedSharedOutput, writer As ExcelDnaResultWriter,
                                              supplementary As IEnumerable(Of ResultTable), charts As IEnumerable(Of Action)) As IReadOnlyList(Of Action)
        If prepared Is Nothing OrElse writer Is Nothing OrElse supplementary Is Nothing OrElse charts Is Nothing Then Throw New ArgumentNullException("write sink")
        Dim actions As New List(Of Action)()
        For Each command As ExcelDnaSharedWriteCommand In prepared.Commands
            Dim captured = command
            actions.Add(Sub() writer.WriteSharedCommand(captured))
        Next
        For Each table In supplementary
            If table Is Nothing Then Throw New ArgumentException("Null supplementary table.")
            Dim captured = table
            actions.Add(Sub()
                            writer.shiftRowPointer()
                            writer.write(captured)
                        End Sub)
        Next
        For Each chart In charts
            If chart Is Nothing Then Throw New ArgumentException("Null chart sink.")
            actions.Add(chart)
        Next
        Return actions.AsReadOnly()
    End Function
End Class
