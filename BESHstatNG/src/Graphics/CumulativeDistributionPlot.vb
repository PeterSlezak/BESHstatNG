Option Explicit On
Option Strict On
Option Infer On

Imports System

''' <summary>
''' Windows compatibility entry point for CDF/ECDF calculations.
''' Calculation options, result payloads, fitting and validation live in BESHStat.Core;
''' Excel chart rendering remains in CumulativeDistributionPlotExcel.
''' </summary>
Public NotInheritable Class CumulativeDistributionPlot

    Private Sub New()
    End Sub

    ''' <summary>Computes an ECDF and optional fitted distribution for one sample.</summary>
    Public Shared Function Compute(values As Array,
                                   Optional seriesName As String = "",
                                   Optional options As CumulativeDistributionPlotOptions = Nothing) As CumulativeDistributionPlotResult
        Return CumulativeDistributionPlotCore.Compute(values, seriesName, options)
    End Function

    ''' <summary>Computes per-group ECDF/CDF results in first-occurrence order.</summary>
    Public Shared Function ComputeGrouped(values As Array,
                                          groupingValues As Array,
                                          Optional variableName As String = "",
                                          Optional options As CumulativeDistributionPlotOptions = Nothing) As CumulativeDistributionPlotSetResult
        Return CumulativeDistributionPlotCore.ComputeGrouped(values, groupingValues, variableName, options)
    End Function

    ''' <summary>Evaluates an available fitted theoretical CDF.</summary>
    Public Shared Function EvaluateFittedCdf(x As Double, fit As CumulativeDistributionFit) As Double
        Return CumulativeDistributionPlotCore.EvaluateFittedCdf(x, fit)
    End Function

End Class
