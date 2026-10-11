Option Explicit On
Option Strict On
Option Infer On

Imports System




''' <summary>
''' Windows compatibility façade for dumbbell data preparation.
''' Calculation, options and result contracts are now owned by BESHStat.Core;
''' Excel rendering remains in DumbbellPlotExcel and the Windows form.
''' </summary>
Public NotInheritable Class DumbbellPlot
    Private Sub New()
    End Sub

    ''' <summary>Prepare endpoint categories and optional auxiliary observations.</summary>
    Public Shared Function Compute(categories As Array,
                                   firstValues As Array,
                                   secondValues As Array,
                                   Optional options As DumbbellPlotOptions = Nothing,
                                   Optional auxiliaryCategories As Array = Nothing,
                                   Optional auxiliaryValues As Array = Nothing) As DumbbellPlotResult
        Return DumbbellPlotCore.Compute(categories, firstValues, secondValues, options, auxiliaryCategories, auxiliaryValues)
    End Function

    ''' <summary>Positional overload used by the Windows UI with auxiliary observations.</summary>
    Public Shared Function ComputeWithAuxiliary(categories As Array,
                                                firstValues As Array,
                                                secondValues As Array,
                                                auxiliaryCategories As Array,
                                                auxiliaryValues As Array,
                                                Optional options As DumbbellPlotOptions = Nothing) As DumbbellPlotResult
        Return DumbbellPlotCore.ComputeWithAuxiliary(categories, firstValues, secondValues, auxiliaryCategories, auxiliaryValues, options)
    End Function

End Class
