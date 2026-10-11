Option Explicit On
Option Strict On

Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>
    ''' Generic host-neutral application dispatcher used by both Excel-DNA and Office.js hosts.
    ''' It is intentionally small: each analysis keeps its own strongly typed service/options,
    ''' while hosts call one stable request/result boundary.
    ''' </summary>
    Public NotInheritable Class SharedAnalysisApplicationService

        Private Sub New()
        End Sub

        Public Shared Function Run(request As SpreadsheetAnalysisRequest) As SpreadsheetAnalysisResult
            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If request.Input Is Nothing Then Throw New ArgumentException("The analysis request has no input.", NameOf(request))

            Select Case request.AnalysisKind
                Case SpreadsheetAnalysisKind.OneWayAnova
                    Return RunOneWayAnova(request)
                Case Else
                    Throw New NotSupportedException(
                        "The shared Excel application layer does not support analysis kind " &
                        CInt(request.AnalysisKind).ToString(Globalization.CultureInfo.InvariantCulture) & ".")
            End Select
        End Function

        Private Shared Function RunOneWayAnova(request As SpreadsheetAnalysisRequest) As SpreadsheetAnalysisResult
            Dim options As OneWayAnovaAnalysisOptions = Nothing
            If request.Options IsNot Nothing Then
                options = TryCast(request.Options, OneWayAnovaAnalysisOptions)
                If options Is Nothing Then
                    Throw New ArgumentException(
                        "One-way ANOVA requires OneWayAnovaAnalysisOptions.",
                        NameOf(request))
                End If
            End If

            Dim result As OneWayAnovaApplicationResult

            If TypeOf request.Input Is SpreadsheetRangeData Then
                result = OneWayAnovaApplicationService.Run(
                    DirectCast(request.Input, SpreadsheetRangeData),
                    options)
            ElseIf TypeOf request.Input Is GroupedNumericData Then
                result = OneWayAnovaApplicationService.Run(
                    DirectCast(request.Input, GroupedNumericData),
                    options)
            Else
                Throw New ArgumentException(
                    "One-way ANOVA requires SpreadsheetRangeData or GroupedNumericData input.",
                    NameOf(request))
            End If

            Dim metrics As New Dictionary(Of String, Double)(StringComparer.Ordinal) From {
                {AnalysisMetricNames.FStatistic, result.FStatistic},
                {AnalysisMetricNames.PValue, result.PValue}
            }

            If result.HasWelch Then
                metrics(AnalysisMetricNames.WelchFStatistic) = result.WelchFStatistic
                metrics(AnalysisMetricNames.WelchPValue) = result.WelchPValue
                metrics(AnalysisMetricNames.WelchDenominatorDf) = result.WelchDenominatorDf
            End If

            Return New SpreadsheetAnalysisResult(
                SpreadsheetAnalysisKind.OneWayAnova,
                result.Tables,
                metrics,
                result)
        End Function
    End Class

End Namespace
