Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon
    ''' <summary>Immutable descriptive projection. No input, worksheet, handler or host is retained.</summary>
    Public NotInheritable Class FeatureAvailabilityInfo
        Friend Sub New(help As HelpDestination, registered As Boolean, preflight As CapabilityPreflightResult)
            Me.Help = help
            IsRegisteredInCommon = registered
            IsSupportedOnHost = preflight.IsSupported
            ReasonCode = preflight.Code
            Reason = preflight.Message
        End Sub

        Public ReadOnly Property Help As HelpDestination
        Public ReadOnly Property IsDocumented As Boolean
            Get
                Return Not Help.IsFallback
            End Get
        End Property
        Public ReadOnly Property IsRegisteredInCommon As Boolean
        Public ReadOnly Property IsSupportedOnHost As Boolean
        Public ReadOnly Property ReasonCode As String
        Public ReadOnly Property Reason As String
    End Class

    Public NotInheritable Class FeatureAvailabilityQuery
        Private Sub New()
        End Sub

        Public Shared Function ByMethodKey(key As String, host As HostCapabilitySnapshot,
                                           destination As SpreadsheetOutputDestination) As FeatureAvailabilityInfo
            Dim help = HelpDestination.ForMethodKey(key)
            Return Evaluate(help, If(help.Metadata Is Nothing, Nothing, help.Metadata.AnalysisId), host, destination)
        End Function

        Public Shared Function ByAnalysisId(id As String, host As HostCapabilitySnapshot,
                                            destination As SpreadsheetOutputDestination) As FeatureAvailabilityInfo
            Return Evaluate(HelpDestination.ForAnalysisId(id), id, host, destination)
        End Function

        Private Shared Function Evaluate(help As HelpDestination, id As String, host As HostCapabilitySnapshot,
                                         destination As SpreadsheetOutputDestination) As FeatureAvailabilityInfo
            Dim descriptor As IAnalysisDescriptor = Nothing
            Dim registered As Boolean = False
            If AnalysisCatalog.TryGetById(id, descriptor) AndAlso descriptor.Availability = AnalysisAvailability.ImplementedInCommon Then
                ' Require real typed registrations; Planned is never evidence of execution.
                registered = True
                For Each inputType As Type In descriptor.InputTypes
                    Dim found As Boolean = False
                    For Each handler As IAnalysisHandler In AnalysisHandlerRegistry.All
                        If String.Equals(handler.AnalysisId, id, StringComparison.Ordinal) AndAlso
                           handler.InputType Is inputType AndAlso handler.OptionsType Is descriptor.OptionsType Then found = True
                    Next
                    If Not found Then registered = False
                Next
            End If
            If Not registered Then
                Dim code = If(help.IsFallback, CapabilityPreflightCodes.UnknownAnalysis, CapabilityPreflightCodes.UnsupportedAnalysis)
                Dim reason = If(help.IsFallback, "Unknown analysis.", "Documented method has no executable Common registration.")
                Return New FeatureAvailabilityInfo(help, False, CapabilityPreflightResult.Rejected(code, reason))
            End If
            Dim target As New SpreadsheetOutputTarget With {.Destination = destination}
            Return New FeatureAvailabilityInfo(help, True, HostCapabilityPreflight.CheckRunAndWrite(id, host, target))
        End Function
    End Class
End Namespace
