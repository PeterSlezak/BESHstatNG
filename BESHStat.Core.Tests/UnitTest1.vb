Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStat.Core

<TestClass>
Public Class CoreInfrastructureTests

    <TestMethod>
    Public Sub Core_ProjectReference_Works()

        Dim result As Double = BESHStatNG.CoreSmokeTestHelper.Add(2.0, 3.0)

        Assert.AreEqual(5.0, result, 0.0)

    End Sub

End Class
