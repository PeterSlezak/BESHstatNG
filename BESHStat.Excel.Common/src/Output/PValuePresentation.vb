Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.AppInfrastructure

Namespace ExcelCommon
    ''' <summary>Immutable snapshot of the existing Core display policy, not a second algorithm.</summary>
    Public NotInheritable Class OutputPValuePolicy
        Public Sub New(settings As PValuePresentationSettings)
            Dim safe As PValuePresentationSettings = CopySettings(settings)
            DecimalPlaces = safe.DecimalPlaces
            SmallValueDisplay = safe.SmallValueDisplay
            UseUpperBound = safe.UseUpperBound
            LowerThreshold = Global.BESHStatNG.PValuePresentation.LowerThreshold(safe)
            UpperThreshold = Global.BESHStatNG.PValuePresentation.UpperThreshold(safe)
        End Sub
        Public ReadOnly Property DecimalPlaces As Integer
        Public ReadOnly Property SmallValueDisplay As PValueSmallValueDisplay
        Public ReadOnly Property UseUpperBound As Boolean
        Public ReadOnly Property LowerThreshold As Double
        Public ReadOnly Property UpperThreshold As Double

        Friend Shared Function CopySettings(settings As PValuePresentationSettings) As PValuePresentationSettings
            Dim safe As New PValuePresentationSettings()
            If settings IsNot Nothing Then
                safe.DecimalPlaces = settings.DecimalPlaces
                safe.SmallValueDisplay = settings.SmallValueDisplay
                safe.UseUpperBound = settings.UseUpperBound
            End If
            safe.EnsureDefaults()
            Return safe
        End Function

        Public Function Preview(value As Double, culture As CultureInfo) As String
            If culture Is Nothing Then Throw New ArgumentNullException(NameOf(culture))
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value < 0 OrElse value > 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(value), "A preview requires a probability.")
            End If
            Return Global.BESHStatNG.PValuePresentation.FormatForDisplay(value,
                New PValuePresentationSettings With {.DecimalPlaces = DecimalPlaces,
                    .SmallValueDisplay = SmallValueDisplay, .UseUpperBound = UseUpperBound}, culture)
        End Function
    End Class

    Public NotInheritable Class OutputSemanticPlanner
        Private Sub New()
        End Sub

        Public Shared Function Create(table As OutputTableArtifact, settings As PValuePresentationSettings,
                                      alpha As Double, Optional previewCultureName As String = Nothing) As OutputSemanticPlan
            If table Is Nothing Then Throw New ArgumentNullException(NameOf(table))
            Return Create(New OutputDocument(New OutputArtifact() {table}), settings, alpha, previewCultureName)
        End Function

        ''' <summary>
        ''' Null culture name disables previews; empty means invariant. A named standard culture
        ''' fully identifies the preview choice in the plan; no machine-default provider is used.
        ''' Invalid alpha uses the legacy factory's 0.05 fallback. Invalid annotations are
        ''' skipped/diagnosed, valid duplicates and column/cell overlap collapse to one target.
        ''' </summary>
        Public Shared Function Create(document As OutputDocument, settings As PValuePresentationSettings,
                                      alpha As Double, Optional previewCultureName As String = Nothing) As OutputSemanticPlan
            If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
            Dim culture As CultureInfo = Nothing
            If previewCultureName IsNot Nothing Then culture = CultureInfo.GetCultureInfo(previewCultureName)
            Dim diagnostics As New List(Of OutputPlanDiagnostic)()
            Dim policy As New OutputPValuePolicy(settings)
            If settings IsNot Nothing AndAlso (settings.DecimalPlaces <> policy.DecimalPlaces OrElse settings.SmallValueDisplay <> policy.SmallValueDisplay) Then
                diagnostics.Add(New OutputPlanDiagnostic(String.Empty, "pvalue.settings.defaulted", Nothing))
            End If
            If Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) OrElse alpha <= 0 OrElse alpha >= 1 Then
                alpha = 0.05
                diagnostics.Add(New OutputPlanDiagnostic(String.Empty, "pvalue.alpha.defaulted", Nothing))
            End If
            Dim artifacts As New List(Of OutputArtifactStylePlan)()
            For Each artifact As OutputArtifact In document.Artifacts
                If TypeOf artifact Is OutputTableArtifact Then
                    artifacts.Add(PlanTable(DirectCast(artifact, OutputTableArtifact), policy, alpha, culture, diagnostics))
                Else
                    Dim role As OutputStyleRole? = Nothing
                    If TypeOf artifact Is OutputDiagnosticArtifact Then
                        role = If(DirectCast(artifact, OutputDiagnosticArtifact).Severity = OutputDiagnosticSeverity.Warning,
                                  OutputStyleRole.DiagnosticWarning, OutputStyleRole.DiagnosticFault)
                    End If
                    artifacts.Add(New OutputArtifactStylePlan(artifact, 0, 0, New OutputCellStyle() {}, role))
                End If
            Next
            Return New OutputSemanticPlan(document, policy, alpha, If(culture Is Nothing, Nothing, culture.Name), artifacts, diagnostics)
        End Function

        Private Shared Function PlanTable(artifact As OutputTableArtifact, policy As OutputPValuePolicy, alpha As Double,
                                          culture As CultureInfo, diagnostics As List(Of OutputPlanDiagnostic)) As OutputArtifactStylePlan
            Dim model As Global.BESHStatNG.ResultTableOutputModel = artifact.Model
            Dim firstBodyRow As Integer = model.TitleRows + model.HeaderTopRows
            Dim bodyRows As Integer = model.RowCount - firstBodyRow - model.FooterRows
            Dim bodyColumns As Integer = model.ColumnCount - model.HeaderLeftColumns
            Dim selected As New HashSet(Of Long)()
            For index As Integer = 0 To model.PvalueColumns.Count - 1
                Dim column As Integer = model.PvalueColumns(index)
                If column < 1 OrElse column > bodyColumns OrElse bodyRows = 0 Then
                    diagnostics.Add(New OutputPlanDiagnostic(artifact.Id, "pvalue.column.invalid", index))
                    Continue For
                End If
                For row As Integer = 0 To bodyRows - 1
                    selected.Add(CellKey(firstBodyRow + row, model.HeaderLeftColumns + column - 1, model.ColumnCount))
                Next
            Next
            For index As Integer = 0 To model.PvalueCells.Count - 1
                Dim address As Global.BESHStatNG.ResultTableCellAddress = model.PvalueCells(index)
                If address.BodyRow < 1 OrElse address.BodyRow > bodyRows OrElse address.BodyColumn < 1 OrElse address.BodyColumn > bodyColumns Then
                    diagnostics.Add(New OutputPlanDiagnostic(artifact.Id, "pvalue.cell.invalid", index))
                    Continue For
                End If
                selected.Add(CellKey(firstBodyRow + address.BodyRow - 1, model.HeaderLeftColumns + address.BodyColumn - 1, model.ColumnCount))
            Next
            Dim canonical As New OutputCellTable(artifact)
            Dim cells As New List(Of OutputCellStyle)()
            Dim general As New OutputNumberFormatIntent(OutputNumberFormatKind.General)
            Dim pvalueFormat As New OutputNumberFormatIntent(OutputNumberFormatKind.PValue, policy.DecimalPlaces)
            For row As Integer = 0 To model.RowCount - 1
                For column As Integer = 0 To model.ColumnCount - 1
                    Dim role As OutputStyleRole
                    If row < model.TitleRows Then
                        role = OutputStyleRole.Title
                    ElseIf row < firstBodyRow Then
                        role = OutputStyleRole.TopColumnHeader
                    ElseIf row >= model.RowCount - model.FooterRows Then
                        role = OutputStyleRole.Footer
                    ElseIf column < model.HeaderLeftColumns Then
                        role = OutputStyleRole.LeftRowHeader
                    Else
                        role = OutputStyleRole.Body
                    End If
                    Dim cell As OutputCell = canonical.CellAt(row, column)
                    Dim isPValue As Boolean = selected.Contains(CellKey(row, column, model.ColumnCount))
                    Dim value As Double
                    Dim valid As Boolean = isPValue AndAlso cell.TryGetProbability(value)
                    Dim significant As Boolean = isPValue AndAlso cell.IsSignificantAt(alpha)
                    Dim preview As String = Nothing
                    If valid AndAlso culture IsNot Nothing Then preview = policy.Preview(value, culture)
                    cells.Add(New OutputCellStyle(artifact.Id, row, column, cell, role,
                        If(isPValue, pvalueFormat, general), isPValue, significant, preview))
                Next
            Next
            Return New OutputArtifactStylePlan(artifact, model.RowCount, model.ColumnCount, cells, Nothing)
        End Function

        Private Shared Function CellKey(row As Integer, column As Integer, columnCount As Integer) As Long
            Return CLng(row) * columnCount + column
        End Function
    End Class
End Namespace
