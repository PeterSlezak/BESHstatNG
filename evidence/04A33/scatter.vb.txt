Option Explicit On
Option Strict On
Imports System
Imports System.Linq
''' <summary>Ordered panel coordinates, with i outer / j inner and zero-based source rows.
''' Correlation text/format and the Windows forced .1 constructor margin are host policies.</summary>
Public NotInheritable Class ScatterMatrixPanelData
    Private ReadOnly storedValues(,) As Double
    Private ReadOnly cx(), cy(), rx(), ry() As Double
    Friend Sub New(data(,) As Double, correlationX() As Double, correlationY() As Double, regressionX() As Double, regressionY() As Double)
        storedValues = DirectCast(data.Clone(), Double(,))
        cx = DirectCast(correlationX.Clone(), Double()) : cy = DirectCast(correlationY.Clone(), Double())
        rx = DirectCast(regressionX.Clone(), Double()) : ry = DirectCast(regressionY.Clone(), Double())
    End Sub
    Public ReadOnly Property Values As Double(,)
        Get
            Return DirectCast(storedValues.Clone(), Double(,))
        End Get
    End Property
    Public ReadOnly Property CorrelationX As Double()
        Get
            Return DirectCast(cx.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property CorrelationY As Double()
        Get
            Return DirectCast(cy.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property RegressionX As Double()
        Get
            Return DirectCast(rx.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property RegressionY As Double()
        Get
            Return DirectCast(ry.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property RegressionBreakMask As Boolean()
        Get
            Return rx.Select(Function(v) v = -100).ToArray()
        End Get
    End Property
    Public ReadOnly Property SourceOrdinals As Integer()
        Get
            Return Enumerable.Range(0, storedValues.GetLength(0)).ToArray()
        End Get
    End Property
    Public ReadOnly Property PanelColumns As Integer()
        Get
            Dim p = storedValues.GetLength(1)
            Return Enumerable.Range(0, p * p).Select(Function(i) i \ p).ToArray()
        End Get
    End Property
    Public ReadOnly Property PanelRows As Integer()
        Get
            Dim p = storedValues.GetLength(1)
            Return Enumerable.Range(0, p * p).Select(Function(i) i Mod p).ToArray()
        End Get
    End Property
    Public Function PanelX(column As Integer) As Double()
        Return DataManagement.ArrayUtilities.GetColumn(storedValues, column)
    End Function
    Public Function PanelY(row As Integer) As Double()
        Return DataManagement.ArrayUtilities.GetColumn(storedValues, row)
    End Function
End Class
