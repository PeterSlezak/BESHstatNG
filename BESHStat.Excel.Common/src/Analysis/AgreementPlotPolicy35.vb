Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon
Public NotInheritable Class AgreementPlotSeries35
Private ReadOnly xData As Double(),yData As Double()
Public Sub New(name As String,x As Double(),y As Double())
Me.Name=name:xData=DirectCast(x.Clone(),Double()):yData=DirectCast(y.Clone(),Double())
End Sub
Public ReadOnly Property Name As String
Public ReadOnly Property XValues As Double()
Get
Return DirectCast(xData.Clone(),Double())
End Get
End Property
Public ReadOnly Property YValues As Double()
Get
Return DirectCast(yData.Clone(),Double())
End Get
End Property
End Class
Public Module AgreementPlotPolicy35
Public Function Regression(minimum As Double,maximum As Double,intercept As Double,slope As Double) As IReadOnlyList(Of AgreementPlotSeries35)
Dim fit=ChartReferenceSeries.Affine(minimum,maximum,intercept,slope),identity=ChartReferenceSeries.Identity(minimum,maximum)
Return Array.AsReadOnly(New AgreementPlotSeries35(){New AgreementPlotSeries35("Regression line",identity.XValues,fit.YValues),New AgreementPlotSeries35("Unity line (y = x)",identity.XValues,identity.XValues)})
End Function
Public Function Identity(minimum As Double,maximum As Double) As IReadOnlyList(Of AgreementPlotSeries35)
Dim line=ChartReferenceSeries.Identity(minimum,maximum)
Return Array.AsReadOnly(New AgreementPlotSeries35(){New AgreementPlotSeries35("Identity",line.XValues,line.XValues)})
End Function
Public Function Bland(x As Double(),bias As Double,lower As Double,upper As Double,show As Boolean,sx As Double(),sy As Double()) As IReadOnlyList(Of AgreementPlotSeries35)
Dim span=Chart34.AgreementSeries.AgreementSeriesPreparation34.BaseSpan(x)
Dim result As New List(Of AgreementPlotSeries35)
Dim values As Double()={bias,lower,upper},names As String()={"Bias","Lower LoA","Upper LoA"}
For i As Integer=0 To 2
Dim line=ChartReferenceSeries.Horizontal(span.Min,span.Max,values(i))
result.Add(New AgreementPlotSeries35(names(i),line.XValues,line.YValues))
Next
If show AndAlso sx IsNot Nothing AndAlso sy IsNot Nothing Then result.Add(New AgreementPlotSeries35("Subject means",sx,sy))
Return result.AsReadOnly()
End Function
End Module
End Namespace
