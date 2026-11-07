Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Public Module PolarMeasuredLayoutCore
Public Function PolarSquare(width As Double, height As Double, left As Double, top As Double) As PolarSquareLayout34
Dim side As Double = Math.Min(width,height)
Dim adjustedLeft As Double = left + (width - side) / 2.0R
Dim adjustedTop As Double = top + (height - side) / 2.0R
Return New PolarSquareLayout34(adjustedLeft,adjustedTop,side)
End Function
End Module
Public NotInheritable Class PolarSquareLayout34
    Public ReadOnly Property Left As Double
    Public ReadOnly Property Top As Double
    Public ReadOnly Property Side As Double
    Public Sub New(inputLeft As Double, inputTop As Double, inputSide As Double)
        Me.Left = inputLeft
        Me.Top = inputTop
        Me.Side = inputSide
    End Sub
End Class
