Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon
    Public Enum OutputStyleRole
        Body = 0
        Title = 1
        TopColumnHeader = 2
        LeftRowHeader = 3
        Footer = 4
        DiagnosticWarning = 5
        DiagnosticFault = 6
    End Enum

    Public Enum OutputNumberFormatKind
        General = 0
        Numeric = 1
        Percentage = 2
        Scientific = 3
        PValue = 4
    End Enum

    ''' <summary>Semantic intent only; never a host number-format string.</summary>
    Public NotInheritable Class OutputNumberFormatIntent
        Public Sub New(kind As OutputNumberFormatKind, Optional decimalPlaces As Integer? = Nothing)
            If Not [Enum].IsDefined(GetType(OutputNumberFormatKind), kind) Then Throw New ArgumentOutOfRangeException(NameOf(kind))
            If kind = OutputNumberFormatKind.General Then
                If decimalPlaces.HasValue Then Throw New ArgumentException("General format has no precision.", NameOf(decimalPlaces))
            Else
                If Not decimalPlaces.HasValue OrElse decimalPlaces.Value < 0 OrElse decimalPlaces.Value > 16 Then
                    Throw New ArgumentOutOfRangeException(NameOf(decimalPlaces))
                End If
                If kind = OutputNumberFormatKind.PValue AndAlso decimalPlaces.Value < 2 Then Throw New ArgumentOutOfRangeException(NameOf(decimalPlaces))
            End If
            Me.Kind = kind
            Me.DecimalPlaces = decimalPlaces
        End Sub
        Public ReadOnly Property Kind As OutputNumberFormatKind
        Public ReadOnly Property DecimalPlaces As Integer?
    End Class

    ''' <summary>
    ''' One resolved directive per cell. ArtifactId + Row + Column is its composite identity;
    ''' LocalId is stable within that artifact. All indices are zero-based table-relative,
    ''' never worksheet addresses. Body role survives a p-value overlay.
    ''' </summary>
    Public NotInheritable Class OutputCellStyle
        Friend Sub New(artifactId As String, row As Integer, column As Integer, cell As OutputCell,
                       role As OutputStyleRole, numberFormat As OutputNumberFormatIntent,
                       isPValue As Boolean, significant As Boolean, preview As String)
            Me.ArtifactId = artifactId
            Me.Row = row
            Me.Column = column
            Me.LocalId = row.ToString(CultureInfo.InvariantCulture) & ":" & column.ToString(CultureInfo.InvariantCulture)
            Me.Cell = cell
            Me.Role = role
            Me.NumberFormat = numberFormat
            Me.IsPValue = isPValue
            Me.IsSignificant = significant
            Me.StrongEmphasis = significant OrElse role = OutputStyleRole.TopColumnHeader OrElse role = OutputStyleRole.LeftRowHeader
            Me.AccessibilityIndicator = If(significant, "significant", String.Empty)
            Me.DisplayPreview = preview
        End Sub
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property LocalId As String
        Public ReadOnly Property Row As Integer
        Public ReadOnly Property Column As Integer
        Public ReadOnly Property Cell As OutputCell
        Public ReadOnly Property Role As OutputStyleRole
        Public ReadOnly Property NumberFormat As OutputNumberFormatIntent
        Public ReadOnly Property IsPValue As Boolean
        Public ReadOnly Property IsSignificant As Boolean
        Public ReadOnly Property StrongEmphasis As Boolean
        Public ReadOnly Property AccessibilityIndicator As String
        ''' <summary>Optional p-value display only; never replaces the numeric Cell.</summary>
        Public ReadOnly Property DisplayPreview As String
    End Class

    Public NotInheritable Class OutputArtifactStylePlan
        Friend Sub New(artifact As OutputArtifact, rows As Integer, columns As Integer,
                       cells As IEnumerable(Of OutputCellStyle), diagnosticRole As OutputStyleRole?)
            ArtifactId = artifact.Id
            Kind = artifact.Kind
            RowCount = rows
            ColumnCount = columns
            Me.Cells = New List(Of OutputCellStyle)(cells).AsReadOnly()
            Me.DiagnosticRole = diagnosticRole
            AccessibilityIndicator = If(diagnosticRole = OutputStyleRole.DiagnosticWarning, "warning",
                If(diagnosticRole = OutputStyleRole.DiagnosticFault, "fault", String.Empty))
        End Sub
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property Kind As OutputArtifactKind
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public ReadOnly Property Cells As IReadOnlyList(Of OutputCellStyle)
        Public ReadOnly Property DiagnosticRole As OutputStyleRole?
        Public ReadOnly Property AccessibilityIndicator As String
    End Class

    ''' <summary>Planner diagnostics do not alter source document diagnostics or annotations.</summary>
    Public NotInheritable Class OutputPlanDiagnostic
        Friend Sub New(artifactId As String, code As String, sourceIndex As Integer?)
            Me.ArtifactId = artifactId
            Me.Code = code
            Me.SourceIndex = sourceIndex
        End Sub
        ''' <summary>Empty for document-wide settings/alpha diagnostics.</summary>
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property Code As String
        ''' <summary>Zero-based position in the offending original annotation list, if any.</summary>
        Public ReadOnly Property SourceIndex As Integer?
    End Class

    ''' <summary>
    ''' Additive v1 in-memory semantic plan, a read-only portable scalar graph. Transport
    ''' adapters must preserve exact decimal precision; normalized JSON fixtures are test-only.
    ''' Artifact order is document order, cells row-major, diagnostics settings then source
    ''' annotations. No host branches, layout, colors, fonts, or executable rendering commands.
    ''' </summary>
    Public NotInheritable Class OutputSemanticPlan
        Friend Sub New(sourceDocument As OutputDocument, policy As OutputPValuePolicy, alpha As Double, previewCultureName As String,
                       artifacts As IEnumerable(Of OutputArtifactStylePlan), diagnostics As IEnumerable(Of OutputPlanDiagnostic))
            Me.SourceDocument = sourceDocument
            Me.Policy = policy
            Me.Alpha = alpha
            Me.PreviewCultureName = previewCultureName
            Me.Artifacts = New List(Of OutputArtifactStylePlan)(artifacts).AsReadOnly()
            Me.Diagnostics = New List(Of OutputPlanDiagnostic)(diagnostics).AsReadOnly()
        End Sub
        Public ReadOnly Property SchemaVersion As Integer = 1
        ' Adapter provenance only; not a public DTO field or serialized member.
        Friend ReadOnly Property SourceDocument As OutputDocument
        Public ReadOnly Property Policy As OutputPValuePolicy
        Public ReadOnly Property Alpha As Double
        ''' <summary>Nothing means no preview; empty string means explicit invariant culture.</summary>
        Public ReadOnly Property PreviewCultureName As String
        Public ReadOnly Property Artifacts As IReadOnlyList(Of OutputArtifactStylePlan)
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of OutputPlanDiagnostic)
    End Class
End Namespace
