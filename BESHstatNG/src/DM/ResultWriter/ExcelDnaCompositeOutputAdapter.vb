Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.ExcelCommon

' Opt-in insertion seam only. No production form, destination acquisition or COM call is added.
Friend NotInheritable Class ExcelDnaCompositeCommand
    Private ReadOnly table As OutputTableArtifact
    Friend Sub New(placement As CompositeOutputPlacement, source As OutputDocument, format As String, alpha As Double)
        Me.Placement = placement : NumberFormat = format : Me.Alpha = alpha
        If placement.Item.Kind = CompositeOutputKind.Table Then table = DirectCast(source.Artifacts(placement.SourceIndex.Value), OutputTableArtifact)
        Text = placement.Item.Text
        If placement.Item.Kind = CompositeOutputKind.Link Then Text &= " [link not active: " & placement.Item.LinkTarget & "]"
    End Sub
    Public ReadOnly Property Placement As CompositeOutputPlacement
    Public ReadOnly Property NumberFormat As String
    Public ReadOnly Property Alpha As Double
    Public ReadOnly Property Text As String
    Public ReadOnly Property Model As ResultTableOutputModel
        Get
            Return If(table Is Nothing, Nothing, table.Model)
        End Get
    End Property
End Class

Friend NotInheritable Class ExcelDnaPreparedCompositeOutput
    Friend Sub New(commands As List(Of ExcelDnaCompositeCommand))
        Me.Commands = commands.AsReadOnly()
    End Sub
    Public ReadOnly Property Commands As IReadOnlyList(Of ExcelDnaCompositeCommand)

    ' All commands are prepared before execution. Each callback receives the actual owned
    ' rectangle. Text sink MUST insert literally and keep text inside those bounds. Links
    ' degrade explicitly; charts are placeholders. Late errors propagate without rollback.
    Public Sub Execute(writer As ExcelDnaResultWriter, writeLiteralBlock As Action(Of ExcelDnaCompositeCommand))
        If writer Is Nothing OrElse writeLiteralBlock Is Nothing Then Throw New ArgumentNullException("sink")
        For Each command As ExcelDnaCompositeCommand In Commands
            Dim placement = command.Placement
            If placement.Bounds Is Nothing Then Continue For
            If placement.Item.Kind = CompositeOutputKind.Table Then
                writer.WriteOutputBlockCore(New ResultTableOutputBlock(placement.Bounds.StartRow,
                    placement.Bounds.StartColumn, command.Model), command.NumberFormat, command.Alpha, False, placement.Bounds)
            Else
                writeLiteralBlock(command)
            End If
        Next
    End Sub
End Class

Friend NotInheritable Class ExcelDnaCompositeOutputAdapter
    Public Shared Function Prepare(plan As CompositeOutputPlan, semantics As OutputSemanticPlan) As ExcelDnaPreparedCompositeOutput
        ' Same bounded projection/provenance validation as Office; no second layout/style policy.
        Dim validated = OfficeCompositeOutputPlanAdapter.Prepare(plan, semantics)
        Return New ExcelDnaPreparedCompositeOutput(plan.Items.Select(Function(p) New ExcelDnaCompositeCommand(
            p, plan.SourceDocument, validated.TablePlan.PvalueNumberFormat, semantics.Alpha)).ToList())
    End Function
End Class
