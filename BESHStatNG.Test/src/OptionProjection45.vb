Option Strict On
Option Explicit On
Imports System
Namespace Harness45
Friend Class FakeControl45
 Friend Enabled As Boolean = True
 Friend Visible As Boolean = True
 Friend Checked As Boolean
 Friend Text As String = ""
 Friend Selections As Integer
 Friend Event Click As EventHandler
 Friend Sub FireClick()
  RaiseEvent Click(Me, System.EventArgs.Empty)
 End Sub
 Friend Sub [Select]()
  Selections += 1
 End Sub
End Class
Friend Class FakeRefEdit45
 Friend Enabled As Boolean = True
 Friend txtAddress As New FakeControl45()
End Class
Friend Class OptionProjection45
 Friend Tag As HelpTopic
 Friend RefEdit1 As New FakeRefEdit45(), RefEdit2 As New FakeRefEdit45(), RefEditOutput As New FakeRefEdit45()
 Friend lblRefedit1 As New FakeControl45(), lblRefedit2 As New FakeControl45()
 Friend WithEvents optOutputRange As New FakeControl45(), optWorksheet As New FakeControl45(), optWorkbook As New FakeControl45(), optByColumn As New FakeControl45(), optByID As New FakeControl45()
    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub
    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub
    Private Sub optByColumn_Click(sender As Object, e As System.EventArgs) Handles optByColumn.Click
        If Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Then
            If Me.Tag = HelpTopic.ROCCurve Then
                Me.lblRefedit1.Text = "Group with characteristic present (patients)"
                Me.lblRefedit2.Text = "Group with characteristic absent (controls)"
            ElseIf Me.Tag = HelpTopic.UnpairedTTest Then
                Me.ApplyUnpairedTtestInputLabels()
            Else
                Me.lblRefedit1.Text = "Data: Group 1"
                Me.lblRefedit2.Text = "Data: Group 2"
            End If

            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()
        Else
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.Enabled = False
            Me.lblRefedit1.Enabled = False
            If Me.Tag = HelpTopic.ViolinPlot OrElse Me.Tag = HelpTopic.CategoricalHistogram OrElse Me.Tag = HelpTopic.CDFplot Then
                Me.lblRefedit2.Text = "Data columns:"
            End If
            Me.RefEdit2.txtAddress.Select()
        End If
    End Sub
    Private Sub optByID_Click(sender As Object, e As System.EventArgs) Handles optByID.Click
        If Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Then
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
        Else
            Me.RefEdit1.Enabled = True
            Me.lblRefedit1.Enabled = True
            If Me.Tag = HelpTopic.ViolinPlot OrElse Me.Tag = HelpTopic.CategoricalHistogram OrElse Me.Tag = HelpTopic.CDFplot Then
                Me.lblRefedit1.Text = "Group ID:"
                Me.lblRefedit2.Text = "Data:"
            End If
        End If
        Me.RefEdit1.txtAddress.Select()
    End Sub
    Private Sub ApplyUnpairedTtestInputLabels()
        If Me.optByColumn.Checked Then
            Me.lblRefedit1.Text = "Control / Reference group:"
            Me.lblRefedit2.Text = "Experimental / Test group:"
        Else
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
        End If
    End Sub
End Class
End Namespace
