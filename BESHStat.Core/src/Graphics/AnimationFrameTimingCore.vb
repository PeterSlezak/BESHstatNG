Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic

Public Module AnimationFrameTimingCore
Public Function MsToCentiseconds(ms As Integer) As Integer
        Dim safeMs = If(ms < 0, 0, ms)
        Return Math.Max(1, CInt(Math.Round(safeMs / 10.0)))
    End Function
End Module
