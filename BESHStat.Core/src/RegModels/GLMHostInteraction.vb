Option Explicit On
Option Strict On

Imports System

Namespace regression

    ''' <summary>
    ''' Host-neutral callback seam for GLM decisions that require user interaction in desktop hosts.
    ''' </summary>
    ''' <remarks>
    ''' Core calculations never display UI directly. If no host callback is registered, quasi-separation
    ''' warnings default to continuing the calculation, which is suitable for tests and non-interactive hosts.
    ''' </remarks>
    Public NotInheritable Class GLMHostInteraction
        Private Sub New()
        End Sub

        Private Shared pQuasiSeparationDecision As Func(Of String, String, Boolean)

        Public Shared Sub ConfigureQuasiSeparationDecision(handler As Func(Of String, String, Boolean))
            pQuasiSeparationDecision = handler
        End Sub

        Public Shared Sub ClearQuasiSeparationDecision()
            pQuasiSeparationDecision = Nothing
        End Sub

        Friend Shared Function ShouldContinueAfterQuasiSeparation() As Boolean
            Const prompt As String = "Quasi-separation of the iterative algorithm."
            Const detail As String = "Results may be misleading."
            Const title As String = "Continue?"

            Dim handler As Func(Of String, String, Boolean) = pQuasiSeparationDecision
            If handler Is Nothing Then Return True

            Return handler(prompt & Environment.NewLine & Environment.NewLine & detail, title)
        End Function
    End Class

End Namespace
