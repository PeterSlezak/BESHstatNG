Option Explicit On
Option Strict On
Option Infer On

Imports System

''' <summary>
''' Windows compatibility façade for Sankey/alluvial data preparation.
''' The numerical backend and result DTOs are owned by BESHStat.Core;
''' Excel graphics rendering remains in SankeyPlotExcel.
''' </summary>
Public NotInheritable Class SankeyPlot
    Private Sub New()
    End Sub


    ''' <summary>Prepare an aggregated, layered Sankey layout from Source/Target rows.</summary>
    Public Shared Function Compute(source As Array,
                                   target As Array,
                                   Optional weights As Double() = Nothing,
                                   Optional sourceStages As Integer() = Nothing,
                                   Optional targetStages As Integer() = Nothing,
                                   Optional stageNames As String() = Nothing,
                                   Optional options As SankeyPlotOptions = Nothing) As SankeyPlotResult
        Return SankeyPlotCore.Compute(source, target, weights, sourceStages, targetStages, stageNames, options)
    End Function

    ''' <summary>Prepare a Sankey layout from a row-by-stage categorical table.</summary>
    Public Shared Function ComputeStaged(stages As Object(,),
                                         Optional weights As Double() = Nothing,
                                         Optional stageNames As String() = Nothing,
                                         Optional options As SankeyPlotOptions = Nothing) As SankeyPlotResult
        Return SankeyPlotCore.ComputeStaged(stages, weights, stageNames, options)
    End Function

End Class
