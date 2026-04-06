Option Strict On
Option Explicit On

Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

' One-liner wiring for any Help button/menu item in WinForms
Public Module HelpWiring

    ' For normal WinForms controls (Button, LinkLabel, PictureBox, etc.)
    <Extension>
    Public Sub WireHelp(frm As Form, helpControl As Control)
        If frm Is Nothing OrElse helpControl Is Nothing Then Return
        AddHandler helpControl.Click, Sub(sender, e) HelpContext.OpenHelp(frm)
    End Sub

    ' For ToolStrip buttons/menu items
    <Extension>
    Public Sub WireHelp(frm As Form, helpItem As ToolStripItem)
        If frm Is Nothing OrElse helpItem Is Nothing Then Return
        AddHandler helpItem.Click, Sub(sender, e) HelpContext.OpenHelp(frm)
    End Sub

End Module
