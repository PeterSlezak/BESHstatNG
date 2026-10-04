Option Explicit On
Option Strict On

Namespace Agreement

    Friend NotInheritable Class PassingBablokPlotData
        Public Property XValues As Double()
        Public Property YValues As Double()
        Public Property XName As String
        Public Property YName As String
        Public Property Title As String
        Public Property MinX As Double
        Public Property MaxX As Double
        Public Property Intercept As Double
        Public Property Slope As Double
    End Class

    Friend NotInheritable Class BlandAltmanPlotData
        Public Property XValues As Double()
        Public Property YValues As Double()
        Public Property XLabel As String
        Public Property YLabel As String
        Public Property Title As String
        Public Property Bias As Double
        Public Property LowerLoA As Double
        Public Property UpperLoA As Double
        Public Property SubjectMeanX As Double()
        Public Property SubjectMeanY As Double()
        Public Property ShowSubjectMeans As Boolean
    End Class

    Friend NotInheritable Class LinConcordancePlotData
        Public Property XValues As Double()
        Public Property YValues As Double()
        Public Property XName As String
        Public Property YName As String
        Public Property Title As String
        Public Property MinValue As Double
        Public Property MaxValue As Double
    End Class

    Friend NotInheritable Class DemingPlotData
        Public Property XValues As Double()
        Public Property YValues As Double()
        Public Property XName As String
        Public Property YName As String
        Public Property Title As String
        Public Property MinX As Double
        Public Property MaxX As Double
        Public Property Intercept As Double
        Public Property Slope As Double
    End Class

End Namespace
