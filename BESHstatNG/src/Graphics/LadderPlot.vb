Option Explicit On
Option Strict On
Option Infer On

Imports System

''' <summary>Windows compatibility façade for ladder/slope plot data preparation.</summary>
Public NotInheritable Class LadderPlot
    Private Sub New()
    End Sub

    ''' <summary>Normal X coordinate for the First/Before side.</summary>
    Public Const FirstBaseXPosition As Double = 1.0R

    ''' <summary>Normal X coordinate for the Second/After side.</summary>
    Public Const SecondBaseXPosition As Double = 2.0R

    ''' <summary>Prepare paired rows, optional labels/groups and collision-aware jitter.</summary>
    Public Shared Function Compute(firstValues As Array,
                                   secondValues As Array,
                                   Optional recordLabels As Array = Nothing,
                                   Optional groupingValues As Array = Nothing,
                                   Optional options As LadderPlotOptions = Nothing) As LadderPlotResult
        Return LadderPlotCore.Compute(firstValues, secondValues, recordLabels, groupingValues, options)
    End Function

    ''' <summary>Compatibility overload for paired values with explicit options.</summary>
    Public Shared Function Compute(firstValues As Array,
                                   secondValues As Array,
                                   options As LadderPlotOptions) As LadderPlotResult
        Return LadderPlotCore.Compute(firstValues, secondValues, options)
    End Function

    ''' <summary>Compatibility overload with explicit record labels.</summary>
    Public Shared Function ComputeWithLabels(firstValues As Array,
                                             secondValues As Array,
                                             recordLabels As Array,
                                             Optional options As LadderPlotOptions = Nothing) As LadderPlotResult
        Return LadderPlotCore.ComputeWithLabels(firstValues, secondValues, recordLabels, options)
    End Function

    ''' <summary>Compatibility overload with explicit labels and groups.</summary>
    Public Shared Function ComputeWithGroups(firstValues As Array,
                                             secondValues As Array,
                                             recordLabels As Array,
                                             groupingValues As Array,
                                             Optional options As LadderPlotOptions = Nothing) As LadderPlotResult
        Return LadderPlotCore.ComputeWithGroups(firstValues, secondValues, recordLabels, groupingValues, options)
    End Function
End Class
