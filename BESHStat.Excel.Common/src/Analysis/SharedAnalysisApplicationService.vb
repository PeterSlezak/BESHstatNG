Option Explicit On
Option Strict On

Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>
    ''' Stable host-neutral application facade. The legacy enum-based request remains
    ''' binary/source compatible, but execution is now delegated to typed registrations.
    ''' New callers may dispatch directly by the stable analysis ID.
    ''' </summary>
    Public NotInheritable Class SharedAnalysisApplicationService

        Private Sub New()
        End Sub

        ''' <summary>Compatibility entry point used by the Windows Excel-DNA host.</summary>
        Public Shared Function Run(request As SpreadsheetAnalysisRequest) As SpreadsheetAnalysisResult
            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If request.Input Is Nothing Then Throw New ArgumentException("The analysis request has no input.", NameOf(request))

            Dim descriptor As IAnalysisDescriptor = Nothing
            If Not AnalysisCatalog.TryGetByLegacyKind(request.AnalysisKind, descriptor) Then
                Throw New NotSupportedException(
                    "The shared Excel application layer does not support analysis kind " &
                    CInt(request.AnalysisKind).ToString(CultureInfo.InvariantCulture) & ".")
            End If
            Return AnalysisHandlerRegistry.Run(descriptor.Id, request.Input, request.Options)
        End Function

        ''' <summary>Stable-ID entry point for new Excel/Office consumers.</summary>
        Public Shared Function Run(analysisId As String,
                                   input As SpreadsheetAnalysisInput,
                                   Optional options As SpreadsheetAnalysisOptions = Nothing) As SpreadsheetAnalysisResult
            Return AnalysisHandlerRegistry.Run(analysisId, input, options)
        End Function
    End Class

End Namespace
