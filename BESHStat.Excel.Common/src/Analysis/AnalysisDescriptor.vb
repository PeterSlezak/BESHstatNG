Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>Current availability of an analysis at the shared application boundary.</summary>
    Public Enum AnalysisAvailability
        Planned = 0
        ImplementedInCommon = 1
        Unavailable = 2
    End Enum

    ''' <summary>Advisory execution cost; not a measured time or cancellation guarantee.</summary>
    Public Enum AnalysisExecutionCost
        Low = 0
        Moderate = 1
        High = 2
    End Enum

    <Flags>
    Public Enum AnalysisOutputKinds
        None = 0
        Tables = 1
        Metrics = 2
        TypedDetail = 4
        ChartDocument = 8
    End Enum

    ''' <summary>Read-only discovery metadata; dispatch remains on the legacy enum until 00.03.</summary>
    Public Interface IAnalysisDescriptor
        ReadOnly Property Id As String
        ReadOnly Property DisplayName As String
        ReadOnly Property Category As String
        ReadOnly Property Description As String
        ReadOnly Property HelpUrl As String
        ReadOnly Property ContractVersion As Integer
        ReadOnly Property InputSchemaId As String
        ReadOnly Property OptionsSchemaId As String
        ReadOnly Property InputTypes As IReadOnlyList(Of Type)
        ReadOnly Property OptionsType As Type
        ReadOnly Property OutputKinds As AnalysisOutputKinds
        ReadOnly Property Availability As AnalysisAvailability
        ReadOnly Property EstimatedCost As AnalysisExecutionCost
        ReadOnly Property RequiredCapabilities As IReadOnlyList(Of String)
        ReadOnly Property LegacyKind As Nullable(Of SpreadsheetAnalysisKind)
        Function CreateDefaultOptions() As SpreadsheetAnalysisOptions
    End Interface

    ''' <summary>Immutable catalog entry. Each defaults request gets a new options instance.</summary>
    Public NotInheritable Class AnalysisDescriptor
        Implements IAnalysisDescriptor

        Private ReadOnly _inputTypes As IReadOnlyList(Of Type)
        Private ReadOnly _requiredCapabilities As IReadOnlyList(Of String)
        Private ReadOnly _createOptions As Func(Of SpreadsheetAnalysisOptions)

        Public Sub New(id As String,
                       displayName As String,
                       category As String,
                       description As String,
                       helpUrl As String,
                       contractVersion As Integer,
                       inputSchemaId As String,
                       optionsSchemaId As String,
                       inputTypes As IEnumerable(Of Type),
                       optionsType As Type,
                       createOptions As Func(Of SpreadsheetAnalysisOptions),
                       outputKinds As AnalysisOutputKinds,
                       availability As AnalysisAvailability,
                       estimatedCost As AnalysisExecutionCost,
                       requiredCapabilities As IEnumerable(Of String),
                       legacyKind As Nullable(Of SpreadsheetAnalysisKind))
            If Not IsStableId(id) Then Throw New ArgumentException("Analysis IDs must be lowercase ASCII segments separated by '.' or '-'.", NameOf(id))
            If String.IsNullOrWhiteSpace(displayName) Then Throw New ArgumentException("Display name is required.", NameOf(displayName))
            If String.IsNullOrWhiteSpace(category) Then Throw New ArgumentException("Category is required.", NameOf(category))
            If String.IsNullOrWhiteSpace(description) Then Throw New ArgumentException("Description is required.", NameOf(description))
            If String.IsNullOrWhiteSpace(helpUrl) OrElse Not Uri.IsWellFormedUriString(helpUrl, UriKind.Absolute) Then
                Throw New ArgumentException("An absolute help URL is required.", NameOf(helpUrl))
            End If
            If contractVersion <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(contractVersion))
            If Not IsStableId(inputSchemaId) Then Throw New ArgumentException("Input schema ID is invalid.", NameOf(inputSchemaId))
            If Not IsStableId(optionsSchemaId) Then Throw New ArgumentException("Options schema ID is invalid.", NameOf(optionsSchemaId))
            If optionsType Is Nothing OrElse Not GetType(SpreadsheetAnalysisOptions).IsAssignableFrom(optionsType) Then
                Throw New ArgumentException("OptionsType must derive from SpreadsheetAnalysisOptions.", NameOf(optionsType))
            End If
            If createOptions Is Nothing Then Throw New ArgumentNullException(NameOf(createOptions))
            If inputTypes Is Nothing Then Throw New ArgumentNullException(NameOf(inputTypes))
            If requiredCapabilities Is Nothing Then Throw New ArgumentNullException(NameOf(requiredCapabilities))

            Dim typeList As New List(Of Type)()
            For Each inputType As Type In inputTypes
                If inputType Is Nothing OrElse Not GetType(SpreadsheetAnalysisInput).IsAssignableFrom(inputType) OrElse typeList.Contains(inputType) Then
                    Throw New ArgumentException("Input types must be unique SpreadsheetAnalysisInput types.", NameOf(inputTypes))
                End If
                typeList.Add(inputType)
            Next
            If typeList.Count = 0 Then Throw New ArgumentException("At least one input type is required.", NameOf(inputTypes))
            _inputTypes = typeList.AsReadOnly()

            Dim caps As New List(Of String)()
            For Each capability As String In requiredCapabilities
                If String.IsNullOrWhiteSpace(capability) OrElse caps.Contains(capability) Then
                    Throw New ArgumentException("Capability IDs must be nonblank and unique.", NameOf(requiredCapabilities))
                End If
                caps.Add(capability)
            Next
            _requiredCapabilities = caps.AsReadOnly()

            Me.Id = id
            Me.DisplayName = displayName
            Me.Category = category
            Me.Description = description
            Me.HelpUrl = helpUrl
            Me.ContractVersion = contractVersion
            Me.InputSchemaId = inputSchemaId
            Me.OptionsSchemaId = optionsSchemaId
            Me.OptionsType = optionsType
            Me.OutputKinds = outputKinds
            Me.Availability = availability
            Me.EstimatedCost = estimatedCost
            Me.LegacyKind = legacyKind
            _createOptions = createOptions
        End Sub

        Public ReadOnly Property Id As String Implements IAnalysisDescriptor.Id
        Public ReadOnly Property DisplayName As String Implements IAnalysisDescriptor.DisplayName
        Public ReadOnly Property Category As String Implements IAnalysisDescriptor.Category
        Public ReadOnly Property Description As String Implements IAnalysisDescriptor.Description
        Public ReadOnly Property HelpUrl As String Implements IAnalysisDescriptor.HelpUrl
        Public ReadOnly Property ContractVersion As Integer Implements IAnalysisDescriptor.ContractVersion
        Public ReadOnly Property InputSchemaId As String Implements IAnalysisDescriptor.InputSchemaId
        Public ReadOnly Property OptionsSchemaId As String Implements IAnalysisDescriptor.OptionsSchemaId
        Public ReadOnly Property OptionsType As Type Implements IAnalysisDescriptor.OptionsType
        Public ReadOnly Property OutputKinds As AnalysisOutputKinds Implements IAnalysisDescriptor.OutputKinds
        Public ReadOnly Property Availability As AnalysisAvailability Implements IAnalysisDescriptor.Availability
        Public ReadOnly Property EstimatedCost As AnalysisExecutionCost Implements IAnalysisDescriptor.EstimatedCost
        Public ReadOnly Property LegacyKind As Nullable(Of SpreadsheetAnalysisKind) Implements IAnalysisDescriptor.LegacyKind

        Public ReadOnly Property InputTypes As IReadOnlyList(Of Type) Implements IAnalysisDescriptor.InputTypes
            Get
                Return _inputTypes
            End Get
        End Property

        Public ReadOnly Property RequiredCapabilities As IReadOnlyList(Of String) Implements IAnalysisDescriptor.RequiredCapabilities
            Get
                Return _requiredCapabilities
            End Get
        End Property

        Public Function CreateDefaultOptions() As SpreadsheetAnalysisOptions Implements IAnalysisDescriptor.CreateDefaultOptions
            Dim options As SpreadsheetAnalysisOptions = _createOptions()
            If options Is Nothing OrElse Not OptionsType.IsInstanceOfType(options) Then
                Throw New InvalidOperationException("The options factory returned an incompatible type for " & Id & ".")
            End If
            Return options
        End Function

        Private Shared Function IsStableId(value As String) As Boolean
            If String.IsNullOrWhiteSpace(value) Then Return False
            Dim precedingSeparator As Boolean = True
            For Each ch As Char In value
                If (ch >= "a"c AndAlso ch <= "z"c) OrElse (ch >= "0"c AndAlso ch <= "9"c) Then
                    precedingSeparator = False
                ElseIf (ch = "."c OrElse ch = "-"c) AndAlso Not precedingSeparator Then
                    precedingSeparator = True
                Else
                    Return False
                End If
            Next
            Return Not precedingSeparator
        End Function
    End Class

End Namespace
