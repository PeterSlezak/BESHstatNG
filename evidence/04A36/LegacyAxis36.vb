Option Strict On
Option Explicit On
Option Infer On
Imports System
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Namespace Global.Legacy36
Public Module OriginalAxis36
' Earlier accepted independently compiled 04A.22 renderer arithmetic, including
' padding/natural-zero checks. Entry Framework tests proved it against live35.
Public Function Bounds(panel As SpcPanelSeriesProjection,spec As SpcSpecificationLimits,showSpecs As Boolean,showTarget As Boolean) As Double()
Dim options As New Spc22LegacyRenderer.SpcControlChartAppearanceOptions With {.ShowSpecificationLimits=showSpecs,.ShowTargetLine=showTarget}
Dim fit As New SpcFitResult(New SpcFitRequest(SpcChartType.Individuals,SpcInputData.FromIndividualSequence(New Double(){1,2,3})),New SpcPanelResult(){panel.Panel})
Dim axis=Spc22LegacyRenderer.BuildSharedAxisContext(fit,options)
Dim data=Spc22LegacyRenderer.BuildPanelChartData(panel.Panel,axis)
Dim result=Spc22LegacyRenderer.ComputeYAxisBounds(panel.Panel,data,spec,options)
Return New Double(){result.Minimum,result.Maximum}
End Function
End Module
End Namespace
