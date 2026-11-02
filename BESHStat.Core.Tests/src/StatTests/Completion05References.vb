Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO

' Test-only independent candidate transport. This file is linked into the three test assemblies.
Friend NotInheritable Class Completion05References
    Public Shared Iterator Function Cases() As IEnumerable(Of Object())
        Dim path=System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"TestData","04A05Completion","paired-candidates-v1.csv")
        Dim lines=File.ReadAllLines(path)
        For index=1 To lines.Length-1
            Dim f=lines(index).Split(","c)
            Yield New Object() {f(0),Integer.Parse(f(1),CultureInfo.InvariantCulture),Integer.Parse(f(2),CultureInfo.InvariantCulture),
                Integer.Parse(f(3),CultureInfo.InvariantCulture),Integer.Parse(f(4),CultureInfo.InvariantCulture),
                Parse(f(5)),Parse(f(6)),Parse(f(7)),Parse(f(8)),Parse(f(9)),Parse(f(10)),Parse(f(11))}
        Next
    End Function
    Private Shared Function Parse(value As String) As Double
        Return Double.Parse(value,CultureInfo.InvariantCulture)
    End Function
    Public Shared Function Close(expected As Double,actual As Double,absolute As Double,relative As Double) As Boolean
        If Double.IsNaN(expected) Then Return Double.IsNaN(actual)
        If Double.IsInfinity(expected) Then Return expected=actual
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then Return False
        Return Math.Abs(expected-actual)<=absolute+relative*Math.Abs(expected)
    End Function
    Public Shared Function PTolerance(expected As Double) As Double
        Return Math.Max(1.0E-315,Math.Min(2.0E-12,Math.Abs(expected)*2.0E-8))
    End Function
End Class
