Option Explicit On
Option Strict On

Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GLMHostInteractionTests

    <TestCleanup>
    Public Sub Cleanup()
        GLMHostInteraction.ClearQuasiSeparationDecision()
    End Sub

    <TestMethod>
    Public Sub QuasiSeparation_without_host_callback_continues()
        GLMHostInteraction.ClearQuasiSeparationDecision()

        Assert.IsTrue(GLMHostInteraction.ShouldContinueAfterQuasiSeparation())
    End Sub

    <TestMethod>
    Public Sub QuasiSeparation_invokes_registered_host_callback()
        Dim seenPrompt As String = Nothing
        Dim seenTitle As String = Nothing

        GLMHostInteraction.ConfigureQuasiSeparationDecision(
            Function(prompt As String, title As String) As Boolean
                seenPrompt = prompt
                seenTitle = title
                Return False
            End Function)

        Assert.IsFalse(GLMHostInteraction.ShouldContinueAfterQuasiSeparation())
        StringAssert.Contains(seenPrompt, "Quasi-separation")
        StringAssert.Contains(seenPrompt, "Results may be misleading")
        Assert.AreEqual("Continue?", seenTitle)
    End Sub
End Class
