Option Explicit On
Option Strict On
Option Infer On

Imports System


''' <summary>
''' Windows compatibility entry point for bullet chart preparation.
'''Calculation and result contracts are owned by BESHStat.Core; the Excel renderer
''' and Windows UI continue to use this original public API.
''' </summary>
Public NotInheritable Class BulletChart
    Public Const MinimumQualitativeRanges As Integer = BulletChartCore.MinimumQualitativeRanges
    Public Const MaximumQualitativeRanges As Integer = BulletChartCore.MaximumQualitativeRanges

    Private Sub New()
    End Sub

    Public Shared Function Compute(measureLabels As Array,
                                   actualValues As Array,
                                   targetValues As Array,
                                   qualitativeRanges As Array,
                                   Optional subtitles As Array = Nothing,
                                   Optional scaleMaximums As Array = Nothing,
                                   Optional majorIntervals As Array = Nothing,
                                   Optional directions As Array = Nothing,
                                   Optional options As BulletChartOptions = Nothing) As BulletChartResult
        Return BulletChartCore.Compute(measureLabels,
                                        actualValues,
                                        targetValues,
                                        qualitativeRanges,
                                        subtitles,
                                        scaleMaximums,
                                        majorIntervals,
                                        directions,
                                        options)
    End Function

    Public Shared Function ComputeBasic(measureLabels As Array,
                                        actualValues As Array,
                                        targetValues As Array,
                                        qualitativeRanges As Array,
                                        Optional options As BulletChartOptions = Nothing) As BulletChartResult
        Return BulletChartCore.ComputeBasic(measureLabels,
                                            actualValues,
                                            targetValues,
                                            qualitativeRanges,
                                            options)
    End Function

End Class
