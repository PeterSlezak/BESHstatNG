Option Explicit On
Option Strict On
Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
<TestClass> Public Class Graphics35GeometryTests
<DataTestMethod><DataRow(-2.0,-4.0,1.0)><DataRow(2.0,4.0,1.0)><DataRow(0.0,0.0,0.0)>
Public Sub ClampedArmsAndZeroBounds(estimate As Double,lower As Double,upper As Double)
Dim x As Double()={estimate},lo As Double()={lower},hi As Double()={upper}
Dim p As New MultipleComparisonGeometry35(x,lo,hi)
Assert.AreEqual(Math.Max(0,upper-estimate),p.PlusError(0)):Assert.AreEqual(Math.Max(0,estimate-lower),p.MinusError(0))
Assert.IsTrue(p.MinimumX<=0):Assert.IsTrue(p.MaximumX>=0)
x(0)=999:lo(0)=999:hi(0)=999
Assert.AreEqual(estimate,p.Estimates(0)):Assert.AreEqual(lower,p.LowerLimits(0)):Assert.AreEqual(upper,p.UpperLimits(0))
Dim values=p.YValues:values(0)=999:Assert.AreEqual(1.0,p.YValues(0))
End Sub
<TestMethod> Public Sub ExplicitNewGeometryInputContract()
Assert.ThrowsException(Of ArgumentNullException)(Sub() Create(Nothing,New Double(){1},New Double(){1}))
Assert.ThrowsException(Of ArgumentException)(Sub() Create(New Double(){},New Double(){},New Double(){}))
Assert.ThrowsException(Of ArgumentException)(Sub() Create(New Double(){1},New Double(){},New Double(){1}))
End Sub
Private Shared Function Create(x As Double(),lower As Double(),upper As Double()) As MultipleComparisonGeometry35
Return New MultipleComparisonGeometry35(x,lower,upper)
End Function
End Class
