Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Public NotInheritable Class MultipleComparisonGeometry35
Private ReadOnly estimatesData As Double(), lowerData As Double(), upperData As Double(), yData As Double(), plusData As Double(), minusData As Double()
Public Sub New(estimates As Double(),lower As Double(),upper As Double())
If estimates Is Nothing Then Throw New ArgumentNullException(NameOf(estimates))
If lower Is Nothing Then Throw New ArgumentNullException(NameOf(lower))
If upper Is Nothing Then Throw New ArgumentNullException(NameOf(upper))
Dim n=estimates.Length
If n=0 OrElse lower.Length<>n OrElse upper.Length<>n Then Throw New ArgumentException("Comparison arrays must have equal non-zero lengths.",NameOf(estimates))
estimatesData=DirectCast(estimates.Clone(),Double()):lowerData=DirectCast(lower.Clone(),Double()):upperData=DirectCast(upper.Clone(),Double())
yData=ComparisonCoordinatesCore.OrdinalY(n)
ReDim plusData(n-1):ReDim minusData(n-1)
Dim minCI=lowerData(0), maxCI=upperData(0)
For i As Integer=0 To n-1
plusData(i)=Math.Max(0.0,upperData(i)-estimatesData(i))
minusData(i)=Math.Max(0.0,estimatesData(i)-lowerData(i))
minCI=Math.Min(minCI,lowerData(i)):maxCI=Math.Max(maxCI,upperData(i))
Next
Dim bounds=ComparisonCoordinatesCore.IncludeZeroBounds(minCI,maxCI)
MinimumX=bounds.Min:MaximumX=bounds.Max
ZeroReference=ChartReferenceSeries.Vertical(0.0R,0.5R,n+0.5R)
End Sub
Public ReadOnly Property MinimumX As Double
Public ReadOnly Property MaximumX As Double
Public ReadOnly Property ZeroReference As ChartReferenceSeries
Public ReadOnly Property Estimates As Double()
Get
Return DirectCast(estimatesData.Clone(),Double())
End Get
End Property
Public ReadOnly Property LowerLimits As Double()
Get
Return DirectCast(lowerData.Clone(),Double())
End Get
End Property
Public ReadOnly Property UpperLimits As Double()
Get
Return DirectCast(upperData.Clone(),Double())
End Get
End Property
Public ReadOnly Property YValues As Double()
Get
Return DirectCast(yData.Clone(),Double())
End Get
End Property
Public ReadOnly Property PlusError As Double()
Get
Return DirectCast(plusData.Clone(),Double())
End Get
End Property
Public ReadOnly Property MinusError As Double()
Get
Return DirectCast(minusData.Clone(),Double())
End Get
End Property
End Class
