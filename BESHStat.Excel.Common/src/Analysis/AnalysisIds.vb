Option Explicit On
Option Strict On

Namespace ExcelCommon

    ''' <summary>Stable, host-independent identifiers for published analyses.</summary>
    ''' <remarks>Never serialize the numeric SpreadsheetAnalysisKind enum as the feature identity.</remarks>
    Public NotInheritable Class AnalysisIds

        Private Sub New()
        End Sub

        Public Const OneWayAnova As String = "anova.oneway"
        Public Const KruskalWallis As String = "nonparametric.kruskal-wallis"

    End Class

End Namespace
