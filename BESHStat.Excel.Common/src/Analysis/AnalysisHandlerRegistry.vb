Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>
    ''' Immutable compile-time registry: the descriptor catalog describes available analyses,
    ''' while this registry alone supplies executable typed handlers.
    ''' Documented but unregistered methods are never dispatched.
    ''' </summary>
    Public NotInheritable Class AnalysisHandlerRegistry

        Private Shared ReadOnly _all As IReadOnlyList(Of IAnalysisHandler) = BuildHandlers()
        Private Shared ReadOnly _byId As Dictionary(Of String, IReadOnlyList(Of IAnalysisHandler)) = BuildIndex(_all)

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property All As IReadOnlyList(Of IAnalysisHandler)
            Get
                Return _all
            End Get
        End Property

        ''' <summary>
        ''' Run using a stable analysis ID. Input and option types are checked before execution.
        ''' Nothing options preserve the historical defaults in each application service.
        ''' </summary>
        Public Shared Function Run(analysisId As String,
                                   input As SpreadsheetAnalysisInput,
                                   options As SpreadsheetAnalysisOptions) As SpreadsheetAnalysisResult
            If String.IsNullOrWhiteSpace(analysisId) Then Throw New ArgumentException("Analysis ID is required.", NameOf(analysisId))
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))

            Dim descriptor As IAnalysisDescriptor = Nothing
            Dim handlers As IReadOnlyList(Of IAnalysisHandler) = Nothing
            If Not AnalysisCatalog.TryGetById(analysisId, descriptor) OrElse
               descriptor.Availability <> AnalysisAvailability.ImplementedInCommon OrElse
               Not _byId.TryGetValue(analysisId, handlers) Then
                Throw New NotSupportedException("The shared Excel application layer does not support analysis ID '" & analysisId & "'.")
            End If

            If options IsNot Nothing AndAlso Not descriptor.OptionsType.IsInstanceOfType(options) Then
                Throw New ArgumentException(descriptor.DisplayName & " requires " & descriptor.OptionsType.Name & ".", NameOf(options))
            End If

            For Each handler As IAnalysisHandler In handlers
                If handler.InputType.IsInstanceOfType(input) Then
                    Dim result As SpreadsheetAnalysisResult = handler.Execute(input, options)
                    If descriptor.LegacyKind.HasValue AndAlso result.AnalysisKind <> descriptor.LegacyKind.Value Then
                        Throw New InvalidOperationException("Handler returned an incompatible analysis kind for " & analysisId & ".")
                    End If
                    Return result
                End If
            Next

            Throw New ArgumentException(descriptor.DisplayName & " does not accept input type " & input.GetType().Name & ".", NameOf(input))
        End Function

        ''' <summary>Verify that every implemented descriptor has exactly one handler for every declared input type.</summary>
        ''' <remarks>Also usable with independent test registrations; does not mutate the production catalog.</remarks>
        Public Shared Sub ValidateRegistrations(descriptors As IEnumerable(Of IAnalysisDescriptor),
                                                handlers As IEnumerable(Of IAnalysisHandler))
            If descriptors Is Nothing Then Throw New ArgumentNullException(NameOf(descriptors))
            If handlers Is Nothing Then Throw New ArgumentNullException(NameOf(handlers))
            AnalysisCatalog.ValidateDescriptors(descriptors)

            Dim byId As New Dictionary(Of String, IAnalysisDescriptor)(StringComparer.Ordinal)
            Dim seen As New Dictionary(Of String, HashSet(Of Type))(StringComparer.Ordinal)
            For Each descriptor As IAnalysisDescriptor In descriptors
                byId.Add(descriptor.Id, descriptor)
                seen.Add(descriptor.Id, New HashSet(Of Type)())
            Next

            For Each handler As IAnalysisHandler In handlers
                If handler Is Nothing OrElse String.IsNullOrWhiteSpace(handler.AnalysisId) Then
                    Throw New ArgumentException("A handler is missing its analysis ID.", NameOf(handlers))
                End If
                Dim descriptor As IAnalysisDescriptor = Nothing
                If Not byId.TryGetValue(handler.AnalysisId, descriptor) Then
                    Throw New ArgumentException("Handler has no matching descriptor: " & handler.AnalysisId, NameOf(handlers))
                End If
                If descriptor.Availability <> AnalysisAvailability.ImplementedInCommon Then
                    Throw New ArgumentException("Unimplemented analysis cannot register a handler: " & handler.AnalysisId, NameOf(handlers))
                End If
                If handler.OptionsType IsNot descriptor.OptionsType Then
                    Throw New ArgumentException("Handler options differ from catalog: " & handler.AnalysisId, NameOf(handlers))
                End If
                If handler.InputType Is Nothing OrElse Not ContainsInputType(descriptor.InputTypes, handler.InputType) Then
                    Throw New ArgumentException("Handler input differs from catalog: " & handler.AnalysisId, NameOf(handlers))
                End If
                If Not seen(handler.AnalysisId).Add(handler.InputType) Then
                    Throw New ArgumentException("Duplicate handler for " & handler.AnalysisId & "/" & handler.InputType.Name, NameOf(handlers))
                End If
            Next

            For Each descriptor As IAnalysisDescriptor In byId.Values
                If descriptor.Availability = AnalysisAvailability.ImplementedInCommon Then
                    For Each inputType As Type In descriptor.InputTypes
                        If Not seen(descriptor.Id).Contains(inputType) Then
                            Throw New ArgumentException("Missing handler for " & descriptor.Id & "/" & inputType.Name, NameOf(handlers))
                        End If
                    Next
                End If
            Next
        End Sub

        Private Shared Function ContainsInputType(types As IReadOnlyList(Of Type), candidate As Type) As Boolean
            For Each item As Type In types
                If item Is candidate Then Return True
            Next
            Return False
        End Function

        Private Shared Function BuildHandlers() As IReadOnlyList(Of IAnalysisHandler)
            Dim handlers As IAnalysisHandler() = {
                New TypedAnalysisHandler(Of SpreadsheetRangeData, OneWayAnovaAnalysisOptions, OneWayAnovaApplicationResult)(
                    AnalysisIds.OneWayAnova,
                    Function(input, options) OneWayAnovaApplicationService.Run(input, options),
                    AddressOf WrapAnova),
                New TypedAnalysisHandler(Of GroupedNumericData, OneWayAnovaAnalysisOptions, OneWayAnovaApplicationResult)(
                    AnalysisIds.OneWayAnova,
                    Function(input, options) OneWayAnovaApplicationService.Run(input, options),
                    AddressOf WrapAnova),
                New TypedAnalysisHandler(Of SpreadsheetRangeData, KruskalWallisAnalysisOptions, KruskalWallisApplicationResult)(
                    AnalysisIds.KruskalWallis,
                    Function(input, options) KruskalWallisApplicationService.Run(input, options),
                    AddressOf WrapKruskalWallis),
                New TypedAnalysisHandler(Of GroupedNumericData, KruskalWallisAnalysisOptions, KruskalWallisApplicationResult)(
                    AnalysisIds.KruskalWallis,
                    Function(input, options) KruskalWallisApplicationService.Run(input, options),
                    AddressOf WrapKruskalWallis)
            }
            ValidateRegistrations(AnalysisCatalog.All, handlers)
            Return Array.AsReadOnly(handlers)
        End Function

        Private Shared Function BuildIndex(handlers As IEnumerable(Of IAnalysisHandler)) As Dictionary(Of String, IReadOnlyList(Of IAnalysisHandler))
            Dim mutable As New Dictionary(Of String, List(Of IAnalysisHandler))(StringComparer.Ordinal)
            For Each handler As IAnalysisHandler In handlers
                Dim list As List(Of IAnalysisHandler) = Nothing
                If Not mutable.TryGetValue(handler.AnalysisId, list) Then
                    list = New List(Of IAnalysisHandler)()
                    mutable.Add(handler.AnalysisId, list)
                End If
                list.Add(handler)
            Next
            Dim result As New Dictionary(Of String, IReadOnlyList(Of IAnalysisHandler))(StringComparer.Ordinal)
            For Each pair As KeyValuePair(Of String, List(Of IAnalysisHandler)) In mutable
                result.Add(pair.Key, pair.Value.AsReadOnly())
            Next
            Return result
        End Function

        Private Shared Function WrapAnova(detail As OneWayAnovaApplicationResult) As SpreadsheetAnalysisResult
            Dim metrics As New Dictionary(Of String, Double)(StringComparer.Ordinal) From {
                {AnalysisMetricNames.FStatistic, detail.FStatistic},
                {AnalysisMetricNames.PValue, detail.PValue}
            }
            If detail.HasWelch Then
                metrics(AnalysisMetricNames.WelchFStatistic) = detail.WelchFStatistic
                metrics(AnalysisMetricNames.WelchPValue) = detail.WelchPValue
                metrics(AnalysisMetricNames.WelchDenominatorDf) = detail.WelchDenominatorDf
            End If
            Return New SpreadsheetAnalysisResult(SpreadsheetAnalysisKind.OneWayAnova, detail.Tables, metrics, detail)
        End Function

        Private Shared Function WrapKruskalWallis(detail As KruskalWallisApplicationResult) As SpreadsheetAnalysisResult
            Dim metrics As New Dictionary(Of String, Double)(StringComparer.Ordinal) From {
                {AnalysisMetricNames.HStatistic, detail.HStatistic},
                {AnalysisMetricNames.TieCorrectedHStatistic, detail.TieCorrectedHStatistic},
                {AnalysisMetricNames.PValue, detail.PValue},
                {AnalysisMetricNames.TieCorrectedPValue, detail.TieCorrectedPValue}
            }
            Return New SpreadsheetAnalysisResult(SpreadsheetAnalysisKind.KruskalWallis, detail.Tables, metrics, detail)
        End Function
    End Class

End Namespace
