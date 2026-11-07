Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Public NotInheritable Class CoxCurveGeometry36
    Private Sub New()
    End Sub
    Public Shared Sub Validate(survTime As Double(), survProb As Double())
        If survTime Is Nothing Then Throw New ArgumentNullException(NameOf(survTime))
        If survProb Is Nothing Then Throw New ArgumentNullException(NameOf(survProb))
        If survTime.Length <> survProb.Length Then Throw New ArgumentException("survTime and survProb must have the same length.")
    End Sub
    Public Shared Function Axis(survTime As Double()) As ChartAxisScaleResult
        Return ChartAxisScalingCore.Compute(0,survTime.Max())
    End Function
    'Read at each original native assignment, never an earlier constructor snapshot.
    Public Shared Function Snapshot(values As Double()) As Double()
        Return DirectCast(values.Clone(),Double())
    End Function
End Class
