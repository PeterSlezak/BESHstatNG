Option Explicit On
Option Strict On

Imports System

''' <summary>Projection of the accepted ROC curve, without recalculating statistics.
''' Cutoff i belongs to plot slot i+1 (zero-based). Slot zero has no cutoff.
''' The last cutoff retains the forced terminal point, including exceptional inputs.
''' Vectors may be Nothing for an uncomputed Windows compatibility snapshot.</summary>
Public NotInheritable Class RocPlotSeriesData
    Private ReadOnly xData() As Double, yData() As Double, cutData() As Double, specData() As Double

    Public Sub New(result As RocDataResult)
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        xData = result.FalsePositiveRate : yData = result.Sensitivity
        cutData = result.Cutoffs : specData = result.Specificity
    End Sub

    ''' <summary>Snapshot the current adapter state, including its pre-compute Nothing
    ''' vectors. No validation/filtering or new endpoint/cutoff policy is applied.</summary>
    Public Sub New(falsePositiveRate() As Double, sensitivity() As Double, cutoffs() As Double, specificity() As Double)
        xData = Copy(falsePositiveRate) : yData = Copy(sensitivity)
        cutData = Copy(cutoffs) : specData = Copy(specificity)
    End Sub

    Private Shared Function Copy(values() As Double) As Double()
        Return If(values Is Nothing, Nothing, DirectCast(values.Clone(), Double()))
    End Function

    Public ReadOnly Property X As Double()
        Get
            Return Copy(xData)
        End Get
    End Property

    Public ReadOnly Property Y As Double()
        Get
            Return Copy(yData)
        End Get
    End Property

    Public ReadOnly Property Cutoffs As Double()
        Get
            Return Copy(cutData)
        End Get
    End Property

    Public ReadOnly Property Specificity As Double()
        Get
            Return Copy(specData)
        End Get
    End Property

    Public ReadOnly Property PointCount As Integer
        Get
            Return If(xData Is Nothing, 0, xData.Length)
        End Get
    End Property

    Public ReadOnly Property CutoffCount As Integer
        Get
            Return If(cutData Is Nothing, 0, cutData.Length)
        End Get
    End Property

    ''' <summary>Plot slot for each cutoff. No fabricated cutoff for slot zero.</summary>
    Public ReadOnly Property CutoffPointIndexes As Integer()
        Get
            Dim indexes(CutoffCount - 1) As Integer
            For i As Integer = 0 To indexes.Length - 1
                indexes(i) = i + 1
            Next
            Return indexes
        End Get
    End Property

    Public ReadOnly Property ReferenceX As Double()
        Get
            Return {0.0, 1.0}
        End Get
    End Property

    Public ReadOnly Property ReferenceY As Double()
        Get
            Return {0.0, 1.0}
        End Get
    End Property

    Public ReadOnly Property AxisMinimum As Double = 0.0
    Public ReadOnly Property AxisMaximum As Double = 1.0
    Public ReadOnly Property AxisMajorUnit As Double = 0.2
    Public ReadOnly Property AxisCrossing As Double = 0.0
End Class
