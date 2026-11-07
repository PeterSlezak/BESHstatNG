Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon.Chart34.CdfSeries
Public NotInheritable Class SeriesPayload34
Private ReadOnly _x As Double(), _y As Double(), _amounts As Double()
Private ReadOnly _labels As String(), _source As Integer()
Public ReadOnly Property Visible As Boolean
Public Sub New(x As Double(), y As Double(), Optional amounts As Double() = Nothing, Optional labels As String() = Nothing, Optional source As Integer() = Nothing, Optional isVisible As Boolean = True)
_x = DirectCast(x.Clone(), Double())
_y = DirectCast(y.Clone(), Double())
_amounts = If(amounts Is Nothing, New Double(){}, DirectCast(amounts.Clone(), Double()))
_labels = If(labels Is Nothing, New String(){}, DirectCast(labels.Clone(), String()))
_source = If(source Is Nothing, New Integer(){}, DirectCast(source.Clone(), Integer()))
Visible = isVisible
End Sub
Public ReadOnly Property XValues As Double()
Get
Return DirectCast(_x.Clone(), Double())
End Get
End Property
Public ReadOnly Property YValues As Double()
Get
Return DirectCast(_y.Clone(), Double())
End Get
End Property
Public ReadOnly Property Amounts As Double()
Get
Return DirectCast(_amounts.Clone(), Double())
End Get
End Property
Public ReadOnly Property Labels As String()
Get
Return DirectCast(_labels.Clone(), String())
End Get
End Property
Public ReadOnly Property SourceOrdinals As Integer()
Get
Return DirectCast(_source.Clone(), Integer())
End Get
End Property
End Class
Public Module CdfSeriesPreparation34
Public Function PercentileReference(xCorner As Double, yCorner As Double, minimumX As Double, minimumY As Double) As SeriesPayload34
If Double.IsNaN(xCorner) OrElse Double.IsInfinity(xCorner) OrElse Double.IsNaN(yCorner) OrElse Double.IsInfinity(yCorner) Then Return Nothing
Dim xValues() As Double = {minimumX, xCorner, xCorner}
Dim yValues() As Double = {yCorner, yCorner, minimumY}
Return New SeriesPayload34(xValues,yValues)
End Function

End Module
End Namespace
