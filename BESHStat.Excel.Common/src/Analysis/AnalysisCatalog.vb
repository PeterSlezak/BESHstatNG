Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>Explicit compile-time feature catalog; no reflection or dispatch is performed here.</summary>
    ''' <remarks>Batch 00.03 will add typed handler registration. The existing enum dispatcher
    ''' and current request/result types remain untouched in 00.02.</remarks>
    Public NotInheritable Class AnalysisCatalog

        Private Shared ReadOnly _all As IReadOnlyList(Of IAnalysisDescriptor) = BuildDescriptors()
        Private Shared ReadOnly _byId As Dictionary(Of String, IAnalysisDescriptor) = BuildIds(_all)
        Private Shared ReadOnly _byLegacy As Dictionary(Of SpreadsheetAnalysisKind, IAnalysisDescriptor) = BuildLegacyIds(_all)

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property All As IReadOnlyList(Of IAnalysisDescriptor)
            Get
                Return _all
            End Get
        End Property

        Public Shared Function TryGetById(id As String, ByRef descriptor As IAnalysisDescriptor) As Boolean
            descriptor = Nothing
            If String.IsNullOrEmpty(id) Then Return False
            Return _byId.TryGetValue(id, descriptor)
        End Function

        Public Shared Function GetById(id As String) As IAnalysisDescriptor
            Dim descriptor As IAnalysisDescriptor = Nothing
            If Not TryGetById(id, descriptor) Then Throw New KeyNotFoundException("Unknown analysis ID: " & If(id, "(null)"))
            Return descriptor
        End Function

        Public Shared Function TryGetByLegacyKind(kind As SpreadsheetAnalysisKind, ByRef descriptor As IAnalysisDescriptor) As Boolean
            Return _byLegacy.TryGetValue(kind, descriptor)
        End Function

        Public Shared Function GetByLegacyKind(kind As SpreadsheetAnalysisKind) As IAnalysisDescriptor
            Dim descriptor As IAnalysisDescriptor = Nothing
            If Not TryGetByLegacyKind(kind, descriptor) Then
                Throw New KeyNotFoundException("Unknown legacy analysis kind: " & CInt(kind).ToString(Globalization.CultureInfo.InvariantCulture))
            End If
            Return descriptor
        End Function

        Public Shared Function TryGetLegacyKind(id As String, ByRef kind As SpreadsheetAnalysisKind) As Boolean
            kind = CType(0, SpreadsheetAnalysisKind)
            Dim descriptor As IAnalysisDescriptor = Nothing
            If Not TryGetById(id, descriptor) OrElse Not descriptor.LegacyKind.HasValue Then Return False
            kind = descriptor.LegacyKind.Value
            Return True
        End Function

        ''' <summary>Validate uniqueness for future catalog additions and independent tests.</summary>
        Public Shared Sub ValidateDescriptors(descriptors As IEnumerable(Of IAnalysisDescriptor))
            If descriptors Is Nothing Then Throw New ArgumentNullException(NameOf(descriptors))
            Dim ids As New HashSet(Of String)(StringComparer.Ordinal)
            Dim kinds As New HashSet(Of SpreadsheetAnalysisKind)()
            For Each descriptor As IAnalysisDescriptor In descriptors
                If descriptor Is Nothing OrElse String.IsNullOrWhiteSpace(descriptor.Id) Then
                    Throw New ArgumentException("A descriptor has no ID.", NameOf(descriptors))
                End If
                If Not ids.Add(descriptor.Id) Then
                    Throw New ArgumentException("Duplicate analysis ID: " & descriptor.Id, NameOf(descriptors))
                End If
                If descriptor.LegacyKind.HasValue AndAlso Not kinds.Add(descriptor.LegacyKind.Value) Then
                    Throw New ArgumentException("Duplicate legacy enum mapping: " & descriptor.LegacyKind.Value.ToString(), NameOf(descriptors))
                End If
            Next
        End Sub

        Private Shared Function BuildDescriptors() As IReadOnlyList(Of IAnalysisDescriptor)
            Dim anovaMetadata As AnalysisMethodMetadata = GeneratedMethodMetadata.GetByAnalysisId(AnalysisIds.OneWayAnova)
            Dim kruskalMetadata As AnalysisMethodMetadata = GeneratedMethodMetadata.GetByAnalysisId(AnalysisIds.KruskalWallis)
            Dim entries As IAnalysisDescriptor() = {
                New AnalysisDescriptor(
                    AnalysisIds.OneWayAnova,
                    anovaMetadata.DisplayName,
                    anovaMetadata.Category,
                    anovaMetadata.Description,
                    anovaMetadata.HelpUrl,
                    1,
                    "grouped-numeric.wide.v1",
                    "anova.oneway.options.v1",
                    New Type() {GetType(SpreadsheetRangeData), GetType(GroupedNumericData)},
                    GetType(OneWayAnovaAnalysisOptions),
                    Function() CType(New OneWayAnovaAnalysisOptions(), SpreadsheetAnalysisOptions),
                    AnalysisOutputKinds.Tables Or AnalysisOutputKinds.Metrics Or AnalysisOutputKinds.TypedDetail,
                    AnalysisAvailability.ImplementedInCommon,
                    AnalysisExecutionCost.Low,
                    Array.Empty(Of String)(),
                    SpreadsheetAnalysisKind.OneWayAnova),
                New AnalysisDescriptor(
                    AnalysisIds.KruskalWallis,
                    kruskalMetadata.DisplayName,
                    kruskalMetadata.Category,
                    kruskalMetadata.Description,
                    kruskalMetadata.HelpUrl,
                    1,
                    "grouped-numeric.wide.v1",
                    "nonparametric.kruskal-wallis.options.v1",
                    New Type() {GetType(SpreadsheetRangeData), GetType(GroupedNumericData)},
                    GetType(KruskalWallisAnalysisOptions),
                    Function() CType(New KruskalWallisAnalysisOptions(), SpreadsheetAnalysisOptions),
                    AnalysisOutputKinds.Tables Or AnalysisOutputKinds.Metrics Or AnalysisOutputKinds.TypedDetail,
                    AnalysisAvailability.ImplementedInCommon,
                    AnalysisExecutionCost.Low,
                    Array.Empty(Of String)(),
                    SpreadsheetAnalysisKind.KruskalWallis)
            }
            ValidateDescriptors(entries)
            Return Array.AsReadOnly(entries)
        End Function

        Private Shared Function BuildIds(entries As IEnumerable(Of IAnalysisDescriptor)) As Dictionary(Of String, IAnalysisDescriptor)
            Dim byId As New Dictionary(Of String, IAnalysisDescriptor)(StringComparer.Ordinal)
            For Each descriptor As IAnalysisDescriptor In entries
                byId.Add(descriptor.Id, descriptor)
            Next
            Return byId
        End Function

        Private Shared Function BuildLegacyIds(entries As IEnumerable(Of IAnalysisDescriptor)) As Dictionary(Of SpreadsheetAnalysisKind, IAnalysisDescriptor)
            Dim byKind As New Dictionary(Of SpreadsheetAnalysisKind, IAnalysisDescriptor)()
            For Each descriptor As IAnalysisDescriptor In entries
                If descriptor.LegacyKind.HasValue Then byKind.Add(descriptor.LegacyKind.Value, descriptor)
            Next
            Return byKind
        End Function
    End Class

End Namespace
