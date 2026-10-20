# Gate B — Common v1 source contract inventory

**OWNER DECISION: PENDING.** Inspected checkout: `codex/gate-b`, HEAD `6b0b931efe2f1a6851bd9efb84289e348e5249d8` (20 October 2026). This is a review proposal, not sign-off. Read with [freeze proposal](gate-b-common-v1-contract-freeze.md).

## Reading this ledger

Every public top-level type in `BESHStat.Excel.Common/src` is listed below, together with the Core output/settings boundary and public Windows writer. Source links identify declarations; declaration excerpts preserve property names, CLR/VB types, defaults, enum numbers, constructor/factory signatures and public methods. They are source observations, not a newly generated production schema. Interface member declarations are included. Inherited members follow the linked base type. Constructors may accept broader states than parsers; structural guards are quoted separately. The family rules immediately above each type apply to it, with exceptions called out in the proposal.

Classification: **F** = Common v1 frozen candidate (API/semantics; serialization only where explicitly stated); **H** = host-specific adapter/transport, separately versioned; **I** = in-memory-only implementation type, no wire compatibility promise; **L** = legacy/transitional compatibility API; **U** = future/unresolved. F is conditional on owner acceptance. For Core types, F freezes Common-facing use, not ownership of the whole Core assembly. No v1 label implies all types are immutable or JSON-serializable.

VB reference types do not carry nullable-reference annotations: a String/List/class may be Nothing unless its constructor/factory validates or normalizes it. `?` and `Nullable(Of T)` mean nullable value type; unselected OutputCell union fields are Nothing. Mutable transport setters permit null after construction; downstream validators remain required. ReadOnly arrays/references alone do not guarantee deep immutability. No implicit constructor adds validation. All enums listed without an explicit `As` have Int32 backing. Source declarations, not JSON enum names, own those numeric values.

Version column text reports only actual declared versions. OutputDocument/Layout/Composite/Intent/Inspection/Consent have no SchemaVersion. Descriptor ContractVersion is analysis metadata; snapshot SchemaVersion is producer-assigned. Unknown optional-field support is reader-specific: default System.Text.Json diagnostic snapshot reader ignores extras; strict Office table/composite readers reject them. No general Common deserializer exists.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisCatalogTests, AnalysisHandlerRegistryTests, GeneratedMethodMetadataTests, HostCapabilitiesTests.

### `AnalysisCatalog` — I

[Declaration L12](../../BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb#L12). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisCatalog
Private Sub New()
Public Shared ReadOnly Property All As IReadOnlyList(Of IAnalysisDescriptor)
Public Shared Function TryGetById(id As String, ByRef descriptor As IAnalysisDescriptor) As Boolean
Public Shared Function GetById(id As String) As IAnalysisDescriptor
Public Shared Function TryGetByLegacyKind(kind As SpreadsheetAnalysisKind, ByRef descriptor As IAnalysisDescriptor) As Boolean
Public Shared Function GetByLegacyKind(kind As SpreadsheetAnalysisKind) As IAnalysisDescriptor
Public Shared Function TryGetLegacyKind(id As String, ByRef kind As SpreadsheetAnalysisKind) As Boolean
Public Shared Sub ValidateDescriptors(descriptors As IEnumerable(Of IAnalysisDescriptor))
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`, `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `GeneratedMethodMetadataTests`, `HostCapabilitiesTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb`

Owner/consumers: AnalysisCatalog and typed registry; UI discovery.

Stable lowercase segmented IDs; required metadata and positive ContractVersion, typed options/input constraints, unique capabilities/input types. Lists copied read-only; defaults factory returns compatible options. Type objects/delegates have no JSON portability promise. Display/help authority remains methods.yml.

Evidence/fixtures: AnalysisCatalogTests; AnalysisHandlerRegistryTests.

### `AnalysisAvailability` — F

[Declaration L10](../../BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum AnalysisAvailability
Planned = 0
ImplementedInCommon = 1
Unavailable = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Host/HostCapabilities.vb`.

Tests: `AnalysisCatalogTests`.

### `AnalysisExecutionCost` — F

[Declaration L17](../../BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb#L17). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum AnalysisExecutionCost
Low = 0
Moderate = 1
High = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`.

Tests: `AnalysisCatalogTests`.

### `AnalysisOutputKinds` — F

[Declaration L24](../../BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb#L24). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum AnalysisOutputKinds
None = 0
Tables = 1
Metrics = 2
TypedDetail = 4
ChartDocument = 8
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`.

Tests: `AnalysisCatalogTests`.

### `IAnalysisDescriptor` — F

[Declaration L33](../../BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb#L33). **Version/exposure:** ContractVersion required positive; current descriptors 1. Type/delegate members are in-process.

```vb
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
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `GeneratedMethodMetadataTests`, `HostCapabilitiesTests`.

### `AnalysisDescriptor` — F

[Declaration L53](../../BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb#L53). **Version/exposure:** ContractVersion required positive; current descriptors 1. Type/delegate members are in-process.

```vb
Public NotInheritable Class AnalysisDescriptor
Public Sub New(id As String, displayName As String, category As String, description As String, helpUrl As String, contractVersion As Integer, inputSchemaId As String, optionsSchemaId As String, inputTypes As IEnumerable(Of Type), optionsType As Type, createOptions As Func(Of SpreadsheetAnalysisOptions), outputKinds As AnalysisOutputKinds, availability As AnalysisAvailability, estimatedCost As AnalysisExecutionCost, requiredCapabilities As IEnumerable(Of String), legacyKind As Nullable(Of SpreadsheetAnalysisKind))
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
Public ReadOnly Property RequiredCapabilities As IReadOnlyList(Of String) Implements IAnalysisDescriptor.RequiredCapabilities
Public Function CreateDefaultOptions() As SpreadsheetAnalysisOptions Implements IAnalysisDescriptor.CreateDefaultOptions
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If Not IsStableId(id) Then Throw New ArgumentException("Analysis IDs must be lowercase ASCII segments separated by '.' or '-'.", NameOf(id))`
- `If String.IsNullOrWhiteSpace(displayName) Then Throw New ArgumentException("Display name is required.", NameOf(displayName))`
- `If String.IsNullOrWhiteSpace(category) Then Throw New ArgumentException("Category is required.", NameOf(category))`
- `If String.IsNullOrWhiteSpace(description) Then Throw New ArgumentException("Description is required.", NameOf(description))`
- `If String.IsNullOrWhiteSpace(helpUrl) OrElse Not Uri.IsWellFormedUriString(helpUrl, UriKind.Absolute) Then Throw New ArgumentException("An absolute help URL is required.", NameOf(helpUrl))`
- `If contractVersion <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(contractVersion))`
- `If Not IsStableId(inputSchemaId) Then Throw New ArgumentException("Input schema ID is invalid.", NameOf(inputSchemaId))`
- `If Not IsStableId(optionsSchemaId) Then Throw New ArgumentException("Options schema ID is invalid.", NameOf(optionsSchemaId))`
- `If optionsType Is Nothing OrElse Not GetType(SpreadsheetAnalysisOptions).IsAssignableFrom(optionsType) Then Throw New ArgumentException("OptionsType must derive from SpreadsheetAnalysisOptions.", NameOf(optionsType))`
- `If createOptions Is Nothing Then Throw New ArgumentNullException(NameOf(createOptions))`
- `If inputTypes Is Nothing Then Throw New ArgumentNullException(NameOf(inputTypes))`
- `If requiredCapabilities Is Nothing Then Throw New ArgumentNullException(NameOf(requiredCapabilities))`
- `If inputType Is Nothing OrElse Not GetType(SpreadsheetAnalysisInput).IsAssignableFrom(inputType) OrElse typeList.Contains(inputType) Then Throw New ArgumentException("Input types must be unique SpreadsheetAnalysisInput types.", NameOf(inputTypes))`
- `If typeList.Count = 0 Then Throw New ArgumentException("At least one input type is required.", NameOf(inputTypes))`
- `If String.IsNullOrWhiteSpace(capability) OrElse caps.Contains(capability) Then Throw New ArgumentException("Capability IDs must be nonblank and unique.", NameOf(requiredCapabilities))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`.

Tests: `AnalysisCatalogTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisHandlerRegistryTests.

### `AnalysisHandlerRegistry` — I

[Declaration L15](../../BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb#L15). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisHandlerRegistry
Private Sub New()
Public Shared ReadOnly Property All As IReadOnlyList(Of IAnalysisHandler)
Public Shared Function Run(analysisId As String, input As SpreadsheetAnalysisInput, options As SpreadsheetAnalysisOptions) As SpreadsheetAnalysisResult
Public Shared Sub ValidateRegistrations(descriptors As IEnumerable(Of IAnalysisDescriptor), handlers As IEnumerable(Of IAnalysisHandler))
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`.

Tests: `AnalysisHandlerRegistryTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/AnalysisIds.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisCatalogTests, AnalysisHandlerRegistryTests, AnalysisResultDocumentTests, GateBOutputContractCompatibilityTests.

### `AnalysisIds` — F

[Declaration L8](../../BESHStat.Excel.Common/src/Analysis/AnalysisIds.vb#L8). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisIds
Private Sub New()
Public Const OneWayAnova As String = "anova.oneway"
Public Const KruskalWallis As String = "nonparametric.kruskal-wallis"
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/GeneratedMethodMetadata.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisHandlerRegistryTests, GeneratedMethodMetadataTests.

### `AnalysisMethodMetadata` — I

[Declaration L14](../../BESHStat.Excel.Common/src/Analysis/GeneratedMethodMetadata.vb#L14). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisMethodMetadata
Public Sub New(key As String, analysisId As String, displayName As String, category As String, description As String, helpUrl As String)
Public ReadOnly Property Key As String
Public ReadOnly Property AnalysisId As String
Public ReadOnly Property DisplayName As String
Public ReadOnly Property Category As String
Public ReadOnly Property Description As String
Public ReadOnly Property HelpUrl As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`.

Tests: `AnalysisHandlerRegistryTests`, `GeneratedMethodMetadataTests`.

### `GeneratedMethodMetadata` — I

[Declaration L31](../../BESHStat.Excel.Common/src/Analysis/GeneratedMethodMetadata.vb#L31). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class GeneratedMethodMetadata
Private Sub New()
Public Shared ReadOnly Property All As IReadOnlyList(Of AnalysisMethodMetadata)
Public Shared Function GetByKey(key As String) As AnalysisMethodMetadata
Public Shared Function TryGetByAnalysisId(id As String, ByRef metadata As AnalysisMethodMetadata) As Boolean
Public Shared Function GetByAnalysisId(id As String) As AnalysisMethodMetadata
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`.

Tests: `AnalysisHandlerRegistryTests`, `GeneratedMethodMetadataTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/IAnalysisHandler.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisHandlerRegistryTests.

### `IAnalysisHandler` — F

[Declaration L12](../../BESHStat.Excel.Common/src/Analysis/IAnalysisHandler.vb#L12). **Version/exposure:** No declared version; no wire promise.

```vb
Public Interface IAnalysisHandler
ReadOnly Property AnalysisId As String
ReadOnly Property InputType As Type
ReadOnly Property OptionsType As Type
Function Execute(input As SpreadsheetAnalysisInput, options As SpreadsheetAnalysisOptions) As SpreadsheetAnalysisResult
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`.

Tests: `AnalysisHandlerRegistryTests`.

### `IAnalysisHandler` — F

[Declaration L24](../../BESHStat.Excel.Common/src/Analysis/IAnalysisHandler.vb#L24). **Version/exposure:** No declared version; no wire promise.

```vb
Public Interface IAnalysisHandler(Of TInput As SpreadsheetAnalysisInput, TOptions As SpreadsheetAnalysisOptions, TDetail As Class)
Inherits IAnalysisHandler
Function ExecuteTyped(input As TInput, options As TOptions) As TDetail
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`.

Tests: `AnalysisHandlerRegistryTests`.

### `TypedAnalysisHandler` — I

[Declaration L36](../../BESHStat.Excel.Common/src/Analysis/IAnalysisHandler.vb#L36). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class TypedAnalysisHandler(Of TInput As SpreadsheetAnalysisInput, TOptions As SpreadsheetAnalysisOptions, TDetail As Class)
Public Sub New(analysisId As String, execute As Func(Of TInput, TOptions, TDetail), wrap As Func(Of TDetail, SpreadsheetAnalysisResult))
Public ReadOnly Property AnalysisId As String Implements IAnalysisHandler.AnalysisId
Public ReadOnly Property InputType As Type Implements IAnalysisHandler.InputType
Public ReadOnly Property OptionsType As Type Implements IAnalysisHandler.OptionsType
Public Function ExecuteTyped(input As TInput, options As TOptions) As TDetail Implements IAnalysisHandler(Of TInput, TOptions, TDetail).ExecuteTyped
Public Function Execute(input As SpreadsheetAnalysisInput, options As SpreadsheetAnalysisOptions) As SpreadsheetAnalysisResult Implements IAnalysisHandler.Execute
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(analysisId) Then Throw New ArgumentException("Analysis ID is required.", NameOf(analysisId))`
- `If execute Is Nothing Then Throw New ArgumentNullException(NameOf(execute))`
- `If wrap Is Nothing Then Throw New ArgumentNullException(NameOf(wrap))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`.

Tests: `AnalysisHandlerRegistryTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisHandlerRegistryTests, AnalysisResultDocumentTests, SharedExcelLayerTests, Stage304ContractSnapshotTests.

### `KruskalWallisApplicationResult` — I

[Declaration L11](../../BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class KruskalWallisApplicationResult
Public Sub New(tables As List(Of Global.BESHStatNG.ResultTableOutputModel), hStatistic As Double, tieCorrectedHStatistic As Double, pValue As Double, tieCorrectedPValue As Double, groupNames As String(), groupCounts As Integer())
Public ReadOnly Property Tables As List(Of Global.BESHStatNG.ResultTableOutputModel)
Public ReadOnly Property Table As Global.BESHStatNG.ResultTableOutputModel
Public ReadOnly Property HStatistic As Double
Public ReadOnly Property TieCorrectedHStatistic As Double
Public ReadOnly Property PValue As Double
Public ReadOnly Property TieCorrectedPValue As Double
Public ReadOnly Property GroupNames As String()
Public ReadOnly Property GroupCounts As Integer()
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If tables Is Nothing OrElse tables.Count = 0 Then Throw New ArgumentException("At least one Kruskal-Wallis result table is required.", NameOf(tables))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `SharedExcelLayerTests`, `Stage304ContractSnapshotTests`.

### `KruskalWallisApplicationService` — I

[Declaration L54](../../BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb#L54). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class KruskalWallisApplicationService
Private Sub New()
Public Shared Function Run(input As SpreadsheetRangeData) As KruskalWallisApplicationResult
Public Shared Function Run(input As SpreadsheetRangeData, options As KruskalWallisAnalysisOptions) As KruskalWallisApplicationResult
Public Shared Function Run(grouped As GroupedNumericData) As KruskalWallisApplicationResult
Public Shared Function Run(grouped As GroupedNumericData, options As KruskalWallisAnalysisOptions) As KruskalWallisApplicationResult
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`.

Tests: `SharedExcelLayerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/OneWayAnovaApplicationService.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisHandlerRegistryTests, AnalysisResultDocumentTests, SharedExcelLayerTests, SpreadsheetGroupedValidationTests.

### `OneWayAnovaApplicationResult` — I

[Declaration L13](../../BESHStat.Excel.Common/src/Analysis/OneWayAnovaApplicationService.vb#L13). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class OneWayAnovaApplicationResult
Public Sub New(table As Global.BESHStatNG.ResultTableOutputModel, fStatistic As Double, pValue As Double, groupNames As String(), groupCounts As Integer())
Public Sub New(tables As List(Of Global.BESHStatNG.ResultTableOutputModel), fStatistic As Double, pValue As Double, groupNames As String(), groupCounts As Integer(), welchResult As Global.BESHStatNG.TestResult, fisherLsdPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData, bonferroniPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData, tukeyKramerPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData, gamesHowellPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData)
Public ReadOnly Property Tables As List(Of Global.BESHStatNG.ResultTableOutputModel)
Public ReadOnly Property Table As Global.BESHStatNG.ResultTableOutputModel
Public ReadOnly Property FStatistic As Double
Public ReadOnly Property PValue As Double
Public ReadOnly Property GroupNames As String()
Public ReadOnly Property GroupCounts As Integer()
Public ReadOnly Property WelchResult As Global.BESHStatNG.TestResult
Public ReadOnly Property FisherLsdPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData
Public ReadOnly Property BonferroniPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData
Public ReadOnly Property TukeyKramerPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData
Public ReadOnly Property GamesHowellPlotData As Global.BESHStatNG.parametric.MultipleComparisonPlotData
Public ReadOnly Property HasWelch As Boolean
Public ReadOnly Property WelchFStatistic As Double
Public ReadOnly Property WelchPValue As Double
Public ReadOnly Property WelchDenominatorDf As Double
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If tables Is Nothing OrElse tables.Count = 0 Then Throw New ArgumentException("At least one one-way ANOVA result table is required.", NameOf(tables))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `SharedExcelLayerTests`, `SpreadsheetGroupedValidationTests`.

### `OneWayAnovaApplicationService` — I

[Declaration L107](../../BESHStat.Excel.Common/src/Analysis/OneWayAnovaApplicationService.vb#L107). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OneWayAnovaApplicationService
Private Sub New()
Public Shared Function Run(input As SpreadsheetRangeData) As OneWayAnovaApplicationResult
Public Shared Function Run(input As SpreadsheetRangeData, options As OneWayAnovaAnalysisOptions) As OneWayAnovaApplicationResult
Public Shared Function Run(grouped As GroupedNumericData) As OneWayAnovaApplicationResult
Public Shared Function Run(grouped As GroupedNumericData, options As OneWayAnovaAnalysisOptions) As OneWayAnovaApplicationResult
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`.

Tests: `SharedExcelLayerTests`, `SpreadsheetGroupedValidationTests`, `TypedNumericRangeParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AnalysisHandlerRegistryTests, AnalysisResultDocumentTests, OfficeOutputPlanAdapterTests, OutputDocumentTests.

### `SharedAnalysisApplicationService` — L

[Declaration L13](../../BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb#L13). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SharedAnalysisApplicationService
Private Sub New()
Public Shared Function Run(request As SpreadsheetAnalysisRequest) As SpreadsheetAnalysisResult
Public Shared Function Run(analysisId As String, input As SpreadsheetAnalysisInput, Optional options As SpreadsheetAnalysisOptions = Nothing) As SpreadsheetAnalysisResult
Public Shared Function RunDocument(analysisId As String, input As SpreadsheetAnalysisInput, Optional options As SpreadsheetAnalysisOptions = Nothing) As AnalysisResultDocument
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `OfficeOutputPlanAdapterTests`, `OutputDocumentTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`

Owner/consumers: Typed registry, Windows shared forms, Home.razor.

Options mutable, alpha default 0.05. Legacy result stores Tables/Metrics/Detail references but captures independent Document tables/metrics on construction. PrimaryTable may be Nothing. SpreadsheetAnalysisKind is compatibility only, never stable serialized feature identity.

Evidence/fixtures: Stage304; AnalysisResultDocumentTests.

### `SpreadsheetAnalysisKind` — L

[Declaration L13](../../BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb#L13). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetAnalysisKind
OneWayAnova = 1
KruskalWallis = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `OfficeOutputPlanAdapterTests`.

### `SpreadsheetAnalysisOptions` — F

[Declaration L22](../../BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb#L22). **Version/exposure:** No declared version; no wire promise.

```vb
Public MustInherit Class SpreadsheetAnalysisOptions
Public Overridable Function GetWarnings() As IEnumerable(Of AnalysisWarning)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `OfficeOutputPlanAdapterTests`.

### `OneWayAnovaAnalysisOptions` — F

[Declaration L33](../../BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb#L33). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class OneWayAnovaAnalysisOptions
Inherits SpreadsheetAnalysisOptions
Public Sub New()
Public Property IncludeWelch As Boolean
Public Property IncludeFisherLsd As Boolean
Public Property IncludeBonferroni As Boolean
Public Property IncludeTukeyKramer As Boolean
Public Property IncludeGamesHowell As Boolean
Public Property Alpha As Double
Public Overrides Function GetWarnings() As IEnumerable(Of AnalysisWarning)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/OneWayAnovaApplicationService.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `OfficeOutputPlanAdapterTests`.

### `KruskalWallisAnalysisOptions` — F

[Declaration L62](../../BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb#L62). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class KruskalWallisAnalysisOptions
Inherits SpreadsheetAnalysisOptions
Public Sub New()
Public Property IncludeDunnComparisons As Boolean
Public Property Alpha As Double
Public Overrides Function GetWarnings() As IEnumerable(Of AnalysisWarning)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `OfficeOutputPlanAdapterTests`.

### `SpreadsheetAnalysisRequest` — F

[Declaration L86](../../BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb#L86). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class SpreadsheetAnalysisRequest
Public Sub New()
Public Sub New(analysisKind As SpreadsheetAnalysisKind, input As SpreadsheetAnalysisInput, options As SpreadsheetAnalysisOptions)
Public Property AnalysisKind As SpreadsheetAnalysisKind
Public Property Input As SpreadsheetAnalysisInput
Public Property Options As SpreadsheetAnalysisOptions
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `AnalysisHandlerRegistryTests`, `OfficeOutputPlanAdapterTests`, `OutputLayoutPlannerTests`, `PValuePresentationPlanTests`.

### `AnalysisMetricNames` — F

[Declaration L110](../../BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb#L110). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisMetricNames
Private Sub New()
Public Const FStatistic As String = "FStatistic"
Public Const PValue As String = "PValue"
Public Const WelchFStatistic As String = "WelchFStatistic"
Public Const WelchPValue As String = "WelchPValue"
Public Const WelchDenominatorDf As String = "WelchDenominatorDf"
Public Const HStatistic As String = "HStatistic"
Public Const TieCorrectedHStatistic As String = "TieCorrectedHStatistic"
Public Const TieCorrectedPValue As String = "TieCorrectedPValue"
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb`.

Tests: `AnalysisResultDocumentTests`, `SharedExcelLayerTests`.

### `SpreadsheetAnalysisResult` — L

[Declaration L130](../../BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb#L130). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class SpreadsheetAnalysisResult
Public Sub New(analysisKind As SpreadsheetAnalysisKind, tables As List(Of Global.BESHStatNG.ResultTableOutputModel), metrics As Dictionary(Of String, Double), detail As Object)
Public Sub New(analysisKind As SpreadsheetAnalysisKind, tables As List(Of Global.BESHStatNG.ResultTableOutputModel), metrics As Dictionary(Of String, Double), detail As Object, warnings As IEnumerable(Of AnalysisWarning))
Public ReadOnly Property AnalysisKind As SpreadsheetAnalysisKind
Public ReadOnly Property Tables As List(Of Global.BESHStatNG.ResultTableOutputModel)
Public ReadOnly Property Metrics As Dictionary(Of String, Double)
Public ReadOnly Property Detail As Object
Public ReadOnly Property Document As AnalysisResultDocument
Public ReadOnly Property PrimaryTable As Global.BESHStatNG.ResultTableOutputModel
Public Function TryGetMetric(name As String, ByRef value As Double) As Boolean
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If tables Is Nothing Then Throw New ArgumentNullException(NameOf(tables))`
- `If metrics Is Nothing Then Throw New ArgumentNullException(NameOf(metrics))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/IAnalysisHandler.vb`, `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`.

Tests: `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `OutputDocumentTests`, `SharedExcelLayerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb`

Owner/consumers: Acquisition/analysis handlers and both host adapters.

Mutable staging DTOs: constructors establish defaults, not full validation. Rows/cell arrays are zero-based; starts and declared source positions are one-based, zero unknown. Blank IDs are unknown; selection revisions are host observations. See proposal source/row rules.

Evidence/fixtures: Stage304; acquisition canonical-two-ranges.json.

### `SpreadsheetAnalysisInput` — F

[Declaration L14](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb#L14). **Version/exposure:** No declared version; no wire promise.

```vb
Public MustInherit Class SpreadsheetAnalysisInput
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisDescriptor.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/IAnalysisHandler.vb`.

Tests: `AdvancedInputParserTests`, `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `LongitudinalParserTests`.

### `SpreadsheetCellKind` — F

[Declaration L22](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb#L22). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetCellKind
Blank = 0
Number = 1
Text = 2
Bool = 3
CellError = 4
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Input/CellValuePolicy.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`, `BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb`.

Tests: `AcquisitionSnapshotTests`, `AdvancedInputParserTests`, `GateBOutputContractCompatibilityTests`, `LongitudinalParserTests`.

### `SpreadsheetCellValue` — F

[Declaration L30](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb#L30). **Version/exposure:** No declared version. Existing primitive host acquisition/capability/target JSON interop; retain observed field names/types/defaults and numeric enums. Mutable setters require downstream validation; no durable identity or generalized wire-extension promise.

```vb
Public Class SpreadsheetCellValue
Public Sub New()
Public Property Kind As SpreadsheetCellKind
Public Property NumberValue As Double
Public Property TextValue As String
Public Property BooleanValue As Boolean
Public Property NumberFormat As String
Public Property NativeErrorCode As String = String.Empty
Public Shared Function BlankCell() As SpreadsheetCellValue
Public Shared Function NumberCell(value As Double) As SpreadsheetCellValue
Public Shared Function TextCell(value As String) As SpreadsheetCellValue
Public Shared Function BooleanCell(value As Boolean) As SpreadsheetCellValue
Public Shared Function ErrorCell(value As String) As SpreadsheetCellValue
Public Shared Function ErrorCell(value As String, nativeErrorCode As String) As SpreadsheetCellValue
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/CellValuePolicy.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`.

Tests: `AdvancedInputParserTests`, `AnalysisHandlerRegistryTests`, `CellValuePolicyTests`, `LongitudinalParserTests`.

### `SpreadsheetRangeData` — F

[Declaration L89](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb#L89). **Version/exposure:** No declared version. Existing primitive host acquisition/capability/target JSON interop; retain observed field names/types/defaults and numeric enums. Mutable setters require downstream validation; no durable identity or generalized wire-extension promise.

```vb
Public Class SpreadsheetRangeData
Inherits SpreadsheetAnalysisInput
Public Sub New()
Public Property WorksheetName As String
Public Property Address As String
Public Property WorkbookId As String
Public Property WorkbookName As String
Public Property WorksheetId As String
Public Property DateSystem As SpreadsheetDateSystem
Public Property DateSerialOrigin As Nullable(Of DateTime)
Public Property AddressDialect As SpreadsheetAddressDialect
Public Property SelectionId As String
Public Property SelectionRevision As String
Public Property StartRow As Integer
Public Property StartColumn As Integer
Public ReadOnly Property QualifiedAddress As String
Public Function ToReference() As SpreadsheetRangeReference
Public Property RowCount As Integer
Public Property ColumnCount As Integer
Public Property Rows As List(Of List(Of SpreadsheetCellValue))
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb`.

Tests: `AdvancedInputParserTests`, `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `CellValuePolicyTests`.

### `SpreadsheetOutputDestination` — F

[Declaration L159](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb#L159). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetOutputDestination
CurrentWorksheet = 0
NewWorksheet = 1
NewWorkbook = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Host/HostCapabilities.vb`, `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`.

Tests: `CompositeOutputTests`, `GateBOutputContractCompatibilityTests`, `HostCapabilitiesTests`, `OfficeOutputPlanAdapterTests`.

### `SpreadsheetOutputTarget` — F

[Declaration L165](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb#L165). **Version/exposure:** No declared version. Existing primitive host acquisition/capability/target JSON interop; retain observed field names/types/defaults and numeric enums. Mutable setters require downstream validation; no durable identity or generalized wire-extension promise.

```vb
Public Class SpreadsheetOutputTarget
Public Sub New()
Public Property Destination As SpreadsheetOutputDestination
Public Property WorksheetName As String
Public Property StartRow As Integer
Public Property StartColumn As Integer
Public Sub EnsureDefaults()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Host/HostCapabilities.vb`, `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb`, `BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb`.

Tests: `CompositeOutputTests`, `GateBOutputContractCompatibilityTests`, `HostCapabilitiesTests`, `OfficeOutputPlanAdapterTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb`

Owner/consumers: Windows/Office acquisition, typed parsers.

Mutable references/address objects, no immutable-snapshot promise. A1 factories validate a single positive bounded rectangle; HostNative factories require positive representable coordinates without Excel parsing. Selection.Validate rejects missing sheets/invalid ranges. QualifiedAddress is display, never durable identity.

Evidence/fixtures: SpreadsheetRangeIdentityTests; AcquisitionSnapshotTests.

### `SpreadsheetAddressDialect` — F

[Declaration L15](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb#L15). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetAddressDialect
Unknown = 0
ExcelA1 = 1
HostNative = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb`.

Tests: `AdvancedInputParserTests`, `GateBOutputContractCompatibilityTests`, `SpreadsheetRangeIdentityTests`, `AcquisitionAdapterTests`.

### `SpreadsheetRangeAddress` — F

[Declaration L22](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb#L22). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetRangeAddress
Public Const ExcelMaxRows As Integer = 1048576
Public Const ExcelMaxColumns As Integer = 16384
Private Sub New()
Public Property WorkbookName As String = String.Empty
Public Property WorksheetName As String = String.Empty
Public Property Address As String = String.Empty
Public Property StartRow As Integer
Public Property StartColumn As Integer
Public Property EndRow As Integer
Public Property EndColumn As Integer
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property QualifiedAddress As String
Public Shared Function Parse(text As String, Optional defaultWorksheetName As String = Nothing) As SpreadsheetRangeAddress
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb`.

Tests: `SpreadsheetRangeIdentityTests`.

### `SpreadsheetRangeReference` — F

[Declaration L143](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb#L143). **Version/exposure:** No declared version. Existing primitive host acquisition/capability/target JSON interop; retain observed field names/types/defaults and numeric enums. Mutable setters require downstream validation; no durable identity or generalized wire-extension promise.

```vb
Public NotInheritable Class SpreadsheetRangeReference
Public Sub New()
Public Property WorkbookId As String
Public Property WorkbookName As String
Public Property WorksheetId As String
Public Property WorksheetName As String
Public Property Address As String
Public Property AddressDialect As SpreadsheetAddressDialect = SpreadsheetAddressDialect.ExcelA1
Public Property StartRow As Integer
Public Property StartColumn As Integer
Public Property RowCount As Integer
Public Property ColumnCount As Integer
Public Property SelectionId As String
Public Property SelectionRevision As String
Public ReadOnly Property QualifiedAddress As String
Public Shared Function FromCoordinates(worksheetName As String, startRow As Integer, startColumn As Integer, rowCount As Integer, columnCount As Integer, Optional displayAddress As String = Nothing) As SpreadsheetRangeReference
Public Shared Function FromUserInput(worksheetNameOrAddress As String, address As String) As SpreadsheetRangeReference
Public Shared Function FromAddress(address As String, Optional worksheetName As String = Nothing, Optional workbookName As String = Nothing) As SpreadsheetRangeReference
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `SpreadsheetRangeIdentityTests`, `AcquisitionAdapterTests`.

### `SpreadsheetRangeSelection` — F

[Declaration L259](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb#L259). **Version/exposure:** No declared version. Existing primitive host acquisition/capability/target JSON interop; retain observed field names/types/defaults and numeric enums. Mutable setters require downstream validation; no durable identity or generalized wire-extension promise.

```vb
Public NotInheritable Class SpreadsheetRangeSelection
Public Sub New()
Public Property Ranges As List(Of SpreadsheetRangeReference)
Public Property SelectionId As String
Public Sub Validate()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `AcquisitionSnapshotTests`, `SpreadsheetRangeIdentityTests`, `AcquisitionAdapterTests`.

### `SpreadsheetRangeSetData` — F

[Declaration L290](../../BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb#L290). **Version/exposure:** No declared version. Existing primitive host acquisition/capability/target JSON interop; retain observed field names/types/defaults and numeric enums. Mutable setters require downstream validation; no durable identity or generalized wire-extension promise.

```vb
Public NotInheritable Class SpreadsheetRangeSetData
Inherits SpreadsheetAnalysisInput
Public Sub New()
Public Property Ranges As List(Of SpreadsheetRangeData)
Public Property SelectionId As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `AcquisitionSnapshotTests`, `SpreadsheetRangeIdentityTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Host/HostCapabilities.vb`

Owner/consumers: Host adapters; analysis/target preflight; Home.razor.

Mutable transport snapshot with identified spreadsheet admission (IsExcel compatibility OR IsSpreadsheetHost). Ordinal capability IDs; only supported operations advertised. Office NewWorkbook explicitly rejected even with false capability report. Profiles are test/diagnostic examples, not runtime detection. Result/exceptions immutable, no version property.

Evidence/fixtures: HostCapabilitiesTests; JS hostCapabilities tests.

### `HostCapabilityIds` — F

[Declaration L11](../../BESHStat.Excel.Common/src/Host/HostCapabilities.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class HostCapabilityIds
Public Const ReadSelectedRange As String = "input.read-selected-range"
Public Const WriteResultTables As String = "output.write-result-tables"
Public Const CurrentWorksheet As String = "output.current-worksheet"
Public Const NewWorksheet As String = "output.new-worksheet"
Public Const NewWorkbook As String = "output.new-workbook"
Private Sub New()
Public Shared Function ForDestination(destination As SpreadsheetOutputDestination) As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/TargetPlanner.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `GateBOutputContractCompatibilityTests`, `HostCapabilitiesTests`, `OutputTargetPlannerTests`, `SharedOutputPlanTests`.

### `HostCapabilitySnapshot` — F

[Declaration L37](../../BESHStat.Excel.Common/src/Host/HostCapabilities.vb#L37). **Version/exposure:** No declared version. Existing primitive host acquisition/capability/target JSON interop; retain observed field names/types/defaults and numeric enums. Mutable setters require downstream validation; no durable identity or generalized wire-extension promise.

```vb
Public NotInheritable Class HostCapabilitySnapshot
Public Sub New()
Public Property Host As String
Public Property Platform As String
Public Property IsExcel As Boolean
Public Property IsSpreadsheetHost As Boolean
Public Property Capabilities As List(Of String)
Public Function Supports(capabilityId As String) As Boolean
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/TargetPlanner.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `HostCapabilitiesTests`.

### `HostCapabilityProfiles` — I

[Declaration L72](../../BESHStat.Excel.Common/src/Host/HostCapabilities.vb#L72). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class HostCapabilityProfiles
Private Sub New()
Public Shared Function WindowsExcelDna() As HostCapabilitySnapshot
Public Shared Function OfficeJsExcel(platform As String) As HostCapabilitySnapshot
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `CompositeOutputTests`, `HostCapabilitiesTests`, `OfficeOutputPlanAdapterTests`, `OutputTargetPlannerTests`.

### `CapabilityPreflightCodes` — F

[Declaration L110](../../BESHStat.Excel.Common/src/Host/HostCapabilities.vb#L110). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class CapabilityPreflightCodes
Public Const HostUnavailable As String = "host.unavailable"
Public Const UnknownAnalysis As String = "analysis.unknown"
Public Const UnsupportedAnalysis As String = "analysis.unsupported"
Public Const MissingCapability As String = "host.capability-missing"
Public Const UnsupportedDestination As String = "output.destination-unsupported"
Public Const InvalidTarget As String = "output.target-invalid"
Public Const SheetNameCollision As String = "output.sheet-name-collision"
Public Const UnwritableTarget As String = "output.target-unwritable"
Public Const MergedTarget As String = "output.target-merged"
Private Sub New()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/TargetPlanner.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `HostCapabilitiesTests`.

### `CapabilityPreflightResult` — F

[Declaration L126](../../BESHStat.Excel.Common/src/Host/HostCapabilities.vb#L126). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class CapabilityPreflightResult
Private Sub New(isSupported As Boolean, code As String, message As String, missingCapability As String)
Public ReadOnly Property IsSupported As Boolean
Public ReadOnly Property Code As String
Public ReadOnly Property Message As String
Public ReadOnly Property MissingCapability As String
Public Shared Function Success() As CapabilityPreflightResult
Public Shared Function Rejected(code As String, message As String, Optional missingCapability As String = Nothing) As CapabilityPreflightResult
Public Sub EnsureSupported()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/TargetPlanner.vb`.

Tests: `HostCapabilitiesTests`.

### `HostCapabilityException` — F

[Declaration L152](../../BESHStat.Excel.Common/src/Host/HostCapabilities.vb#L152). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class HostCapabilityException
Inherits NotSupportedException
Public Sub New(result As CapabilityPreflightResult)
Public ReadOnly Property Code As String
Public ReadOnly Property MissingCapability As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/WriteResult.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `HostCapabilitiesTests`, `OutputTargetPlannerTests`, `SharedOutputPlanTests`.

### `HostCapabilityPreflight` — I

[Declaration L167](../../BESHStat.Excel.Common/src/Host/HostCapabilities.vb#L167). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class HostCapabilityPreflight
Private Sub New()
Public Shared Function CheckSelection(snapshot As HostCapabilitySnapshot) As CapabilityPreflightResult
Public Shared Function CheckAnalysis(analysisId As String, snapshot As HostCapabilitySnapshot) As CapabilityPreflightResult
Public Shared Function CheckOutput(snapshot As HostCapabilitySnapshot, target As SpreadsheetOutputTarget) As CapabilityPreflightResult
Public Shared Function CheckRunAndWrite(analysisId As String, snapshot As HostCapabilitySnapshot, target As SpreadsheetOutputTarget, Optional readsSelection As Boolean = True) As CapabilityPreflightResult
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/TargetPlanner.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `HostCapabilitiesTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AdvancedInputParserTests.

### `AdvancedInputParsers` — I

[Declaration L9](../../BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb#L9). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AdvancedInputParsers
Private Sub New()
Public Shared Function ParseMatrix(input As SpreadsheetRangeData, Optional options As SpreadsheetMatrixParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetMatrixInput)
Public Shared Function ParseClassification(input As SpreadsheetRangeData, Optional options As SpreadsheetClassificationParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetClassificationInput)
Public Shared Function ParsePsm(input As SpreadsheetRangeData, Optional options As SpreadsheetPsmParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetPsmInput)
Public Shared Function ParseSpc(input As SpreadsheetRangeData, Optional options As SpreadsheetSpcParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetSpcInput)
Public Shared Function ParseChartSource(input As SpreadsheetRangeData, Optional options As SpreadsheetChartSourceParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetChartSourceInput)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `AdvancedInputParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`

Owner/consumers: AdvancedInputParsers.

Matrix/classification/PSM/SPC/chart inputs retain source frame/typed rows and original row positions. Lists copied read-only; array projections are fresh. Options mutable with zero-based role columns, -1 where optional. Matrix shape/class/treatment/order semantics validated by parsers, not all by constructors.

Evidence/fixtures: AdvancedInputParserTests.

### `SpreadsheetMatrixParseOptions` — F

[Declaration L9](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L9). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class SpreadsheetMatrixParseOptions
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
Public Property NumericColumnIndices As Integer() = Nothing
Public Property MinimumRows As Integer = 1
Public Property MinimumColumns As Integer = 1
Public Property RequireSquare As Boolean = False
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

Tests: `AdvancedInputParserTests`.

### `SpreadsheetClassificationParseOptions` — F

[Declaration L20](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L20). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class SpreadsheetClassificationParseOptions
Inherits SpreadsheetMatrixParseOptions
Public Property ClassColumnIndex As Integer = 0
Public Property MinimumClasses As Integer = 2
Public Property IgnoreCategoryCase As Boolean = False
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

Tests: `AdvancedInputParserTests`.

### `SpreadsheetPsmParseOptions` — F

[Declaration L27](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L27). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetPsmParseOptions
Inherits SpreadsheetMatrixParseOptions
Public Property TreatmentColumnIndex As Integer = 0
Public Property TreatedLevel As SpreadsheetCategoryValue = New SpreadsheetCategoryValue(SpreadsheetCellValue.NumberCell(1))
Public Property ControlLevel As SpreadsheetCategoryValue = New SpreadsheetCategoryValue(SpreadsheetCellValue.NumberCell(0))
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

Tests: `AdvancedInputParserTests`.

### `SpreadsheetSpcParseOptions` — F

[Declaration L35](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L35). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class SpreadsheetSpcParseOptions
Inherits SpreadsheetMatrixParseOptions
Public Property OrderColumnIndex As Integer = -1
Public Property InterpretOrderAsDate As Boolean = False
Public Property SubgroupColumnIndex As Integer = -1
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

Tests: `AdvancedInputParserTests`.

### `SpreadsheetChartSourceParseOptions` — F

[Declaration L44](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L44). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetChartSourceParseOptions
Inherits SpreadsheetMatrixParseOptions
Public Property XColumnIndex As Integer = -1
Public Property RequireNumericX As Boolean = False
Public Property RequireAscendingX As Boolean = False
Public Property InterpretXAsDate As Boolean = False
Public Property GroupColumnIndex As Integer = -1
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

Tests: `AdvancedInputParserTests`.

### `SpreadsheetAdvancedRow` — F

[Declaration L55](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L55). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetAdvancedRow
Friend Sub New(values As IEnumerable(Of SpreadsheetNumericObservation), label As SpreadsheetFrameCell, group As SpreadsheetFrameCell, coordinate As SpreadsheetFrameCell, coordinateValue As Nullable(Of Double), coordinateDate As Nullable(Of DateTime), treated As Nullable(Of Boolean), labelLevel As SpreadsheetCategoryValue)
Public ReadOnly Property Values As IReadOnlyList(Of SpreadsheetNumericObservation)
Public ReadOnly Property Label As SpreadsheetFrameCell
Public ReadOnly Property LabelLevel As SpreadsheetCategoryValue
Public ReadOnly Property Group As SpreadsheetFrameCell
Public ReadOnly Property Coordinate As SpreadsheetFrameCell
Public ReadOnly Property CoordinateValue As Nullable(Of Double)
Public ReadOnly Property CoordinateDate As Nullable(Of DateTime)
Public ReadOnly Property IsTreated As Nullable(Of Boolean)
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property WorksheetRow As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

### `SpreadsheetMatrixInput` — F

[Declaration L91](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L91). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class SpreadsheetMatrixInput
Inherits SpreadsheetAnalysisInput
Friend Sub New(source As SpreadsheetRectangularDataFrameInput, columns As IEnumerable(Of SpreadsheetFrameColumn), rows As IEnumerable(Of SpreadsheetAdvancedRow), Optional dateSerialOrigin As Nullable(Of DateTime) = Nothing)
Public ReadOnly Property Source As SpreadsheetRectangularDataFrameInput
Public ReadOnly Property DateSerialOrigin As Nullable(Of DateTime)
Public ReadOnly Property Columns As IReadOnlyList(Of SpreadsheetFrameColumn)
Public ReadOnly Property Rows As IReadOnlyList(Of SpreadsheetAdvancedRow)
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public Function ToArray() As Double(,)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

### `SpreadsheetClassificationInput` — F

[Declaration L125](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L125). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetClassificationInput
Inherits SpreadsheetMatrixInput
Friend Sub New(matrix As SpreadsheetMatrixInput, levels As IEnumerable(Of SpreadsheetCategoryValue))
Public ReadOnly Property Classes As IReadOnlyList(Of SpreadsheetCategoryValue)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

### `SpreadsheetPsmInput` — F

[Declaration L134](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L134). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetPsmInput
Inherits SpreadsheetMatrixInput
Friend Sub New(matrix As SpreadsheetMatrixInput, treated As SpreadsheetCategoryValue, control As SpreadsheetCategoryValue)
Public ReadOnly Property TreatedLevel As SpreadsheetCategoryValue
Public ReadOnly Property ControlLevel As SpreadsheetCategoryValue
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

### `SpreadsheetSpcInput` — F

[Declaration L145](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L145). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetSpcInput
Inherits SpreadsheetMatrixInput
Friend Sub New(matrix As SpreadsheetMatrixInput)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

### `SpreadsheetChartSourceInput` — F

[Declaration L152](../../BESHStat.Excel.Common/src/Input/AdvancedInputs.vb#L152). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetChartSourceInput
Inherits SpreadsheetMatrixInput
Friend Sub New(matrix As SpreadsheetMatrixInput)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/CellValuePolicy.vb`

Owner/consumers: All typed input parsers; host acquisition policies.

Policy options/evaluation are mutable. Defaults: only Blank missing, reject numeric text/errors and Boolean/date coercion. Numeric evaluation rejects nonfinite values; error raw/native identity survives. Date conversion requires verified Excel basis or explicit midnight origin, never both; 1900 serial [60,61) is rejected. Error canonicalization differs intentionally from OutputCell explicit ordinal mapping.

Evidence/fixtures: CellValuePolicyTests; acquisition fixtures.

### `SpreadsheetDateSystem` — F

[Declaration L10](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetDateSystem
Unknown = 0
Excel1900 = 1
Excel1904 = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Contracts/SpreadsheetContracts.vb`, `BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb`.

Tests: `AcquisitionSnapshotTests`, `AdvancedInputParserTests`, `CellValuePolicyTests`, `LongitudinalParserTests`.

### `SpreadsheetMissingTextPolicy` — F

[Declaration L16](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L16). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetMissingTextPolicy
BlankOnly = 0
BlankOrEmptyText = 1
BlankOrWhitespaceText = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Tests: `AdvancedInputParserTests`, `CellValuePolicyTests`, `RectangularCategoricalParserTests`, `SpreadsheetGroupedValidationTests`.

### `SpreadsheetErrorPolicy` — F

[Declaration L22](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L22). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetErrorPolicy
Reject = 0
Preserve = 1
TreatAsMissing = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`, `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`, `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

Tests: `AcquisitionSnapshotTests`, `AdvancedInputParserTests`, `CellValuePolicyTests`, `RectangularCategoricalParserTests`.

### `SpreadsheetNumericTextPolicy` — F

[Declaration L28](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L28). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetNumericTextPolicy
Reject = 0
Invariant = 1
ExplicitCulture = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Tests: `AdvancedInputParserTests`, `CellValuePolicyTests`, `ModelInputCompilerTests`, `RectangularCategoricalParserTests`.

### `SpreadsheetCellPolicyOptions` — F

[Declaration L38](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L38). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCellPolicyOptions
Public Property MissingText As SpreadsheetMissingTextPolicy = SpreadsheetMissingTextPolicy.BlankOnly
Public Property Errors As SpreadsheetErrorPolicy = SpreadsheetErrorPolicy.Reject
Public Property NumericText As SpreadsheetNumericTextPolicy = SpreadsheetNumericTextPolicy.Reject
Public Property NumericCultureName As String = "en-US"
Public Property AllowBooleanNumbers As Boolean
Public Property AllowDateSerialNumbers As Boolean
Public Function GetNumericCulture() As CultureInfo
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb`.

Tests: `AcquisitionSnapshotTests`, `CellValuePolicyTests`, `ModelInputCompilerTests`, `RectangularCategoricalParserTests`.

### `SpreadsheetExcelErrorValue` — F

[Declaration L62](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L62). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetExcelErrorValue
Public Sub New(rawText As String)
Public Sub New(rawText As String, nativeCode As String)
Public ReadOnly Property RawText As String
Public ReadOnly Property Code As String
Public ReadOnly Property NativeCode As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `AcquisitionSnapshotTests`, `CellValuePolicyTests`.

### `SpreadsheetExcelErrors` — I

[Declaration L80](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L80). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetExcelErrors
Private Sub New()
Public Shared Function Canonicalize(value As String) As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `CellValuePolicyTests`.

### `SpreadsheetCellEvaluation` — I

[Declaration L95](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L95). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCellEvaluation
Public Property IsMissing As Boolean
Public Property IsError As Boolean
Public Property [Error] As SpreadsheetExcelErrorValue
Public Property HasNumericValue As Boolean
Public Property NumericValue As Double
Public Property HasDateValue As Boolean
Public Property DateValue As DateTime
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetCellValuePolicy` — I

[Declaration L109](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L109). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCellValuePolicy
Private Sub New()
Public Shared Function Evaluate(cell As SpreadsheetCellValue, Optional options As SpreadsheetCellPolicyOptions = Nothing, Optional dateSystem As SpreadsheetDateSystem = SpreadsheetDateSystem.Unknown, Optional interpretNumberAsDate As Boolean = False) As SpreadsheetCellEvaluation
Public Shared Function Evaluate(cell As SpreadsheetCellValue, options As SpreadsheetCellPolicyOptions, dateSystem As SpreadsheetDateSystem, interpretNumberAsDate As Boolean, dateSerialOrigin As Nullable(Of DateTime)) As SpreadsheetCellEvaluation
Public Shared Function ReadNumeric(cell As SpreadsheetCellValue, Optional options As SpreadsheetCellPolicyOptions = Nothing) As Double
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

Tests: `AcquisitionSnapshotTests`, `CellValuePolicyTests`, `AcquisitionAdapterTests`.

### `SpreadsheetDateSerialConverter` — I

[Declaration L206](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L206). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetDateSerialConverter
Private Sub New()
Public Shared Function TryToDateTime(serial As Double, origin As DateTime, ByRef value As DateTime) As Boolean
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `CellValuePolicyTests`.

### `ExcelDateSerialConverter` — I

[Declaration L223](../../BESHStat.Excel.Common/src/Input/CellValuePolicy.vb#L223). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class ExcelDateSerialConverter
Private Sub New()
Public Shared Function TryToDateTime(serial As Double, basis As SpreadsheetDateSystem, ByRef value As DateTime) As Boolean
Public Shared Function ToSerial(value As DateTime, basis As SpreadsheetDateSystem) As Double
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `CellValuePolicyTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`

Owner/consumers: GroupedNumericRangeParser configured overloads.

Options mutable, role indices zero-based. Required/None/Auto headers and column/row/long orientation are explicit. Diagnostics preserve selected and absolute locations, but resulting GroupedNumericData retains only values/names. Auto must not silently reinterpret ambiguous numeric headers.

Evidence/fixtures: SpreadsheetGroupedValidationTests.

### `SpreadsheetGroupedOrientation` — F

[Declaration L9](../../BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb#L9). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetGroupedOrientation
GroupsInColumns = 0
GroupsInRows = 1
LongFormat = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `SpreadsheetGroupedValidationTests`, `TypedNumericRangeParserTests`.

### `SpreadsheetHeaderMode` — F

[Declaration L15](../../BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb#L15). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetHeaderMode
Required = 0
None = 1
Auto = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb`.

Tests: `AdvancedInputParserTests`, `LongitudinalParserTests`, `ModelInputCompilerTests`, `RectangularCategoricalParserTests`.

### `SpreadsheetGroupedParseOptions` — F

[Declaration L26](../../BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb#L26). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetGroupedParseOptions
Public Property Orientation As SpreadsheetGroupedOrientation = SpreadsheetGroupedOrientation.GroupsInColumns
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
Public Property CategoryColumnIndex As Integer = 0
Public Property ValueColumnIndex As Integer = 1
Public Property MinimumObservationsPerGroup As Integer = 2
Public Property WarnOnSkippedObservations As Boolean = False
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
Public Shared Function LegacyWideHeaders() As SpreadsheetGroupedParseOptions
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb`, `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `SpreadsheetGroupedValidationTests`, `TypedNumericRangeParserTests`.

### `SpreadsheetGroupedParseResult` — F

[Declaration L41](../../BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb#L41). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetGroupedParseResult
Public Sub New(data As GroupedNumericData, validation As SpreadsheetValidationReport)
Public ReadOnly Property Data As GroupedNumericData
Public ReadOnly Property Validation As SpreadsheetValidationReport
Public ReadOnly Property Success As Boolean
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If validation Is Nothing Then Throw New ArgumentNullException(NameOf(validation))`
- `If validation.HasErrors AndAlso data IsNot Nothing Then Throw New ArgumentException("An invalid parse cannot contain grouped data.", NameOf(data))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb`, `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

Tests: `SpreadsheetGroupedValidationTests`.

### `ConfigurableGroupedNumericParser` — I

[Declaration L64](../../BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb#L64). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class ConfigurableGroupedNumericParser
Private Sub New()
Public Shared Function Parse(input As SpreadsheetRangeData, Optional options As SpreadsheetGroupedParseOptions = Nothing) As SpreadsheetGroupedParseResult
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb`, `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb`

Owner/consumers: ANOVA/KW application services and registry.

LEGACY: GroupedNumericData stores caller arrays directly, no row provenance; GroupCounts is computed. Old Parse requires header + at least two numeric observations per group; blanks skipped independently; numeric text/errors/nonfinite rejected. Configured path delegates to existing parser. Do not relabel grouped positions as original worksheet rows.

Evidence/fixtures: Stage304 five fixtures; SharedExcelLayerTests.

### `GroupedNumericData` — L

[Declaration L12](../../BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb#L12). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class GroupedNumericData
Inherits SpreadsheetAnalysisInput
Public Sub New(groupNames As String(), groups As Double()())
Public ReadOnly Property GroupNames As String()
Public ReadOnly Property Groups As Double()()
Public ReadOnly Property GroupCounts As Integer()
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If groupNames Is Nothing Then Throw New ArgumentNullException(NameOf(groupNames))`
- `If groups Is Nothing Then Throw New ArgumentNullException(NameOf(groups))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisCatalog.vb`, `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb`.

Tests: `AnalysisCatalogTests`, `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `OfficeOutputPlanAdapterTests`.

### `GroupedNumericRangeParser` — L

[Declaration L41](../../BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb#L41). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class GroupedNumericRangeParser
Private Sub New()
Public Shared Function Parse(input As SpreadsheetRangeData) As GroupedNumericData
Public Shared Function ParseWithDiagnostics(input As SpreadsheetRangeData, Optional options As SpreadsheetGroupedParseOptions = Nothing) As SpreadsheetGroupedParseResult
Public Shared Function Parse(input As SpreadsheetRangeData, options As SpreadsheetGroupedParseOptions) As GroupedNumericData
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb`, `BESHStat.Excel.Common/src/Analysis/OneWayAnovaApplicationService.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`.

Tests: `AdvancedInputParserTests`, `AnalysisHandlerRegistryTests`, `CellValuePolicyTests`, `SharedExcelLayerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/InputSizeLimits.vb`

Owner/consumers: Bounded host acquisition; prototype output guards.

Immutable positive limits; finite bounded byte/time values are checked through ValidatePositive. Constructor defaults are acquisition/prototype limits, not permanent production output quotas. InputAcquisitionBudget is a mutable in-process cooperative budget: Checkpoint validates monotonic elapsed time/cancellation; AddRange checks dimension products, totals and estimates before updating counters. No host I/O.

Evidence/fixtures: InputSizeLimitsTests; JS largeRangeAcquisition tests.

### `InputSizeLimits` — F

[Declaration L14](../../BESHStat.Excel.Common/src/Input/InputSizeLimits.vb#L14). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class InputSizeLimits
Public Sub New(Optional maxRangeCells As Long = 10000, Optional maxTotalCells As Long = 10000, Optional maxRanges As Integer = 64, Optional maxChunkCells As Integer = 2000, Optional maxEstimatedBytes As Double = 67108864, Optional estimatedBytesPerCell As Double = 1024, Optional timeBudgetMilliseconds As Double = 30000)
Public ReadOnly Property MaxRangeCells As Long
Public ReadOnly Property MaxTotalCells As Long
Public ReadOnly Property MaxRanges As Integer
Public ReadOnly Property MaxChunkCells As Integer
Public ReadOnly Property MaxEstimatedBytes As Double
Public ReadOnly Property EstimatedBytesPerCell As Double
Public ReadOnly Property TimeBudgetMilliseconds As Double
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If maxRangeCells < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxRangeCells))`
- `If maxTotalCells < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxTotalCells))`
- `If maxRanges < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxRanges))`
- `If maxChunkCells < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxChunkCells))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`.

Tests: `InputSizeLimitsTests`, `AcquisitionAdapterTests`.

### `InputAcquisitionBudget` — I

[Declaration L54](../../BESHStat.Excel.Common/src/Input/InputSizeLimits.vb#L54). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class InputAcquisitionBudget
Public Sub New(Optional limits As InputSizeLimits = Nothing, Optional cancellation As CancellationToken = Nothing, Optional elapsedMilliseconds As Func(Of Double) = Nothing)
Public ReadOnly Property TotalCells As Long
Public ReadOnly Property RangeCount As Integer
Public Sub Checkpoint()
Public Sub AddRange(rows As Long, columns As Long)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `InputSizeLimitsTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb`

Owner/consumers: LongitudinalParsers.

Typed survival/longitudinal rows retain observation/frame-cell provenance, including original selected/worksheet row after exclusions. Lists copied read-only; typed row data retained. Options mutable with zero-based role indices. Constructor guards below are structural; parser owns time/event/visit/subject domain validation.

Evidence/fixtures: LongitudinalParserTests.

### `SpreadsheetSurvivalObservation` — F

[Declaration L11](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetSurvivalObservation
Public Sub New(time As SpreadsheetNumericObservation, entry As SpreadsheetNumericObservation, isEvent As Boolean, stratum As SpreadsheetCategoryValue)
Public ReadOnly Property Time As SpreadsheetNumericObservation
Public ReadOnly Property EntryTime As SpreadsheetNumericObservation
Public ReadOnly Property IsEvent As Boolean
Public ReadOnly Property Stratum As SpreadsheetCategoryValue
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property WorksheetRow As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If time Is Nothing Then Throw New ArgumentNullException(NameOf(time))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetSurvivalInput` — F

[Declaration L37](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L37). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetSurvivalInput
Inherits SpreadsheetAnalysisInput
Public Sub New(observations As IEnumerable(Of SpreadsheetSurvivalObservation))
Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetSurvivalObservation)
Public ReadOnly Property Count As Integer
Public ReadOnly Property EventCount As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))`
- `If observation Is Nothing Then Throw New ArgumentException("Survival rows cannot be null.", NameOf(observations))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetRepeatedObservation` — F

[Declaration L71](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L71). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetRepeatedObservation
Public Sub New(subject As SpreadsheetCategoryValue, visit As SpreadsheetCategoryValue, visitTime As Nullable(Of Double), visitDate As Nullable(Of DateTime), response As SpreadsheetNumericObservation, treatmentGroup As SpreadsheetCategoryValue)
Public ReadOnly Property Subject As SpreadsheetCategoryValue
Public ReadOnly Property Visit As SpreadsheetCategoryValue
Public ReadOnly Property VisitTime As Nullable(Of Double)
Public ReadOnly Property VisitDate As Nullable(Of DateTime)
Public ReadOnly Property Response As SpreadsheetNumericObservation
Public ReadOnly Property TreatmentGroup As SpreadsheetCategoryValue
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property WorksheetRow As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If subject Is Nothing Then Throw New ArgumentNullException(NameOf(subject))`
- `If response Is Nothing Then Throw New ArgumentNullException(NameOf(response))`
- `If visitTime.HasValue AndAlso (Double.IsNaN(visitTime.Value) OrElse Double.IsInfinity(visitTime.Value)) Then Throw New ArgumentException("Visit time must be finite.", NameOf(visitTime))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetRepeatedSubjectInput` — F

[Declaration L108](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L108). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetRepeatedSubjectInput
Inherits SpreadsheetAnalysisInput
Public Sub New(observations As IEnumerable(Of SpreadsheetRepeatedObservation))
Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetRepeatedObservation)
Public ReadOnly Property Subjects As IReadOnlyList(Of SpreadsheetCategoryValue)
Public ReadOnly Property Count As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))`
- `If observation Is Nothing Then Throw New ArgumentException("Repeated rows cannot be null.", NameOf(observations))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetMixedModelObservation` — F

[Declaration L143](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L143). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetMixedModelObservation
Public Sub New(repeated As SpreadsheetRepeatedObservation, covariates As IEnumerable(Of SpreadsheetFrameCell), randomGroups As IEnumerable(Of SpreadsheetCategoryValue))
Public ReadOnly Property Repeated As SpreadsheetRepeatedObservation
Public ReadOnly Property Covariates As IReadOnlyList(Of SpreadsheetFrameCell)
Public ReadOnly Property RandomGroups As IReadOnlyList(Of SpreadsheetCategoryValue)
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If repeated Is Nothing Then Throw New ArgumentNullException(NameOf(repeated))`
- `If covariates Is Nothing Then Throw New ArgumentNullException(NameOf(covariates))`
- `If randomGroups Is Nothing Then Throw New ArgumentNullException(NameOf(randomGroups))`
- `If c.Contains(Nothing) OrElse g.Contains(Nothing) Then Throw New ArgumentException("Mixed-model row contains null values.")`

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetMixedModelInput` — F

[Declaration L172](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L172). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetMixedModelInput
Inherits SpreadsheetAnalysisInput
Public Sub New(rows As IEnumerable(Of SpreadsheetMixedModelObservation), covariateNames As IEnumerable(Of String), randomGroupNames As IEnumerable(Of String))
Public ReadOnly Property Rows As IReadOnlyList(Of SpreadsheetMixedModelObservation)
Public ReadOnly Property CovariateNames As IReadOnlyList(Of String)
Public ReadOnly Property RandomGroupNames As IReadOnlyList(Of String)
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))`
- `If covariateNames Is Nothing Then Throw New ArgumentNullException(NameOf(covariateNames))`
- `If randomGroupNames Is Nothing Then Throw New ArgumentNullException(NameOf(randomGroupNames))`
- `If copied.Contains(Nothing) Then Throw New ArgumentException("Null mixed-model row.", NameOf(rows))`
- `If item.Covariates.Count <> cov.Count OrElse item.RandomGroups.Count <> groups.Count Then Throw New ArgumentException("Mixed-model row schema mismatch.", NameOf(rows))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetSurvivalParseOptions` — F

[Declaration L213](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L213). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetSurvivalParseOptions
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
Public Property TimeColumnIndex As Integer = 0
Public Property StatusColumnIndex As Integer = 1
Public Property EntryColumnIndex As Integer = -1
Public Property StratumColumnIndex As Integer = -1
Public Property MinimumObservations As Integer = 1
Public Property RequirePositiveTime As Boolean = True
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
Public Property AllowBooleanStatus As Boolean = False
Public Property EventText As String = Nothing
Public Property CensorText As String = Nothing
Public Property IgnoreStatusCase As Boolean = False
Public Property IgnoreCategoryCase As Boolean = False
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

Tests: `LongitudinalParserTests`.

### `SpreadsheetRepeatedParseOptions` — F

[Declaration L231](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L231). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class SpreadsheetRepeatedParseOptions
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
Public Property SubjectColumnIndex As Integer = 0
Public Property VisitColumnIndex As Integer = 1
Public Property ResponseColumnIndex As Integer = 2
Public Property GroupColumnIndex As Integer = -1
Public Property MinimumObservations As Integer = 1
Public Property MinimumSubjects As Integer = 1
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
Public Property RejectDuplicateSubjectVisits As Boolean = True
Public Property RequireAscendingVisits As Boolean = False
Public Property InterpretVisitAsDate As Boolean = False
Public Property IgnoreCategoryCase As Boolean = False
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

Tests: `LongitudinalParserTests`.

### `SpreadsheetMixedModelParseOptions` — F

[Declaration L249](../../BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb#L249). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetMixedModelParseOptions
Inherits SpreadsheetRepeatedParseOptions
Public Property FixedEffectColumnIndices As Integer() = Array.Empty(Of Integer)()
Public Property RandomGroupColumnIndices As Integer() = Array.Empty(Of Integer)()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

Tests: `LongitudinalParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: LongitudinalParserTests.

### `LongitudinalParsers` — I

[Declaration L11](../../BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class LongitudinalParsers
Private Sub New()
Public Shared Function ParseSurvival(input As SpreadsheetRangeData, Optional options As SpreadsheetSurvivalParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetSurvivalInput)
Public Shared Function ParseRepeatedSubjects(input As SpreadsheetRangeData, Optional options As SpreadsheetRepeatedParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetRepeatedSubjectInput)
Public Shared Function ParseMixedModel(input As SpreadsheetRangeData, Optional options As SpreadsheetMixedModelParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetMixedModelInput)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `LongitudinalParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/ModelInput.vb`

Owner/consumers: SpreadsheetModelInputCompiler; Core formula/design services.

Options mutable: zero-based role indices; RHS-only existing Core grammar. Source rows are original one-based selected/worksheet positions (zero absolute unknown), not dense design indices. Compiled arrays copied on construction/access; lists read-only, dimensions aligned. Category levels preserve typed identity; code is Core encoding, not a worksheet row.

Evidence/fixtures: ModelInputCompilerTests.

### `SpreadsheetModelParseOptions` — F

[Declaration L13](../../BESHStat.Excel.Common/src/Input/ModelInput.vb#L13). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetModelParseOptions
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
Public Property ResponseColumnIndex As Integer = 0
Public Property PredictorColumnIndices As Integer() = Nothing
Public Property CategoricalColumnIndices As Integer() = Array.Empty(Of Integer)()
Public Property FormulaText As String = Nothing
Public Property IncludeIntercept As Boolean = True
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
Public Property IgnoreCategoryCase As Boolean = False
Public Property MinimumCompleteRows As Integer = 2
Public Property MaximumExpandedColumns As Integer = 2048
Public Property RejectRankDeficiency As Boolean = False
Public Property AllowRelativeColumnLetters As Boolean = True
Public Property AllowAbsoluteColumnLetters As Boolean = False
Public Property AllowQuotedVariableNames As Boolean = True
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

Tests: `ModelInputCompilerTests`.

### `SpreadsheetModelSourceRow` — F

[Declaration L33](../../BESHStat.Excel.Common/src/Input/ModelInput.vb#L33). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetModelSourceRow
Public Sub New(selectedRow As Integer, worksheetRow As Integer)
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property WorksheetRow As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If selectedRow < 1 OrElse worksheetRow < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRow))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

### `SpreadsheetModelCategoryLevel` — F

[Declaration L46](../../BESHStat.Excel.Common/src/Input/ModelInput.vb#L46). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetModelCategoryLevel
Public Sub New(value As SpreadsheetCategoryValue, code As Double)
Public ReadOnly Property Value As SpreadsheetCategoryValue
Public ReadOnly Property Code As Double
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

### `SpreadsheetModelCategoricalColumn` — F

[Declaration L56](../../BESHStat.Excel.Common/src/Input/ModelInput.vb#L56). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetModelCategoricalColumn
Public Sub New(columnName As String, selectedColumn As Integer, levels As IEnumerable(Of SpreadsheetModelCategoryLevel))
Public ReadOnly Property ColumnName As String
Public ReadOnly Property SelectedColumn As Integer
Public ReadOnly Property Levels As IReadOnlyList(Of SpreadsheetModelCategoryLevel)
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If levels Is Nothing Then Throw New ArgumentNullException(NameOf(levels))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

### `SpreadsheetModelInput` — F

[Declaration L77](../../BESHStat.Excel.Common/src/Input/ModelInput.vb#L77). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetModelInput
Inherits SpreadsheetAnalysisInput
Public Sub New(responseName As String, response As Double(), rawPredictors As Double(,), design As Double(,), predictorNames As IEnumerable(Of String), rawPredictorNames As IEnumerable(Of String), requiredPredictorNames As IEnumerable(Of String), rows As IEnumerable(Of SpreadsheetModelSourceRow), categoricalColumns As IEnumerable(Of SpreadsheetModelCategoricalColumn), originalFormula As String, normalizedFormula As String, includesIntercept As Boolean, matrixRank As Integer, sourceWorkbookId As String, sourceWorksheetId As String, selectionRevision As String)
Public ReadOnly Property ResponseName As String
Public ReadOnly Property OriginalFormula As String
Public ReadOnly Property NormalizedFormula As String
Public ReadOnly Property IncludesIntercept As Boolean
Public ReadOnly Property MatrixRank As Integer
Public ReadOnly Property WorkbookId As String
Public ReadOnly Property WorksheetId As String
Public ReadOnly Property SelectionRevision As String
Public ReadOnly Property ObservationCount As Integer
Public ReadOnly Property DesignColumnCount As Integer
Public ReadOnly Property Response As Double()
Public ReadOnly Property RawPredictorMatrix As Double(,)
Public ReadOnly Property DesignMatrix As Double(,)
Public ReadOnly Property PredictorNames As IReadOnlyList(Of String)
Public ReadOnly Property RawPredictorNames As IReadOnlyList(Of String)
Public ReadOnly Property RequiredPredictorNames As IReadOnlyList(Of String)
Public ReadOnly Property SourceRows As IReadOnlyList(Of SpreadsheetModelSourceRow)
Public ReadOnly Property CategoricalColumns As IReadOnlyList(Of SpreadsheetModelCategoricalColumn)
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If response Is Nothing OrElse rawPredictors Is Nothing OrElse design Is Nothing Then Throw New ArgumentNullException(NameOf(response))`
- `If predictorNames Is Nothing OrElse rawPredictorNames Is Nothing OrElse requiredPredictorNames Is Nothing OrElse rows Is Nothing OrElse categoricalColumns Is Nothing Then Throw New ArgumentNullException(NameOf(predictorNames))`
- `If items.Count <> response.Length OrElse rawPredictors.GetLength(0) <> response.Length OrElse design.GetLength(0) <> response.Length OrElse columnNames.Count <> design.GetLength(1) OrElse rawNames.Count <> rawPredictors.GetLength(1) Then Throw New ArgumentException("Model arrays and schema have inconsistent dimensions.")`

Direct source consumers: `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

Tests: `ModelInputCompilerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: ModelInputCompilerTests.

### `SpreadsheetModelInputCompiler` — I

[Declaration L15](../../BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb#L15). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetModelInputCompiler
Private Sub New()
Public Shared Function Compile(input As SpreadsheetRangeData, Optional options As SpreadsheetModelParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetModelInput)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `ModelInputCompilerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb`

Owner/consumers: RectangularCategoricalParsers; model/advanced/longitudinal bridges.

Frame cells snapshot primitive values/native errors; finite numbers, known kind and positive selected coordinates. Frame copies every row and column list, validates rectangular nonnull cells. Headers excluded from Rows but original SelectedRow retained. Categories use disjoint type-tagged keys; blanks/errors forbidden. Contingency counts copy nested arrays, require nonnegative counts/dimensions and checked Int64 totals. Options mutable.

Evidence/fixtures: RectangularCategoricalParserTests.

### `SpreadsheetFrameCell` — F

[Declaration L12](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L12). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetFrameCell
Public Sub New(cell As SpreadsheetCellValue, selectedRow As Integer, selectedColumn As Integer, worksheetRow As Integer, worksheetColumn As Integer)
Public ReadOnly Property Kind As SpreadsheetCellKind
Public ReadOnly Property NumberValue As Double
Public ReadOnly Property TextValue As String
Public ReadOnly Property BooleanValue As Boolean
Public ReadOnly Property NumberFormat As String
Public ReadOnly Property NativeErrorCode As String
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property SelectedColumn As Integer
Public ReadOnly Property WorksheetRow As Integer
Public ReadOnly Property WorksheetColumn As Integer
Public Function ToCellValue() As SpreadsheetCellValue
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If selectedRow < 1 OrElse selectedColumn < 1 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRow))`
- `If Not [Enum].IsDefined(GetType(SpreadsheetCellKind), value.Kind) Then Throw New ArgumentException("Unknown cell kind.", NameOf(cell))`
- `If value.Kind = SpreadsheetCellKind.Number AndAlso (Double.IsNaN(value.NumberValue) OrElse Double.IsInfinity(value.NumberValue)) Then Throw New ArgumentException("Non-finite spreadsheet number.", NameOf(cell))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb`.

### `SpreadsheetFrameColumn` — F

[Declaration L51](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L51). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetFrameColumn
Public Sub New(name As String, selectedColumn As Integer, worksheetColumn As Integer)
Public ReadOnly Property Name As String
Public ReadOnly Property SelectedColumn As Integer
Public ReadOnly Property WorksheetColumn As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Column name cannot be blank.", NameOf(name))`
- `If selectedColumn < 1 OrElse worksheetColumn < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedColumn))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

### `SpreadsheetRectangularDataFrameInput` — F

[Declaration L65](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L65). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetRectangularDataFrameInput
Inherits SpreadsheetAnalysisInput
Public Sub New(columns As IEnumerable(Of SpreadsheetFrameColumn), rows As IEnumerable(Of IReadOnlyList(Of SpreadsheetFrameCell)), workbookName As String, worksheetName As String, worksheetId As String, Optional workbookId As String = Nothing, Optional sourceAddress As String = Nothing, Optional selectionId As String = Nothing, Optional selectionRevision As String = Nothing, Optional dateSystem As SpreadsheetDateSystem = SpreadsheetDateSystem.Unknown)
Public ReadOnly Property WorkbookId As String
Public ReadOnly Property SourceAddress As String
Public ReadOnly Property SelectionId As String
Public ReadOnly Property SelectionRevision As String
Public ReadOnly Property DateSystem As SpreadsheetDateSystem
Public ReadOnly Property WorkbookName As String
Public ReadOnly Property WorksheetName As String
Public ReadOnly Property WorksheetId As String
Public ReadOnly Property Columns As IReadOnlyList(Of SpreadsheetFrameColumn)
Public ReadOnly Property Rows As IReadOnlyList(Of IReadOnlyList(Of SpreadsheetFrameCell))
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public Function Cell(rowIndex As Integer, columnIndex As Integer) As SpreadsheetFrameCell
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))`
- `If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))`
- `If columnCopy.Count = 0 OrElse columnCopy.Contains(Nothing) Then Throw New ArgumentException("Invalid columns.", NameOf(columns))`
- `If row Is Nothing OrElse row.Count <> columnCopy.Count Then Throw New ArgumentException("Invalid frame row width.", NameOf(rows))`
- `If item.Contains(Nothing) Then Throw New ArgumentException("Frame cells cannot be null.", NameOf(rows))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`.

### `SpreadsheetCategoryValue` — F

[Declaration L131](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L131). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCategoryValue
Public Sub New(cell As SpreadsheetCellValue, Optional ignoreTextCase As Boolean = False)
Public ReadOnly Property Kind As SpreadsheetCellKind
Public ReadOnly Property NumberValue As Double
Public ReadOnly Property TextValue As String
Public ReadOnly Property BooleanValue As Boolean
Public ReadOnly Property Key As String
Public ReadOnly Property DisplayText As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If cell Is Nothing Then Throw New ArgumentNullException(NameOf(cell))`
- `If Double.IsNaN(cell.NumberValue) OrElse Double.IsInfinity(cell.NumberValue) Then Throw New ArgumentException("Non-finite category number.", NameOf(cell))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb`.

Tests: `AdvancedInputParserTests`, `RectangularCategoricalParserTests`.

### `SpreadsheetCategoricalObservation` — F

[Declaration L162](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L162). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCategoricalObservation
Public Sub New(level As SpreadsheetCategoryValue, selectedRow As Integer, selectedColumn As Integer, worksheetRow As Integer, worksheetColumn As Integer)
Public ReadOnly Property Level As SpreadsheetCategoryValue
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property SelectedColumn As Integer
Public ReadOnly Property WorksheetRow As Integer
Public ReadOnly Property WorksheetColumn As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If level Is Nothing Then Throw New ArgumentNullException(NameOf(level))`
- `If selectedRow < 1 OrElse selectedColumn < 1 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRow))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

Tests: `RectangularCategoricalParserTests`.

### `SpreadsheetCategoricalVectorInput` — F

[Declaration L180](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L180). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCategoricalVectorInput
Inherits SpreadsheetAnalysisInput
Public Sub New(name As String, observations As IEnumerable(Of SpreadsheetCategoricalObservation), levels As IEnumerable(Of SpreadsheetCategoryValue))
Public ReadOnly Property Name As String
Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetCategoricalObservation)
Public ReadOnly Property Levels As IReadOnlyList(Of SpreadsheetCategoryValue)
Public ReadOnly Property Count As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))`
- `If levels Is Nothing Then Throw New ArgumentNullException(NameOf(levels))`
- `If copied.Contains(Nothing) OrElse levelCopy.Contains(Nothing) Then Throw New ArgumentException("Null observations or levels.")`

Direct source consumers: `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

Tests: `RectangularCategoricalParserTests`.

### `SpreadsheetContingencyTableInput` — F

[Declaration L214](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L214). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetContingencyTableInput
Inherits SpreadsheetAnalysisInput
Public Sub New(rowLevels As IEnumerable(Of SpreadsheetCategoryValue), columnLevels As IEnumerable(Of SpreadsheetCategoryValue), counts As Long()())
Public ReadOnly Property RowLevels As IReadOnlyList(Of SpreadsheetCategoryValue)
Public ReadOnly Property ColumnLevels As IReadOnlyList(Of SpreadsheetCategoryValue)
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property GrandTotal As Long
Public Function Count(rowIndex As Integer, columnIndex As Integer) As Long
Public ReadOnly Property Counts As Long()()
Public ReadOnly Property RowTotals As Long()
Public ReadOnly Property ColumnTotals As Long()
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If rowLevels Is Nothing Then Throw New ArgumentNullException(NameOf(rowLevels))`
- `If columnLevels Is Nothing Then Throw New ArgumentNullException(NameOf(columnLevels))`
- `If counts Is Nothing Then Throw New ArgumentNullException(NameOf(counts))`
- `If rr.Count = 0 OrElse cc.Count = 0 OrElse rr.Contains(Nothing) OrElse cc.Contains(Nothing) OrElse counts.Length <> rr.Count Then Throw New ArgumentException("Contingency matrix dimensions are inconsistent.", NameOf(counts))`
- `If counts(r) Is Nothing OrElse counts(r).Length <> cc.Count Then Throw New ArgumentException("Contingency matrix is not rectangular.", NameOf(counts))`
- `If count < 0 Then Throw New ArgumentOutOfRangeException(NameOf(counts), "Negative counts are invalid.")`

Direct source consumers: `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

### `SpreadsheetDataFrameParseOptions` — F

[Declaration L306](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L306). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetDataFrameParseOptions
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
Public Property RejectDuplicateNames As Boolean = True
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`, `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

Tests: `RectangularCategoricalParserTests`.

### `SpreadsheetCategoricalVectorParseOptions` — F

[Declaration L311](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L311). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCategoricalVectorParseOptions
Public Property Orientation As SpreadsheetVectorOrientation = SpreadsheetVectorOrientation.Column
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
Public Property ColumnIndex As Integer = 0
Public Property RowIndex As Integer = 0
Public Property MinimumObservations As Integer = 1
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Skip
Public Property IgnoreTextCase As Boolean = False
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

Tests: `RectangularCategoricalParserTests`.

### `SpreadsheetContingencyCountParseOptions` — F

[Declaration L322](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L322). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetContingencyCountParseOptions
Public Property HasColumnHeaders As Boolean = True
Public Property HasRowLabels As Boolean = True
Public Property MinimumRows As Integer = 2
Public Property MinimumColumns As Integer = 2
Public Property TreatMissingAsZero As Boolean = False
Public Property IgnoreTextCase As Boolean = False
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

Tests: `RectangularCategoricalParserTests`.

### `SpreadsheetContingencyPairsParseOptions` — F

[Declaration L332](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb#L332). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetContingencyPairsParseOptions
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
Public Property FirstColumnIndex As Integer = 0
Public Property SecondColumnIndex As Integer = 1
Public Property MinimumRows As Integer = 2
Public Property MinimumColumns As Integer = 2
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
Public Property IgnoreTextCase As Boolean = False
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`.

Tests: `RectangularCategoricalParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: LongitudinalParserTests, RectangularCategoricalParserTests.

### `RectangularCategoricalParsers` — I

[Declaration L11](../../BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class RectangularCategoricalParsers
Private Sub New()
Public Shared Function ParseDataFrame(input As SpreadsheetRangeData, Optional options As SpreadsheetDataFrameParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetRectangularDataFrameInput)
Public Shared Function ParseCategoricalVector(input As SpreadsheetRangeData, Optional options As SpreadsheetCategoricalVectorParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetCategoricalVectorInput)
Public Shared Function ParseContingencyCounts(input As SpreadsheetRangeData, Optional options As SpreadsheetContingencyCountParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetContingencyTableInput)
Public Shared Function ParseContingencyPairs(input As SpreadsheetRangeData, Optional options As SpreadsheetContingencyPairsParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetContingencyTableInput)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`, `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

Tests: `LongitudinalParserTests`, `RectangularCategoricalParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/SpreadsheetCoreTableBridge.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: AcquisitionSnapshotTests, CellValuePolicyTests.

### `SpreadsheetCoreTableBridge` — I

[Declaration L14](../../BESHStat.Excel.Common/src/Input/SpreadsheetCoreTableBridge.vb#L14). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetCoreTableBridge
Private Sub New()
Public Shared Function ToCoreDataTable(input As SpreadsheetRangeData, columnNames As String(), Optional options As SpreadsheetCellPolicyOptions = Nothing) As Global.BESHStatNG.CoreDataTable
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `AcquisitionSnapshotTests`, `CellValuePolicyTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/SpreadsheetValidation.vb`

Owner/consumers: Diagnostics-first parsers and host error display.

Issue code required; coordinates nonnegative. Selected positions are one-based in full selection, worksheet positions one-based or zero unknown. Null message/path becomes empty. Report copies nonnull issues; exception retains report. No report wire version.

Evidence/fixtures: SpreadsheetGroupedValidationTests; TypedNumericRangeParserTests.

### `SpreadsheetValidationSeverity` — F

[Declaration L11](../../BESHStat.Excel.Common/src/Input/SpreadsheetValidation.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetValidationSeverity
Information = 0
Warning = 1
[Error] = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`, `BESHStat.Excel.Common/src/Input/InputSizeLimits.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `ModelInputCompilerTests`, `SpreadsheetGroupedValidationTests`.

### `SpreadsheetValidationIssue` — F

[Declaration L22](../../BESHStat.Excel.Common/src/Input/SpreadsheetValidation.vb#L22). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetValidationIssue
Public Sub New(code As String, severity As SpreadsheetValidationSeverity, message As String, fieldPath As String, Optional selectedRow As Integer = 0, Optional selectedColumn As Integer = 0, Optional worksheetRow As Integer = 0, Optional worksheetColumn As Integer = 0)
Public ReadOnly Property Code As String
Public ReadOnly Property Severity As SpreadsheetValidationSeverity
Public ReadOnly Property Message As String
Public ReadOnly Property FieldPath As String
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property SelectedColumn As Integer
Public ReadOnly Property WorksheetRow As Integer
Public ReadOnly Property WorksheetColumn As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("An issue code is required.", NameOf(code))`
- `If selectedRow < 0 OrElse selectedColumn < 0 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRow), "Validation coordinates must be nonnegative.")`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`, `BESHStat.Excel.Common/src/Input/InputSizeLimits.vb`.

Tests: `AdvancedInputParserTests`, `InputSizeLimitsTests`, `LongitudinalParserTests`, `ModelInputCompilerTests`.

### `SpreadsheetValidationReport` — F

[Declaration L52](../../BESHStat.Excel.Common/src/Input/SpreadsheetValidation.vb#L52). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetValidationReport
Public Sub New(issues As IEnumerable(Of SpreadsheetValidationIssue))
Public ReadOnly Property Issues As IReadOnlyList(Of SpreadsheetValidationIssue)
Public ReadOnly Property HasErrors As Boolean
Public ReadOnly Property HasWarnings As Boolean
Public ReadOnly Property IsValid As Boolean
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If issues Is Nothing Then Throw New ArgumentNullException(NameOf(issues))`
- `If item Is Nothing Then Throw New ArgumentException("Null validation issues are not allowed.", NameOf(issues))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/ConfigurableGroupedNumericParser.vb`, `BESHStat.Excel.Common/src/Input/InputSizeLimits.vb`.

Tests: `SpreadsheetGroupedValidationTests`.

### `SpreadsheetInputValidationException` — F

[Declaration L88](../../BESHStat.Excel.Common/src/Input/SpreadsheetValidation.vb#L88). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetInputValidationException
Inherits ArgumentException
Public Sub New(report As SpreadsheetValidationReport)
Public ReadOnly Property Report As SpreadsheetValidationReport
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If report Is Nothing Then Throw New ArgumentNullException(NameOf(report))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb`, `BESHStat.Excel.Common/src/Input/InputSizeLimits.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `InputSizeLimitsTests`, `SpreadsheetGroupedValidationTests`, `AcquisitionAdapterTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb`

Owner/consumers: TypedNumericRangeParsers; future typed handlers.

Observation requires finite Double and positive selected coordinates, absolute coordinates nonnegative. Scalar/pair require nonnull inputs. Vector/pair collections copied read-only; Values arrays are fresh. Pair alignment preserves common source row. Parse result requires report and forbids data when report has errors. Options mutable; indices zero-based.

Evidence/fixtures: TypedNumericRangeParserTests.

### `SpreadsheetNumericObservation` — F

[Declaration L11](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetNumericObservation
Public Sub New(value As Double, selectedRow As Integer, selectedColumn As Integer, worksheetRow As Integer, worksheetColumn As Integer)
Public ReadOnly Property Value As Double
Public ReadOnly Property SelectedRow As Integer
Public ReadOnly Property SelectedColumn As Integer
Public ReadOnly Property WorksheetRow As Integer
Public ReadOnly Property WorksheetColumn As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Throw New ArgumentOutOfRangeException(NameOf(value))`
- `If selectedRow < 1 OrElse selectedColumn < 1 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRow), "Source coordinates are invalid.")`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb`.

### `SpreadsheetScalarNumericInput` — F

[Declaration L33](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L33). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetScalarNumericInput
Inherits SpreadsheetAnalysisInput
Public Sub New(observation As SpreadsheetNumericObservation)
Public ReadOnly Property Observation As SpreadsheetNumericObservation
Public ReadOnly Property Value As Double
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If observation Is Nothing Then Throw New ArgumentNullException(NameOf(observation))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

### `SpreadsheetVectorNumericInput` — F

[Declaration L50](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L50). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetVectorNumericInput
Inherits SpreadsheetAnalysisInput
Public Sub New(name As String, observations As IEnumerable(Of SpreadsheetNumericObservation))
Public ReadOnly Property Name As String
Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetNumericObservation)
Public ReadOnly Property Count As Integer
Public ReadOnly Property Values As Double()
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))`
- `If observation Is Nothing Then Throw New ArgumentException("Null observations are unsupported.", NameOf(observations))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

### `SpreadsheetNumericPair` — F

[Declaration L89](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L89). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetNumericPair
Public Sub New(first As SpreadsheetNumericObservation, second As SpreadsheetNumericObservation)
Public ReadOnly Property First As SpreadsheetNumericObservation
Public ReadOnly Property Second As SpreadsheetNumericObservation
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If first Is Nothing Then Throw New ArgumentNullException(NameOf(first))`
- `If second Is Nothing Then Throw New ArgumentNullException(NameOf(second))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

### `SpreadsheetPairedNumericInput` — F

[Declaration L102](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L102). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetPairedNumericInput
Inherits SpreadsheetAnalysisInput
Public Sub New(firstName As String, secondName As String, pairs As IEnumerable(Of SpreadsheetNumericPair))
Public ReadOnly Property FirstName As String
Public ReadOnly Property SecondName As String
Public ReadOnly Property Pairs As IReadOnlyList(Of SpreadsheetNumericPair)
Public ReadOnly Property Count As Integer
Public ReadOnly Property FirstValues As Double()
Public ReadOnly Property SecondValues As Double()
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If pairs Is Nothing Then Throw New ArgumentNullException(NameOf(pairs))`
- `If item Is Nothing Then Throw New ArgumentException("Null pairs are unsupported.", NameOf(pairs))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

### `SpreadsheetTypedParseResult` — F

[Declaration L154](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L154). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetTypedParseResult(Of T As SpreadsheetAnalysisInput)
Public Sub New(data As T, validation As SpreadsheetValidationReport)
Public ReadOnly Property Data As T
Public ReadOnly Property Validation As SpreadsheetValidationReport
Public ReadOnly Property Success As Boolean
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If validation Is Nothing Then Throw New ArgumentNullException(NameOf(validation))`
- `If validation.HasErrors AndAlso data IsNot Nothing Then Throw New ArgumentException("Invalid parses cannot carry typed data.", NameOf(data))`

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalParsers.vb`, `BESHStat.Excel.Common/src/Input/ModelInputCompiler.vb`.

Tests: `AdvancedInputParserTests`, `LongitudinalParserTests`, `ModelInputCompilerTests`, `RectangularCategoricalParserTests`.

### `SpreadsheetVectorOrientation` — F

[Declaration L172](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L172). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetVectorOrientation
Auto = 0
Column = 1
Row = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Input/RectangularCategoricalInputs.vb`, `BESHStat.Excel.Common/src/Input/RectangularCategoricalParsers.vb`, `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

Tests: `RectangularCategoricalParserTests`, `TypedNumericRangeParserTests`.

### `SpreadsheetPairOrientation` — F

[Declaration L178](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L178). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetPairOrientation
TwoColumns = 0
TwoRows = 1
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

Tests: `TypedNumericRangeParserTests`.

### `SpreadsheetIncompleteObservationPolicy` — F

[Declaration L183](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L183). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum SpreadsheetIncompleteObservationPolicy
Reject = 0
Skip = 1
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Input/AdvancedInputParsers.vb`, `BESHStat.Excel.Common/src/Input/AdvancedInputs.vb`, `BESHStat.Excel.Common/src/Input/LongitudinalInputs.vb`.

Tests: `AdvancedInputParserTests`, `LongitudinalParserTests`, `ModelInputCompilerTests`, `RectangularCategoricalParserTests`.

### `SpreadsheetVectorParseOptions` — F

[Declaration L188](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L188). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetVectorParseOptions
Public Property Orientation As SpreadsheetVectorOrientation = SpreadsheetVectorOrientation.Auto
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
Public Property ColumnIndex As Integer = 0
Public Property RowIndex As Integer = 0
Public Property MinimumObservations As Integer = 1
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Skip
Public Property WarnOnSkippedObservations As Boolean = False
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

Tests: `TypedNumericRangeParserTests`.

### `SpreadsheetPairedParseOptions` — F

[Declaration L199](../../BESHStat.Excel.Common/src/Input/TypedNumericInputs.vb#L199). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SpreadsheetPairedParseOptions
Public Property Orientation As SpreadsheetPairOrientation = SpreadsheetPairOrientation.TwoColumns
Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
Public Property FirstIndex As Integer = 0
Public Property SecondIndex As Integer = 1
Public Property MinimumPairs As Integer = 1
Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
Public Property WarnOnSkippedPairs As Boolean = True
Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`.

Tests: `TypedNumericRangeParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: TypedNumericRangeParserTests.

### `TypedNumericRangeParsers` — I

[Declaration L15](../../BESHStat.Excel.Common/src/Input/TypedNumericRangeParsers.vb#L15). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class TypedNumericRangeParsers
Private Sub New()
Public Shared Function ParseScalar(input As SpreadsheetRangeData, Optional cellPolicy As SpreadsheetCellPolicyOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetScalarNumericInput)
Public Shared Function ParseVector(input As SpreadsheetRangeData, Optional options As SpreadsheetVectorParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetVectorNumericInput)
Public Shared Function ParsePaired(input As SpreadsheetRangeData, Optional options As SpreadsheetPairedParseOptions = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetPairedNumericInput)
Public Shared Function ParseGrouped(input As SpreadsheetRangeData, Optional options As SpreadsheetGroupedParseOptions = Nothing) As SpreadsheetTypedParseResult(Of GroupedNumericData)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `TypedNumericRangeParserTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`

Owner/consumers: Office composite adapter; Windows recording/insertion seam.

Separate opt-in family; source references exactly once in source order, unique ordinal item IDs, notes/links may interleave. Dimensions explicit, positive visible or nonspatial; table dimensions from matrix. Bounds one-based inclusive; SourceIndex/Index zero-based. Snapshots/read-only lists. HTTPS validation, literal inactive links and chart placeholders only; no renderer/active navigation promise.

Evidence/fixtures: CompositeOutputTests; composite-c10/secondary/secondary-link fixtures.

### `CompositeOutputKind` — F

[Declaration L10](../../BESHStat.Excel.Common/src/Output/CompositeOutput.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum CompositeOutputKind
Table = 1
Diagnostic = 2
PlotPlaceholder = 3
Note = 4
Link = 5
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`.

### `CompositeOutputItem` — F

[Declaration L19](../../BESHStat.Excel.Common/src/Output/CompositeOutput.vb#L19). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class CompositeOutputItem
Private Sub New(id As String, artifactId As String, kind As CompositeOutputKind, rows As Integer, columns As Integer, text As String, relatedId As String, url As String)
Public ReadOnly Property Id As String
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property Kind As CompositeOutputKind
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property Text As String
Public ReadOnly Property RelatedArtifactId As String
Public ReadOnly Property LinkTarget As String
Public Shared Function FromArtifact(artifact As OutputArtifact, Optional rows As Integer = 0, Optional columns As Integer = 0) As CompositeOutputItem
Public Shared Function Note(id As String, text As String, rows As Integer, columns As Integer, Optional relatedArtifactId As String = Nothing) As CompositeOutputItem
Public Shared Function Link(id As String, displayText As String, target As String, rows As Integer, columns As Integer, Optional relatedArtifactId As String = Nothing) As CompositeOutputItem
Public Shared Sub ValidateLink(target As String)
Public Shared Sub Check(value As String, allowEmpty As Boolean)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`.

Tests: `CompositeOutputTests`, `CompositeOutputPlanTests`.

### `CompositeOutputPlacement` — F

[Declaration L119](../../BESHStat.Excel.Common/src/Output/CompositeOutput.vb#L119). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class CompositeOutputPlacement
Friend Sub New(item As CompositeOutputItem, index As Integer, sourceIndex As Integer?, bounds As OutputRectangle)
Public ReadOnly Property Item As CompositeOutputItem
Public ReadOnly Property Index As Integer
Public ReadOnly Property SourceIndex As Integer?
Public ReadOnly Property Bounds As OutputRectangle
Public ReadOnly Property State As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`.

Tests: `CompositeOutputTests`.

### `CompositeOutputPlan` — F

[Declaration L136](../../BESHStat.Excel.Common/src/Output/CompositeOutput.vb#L136). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class CompositeOutputPlan
Friend Sub New(id As String, source As OutputDocument, decision As OutputTargetDecision, options As OutputLayoutOptions, items As List(Of CompositeOutputPlacement), used As OutputRectangle, reservations As List(Of OutputRectangle))
Public ReadOnly Property DocumentId As String
Public ReadOnly Property SourceDocument As OutputDocument
Public ReadOnly Property Target As OutputTargetDecision
Public ReadOnly Property Options As OutputLayoutOptions
Public ReadOnly Property Items As IReadOnlyList(Of CompositeOutputPlacement)
Public ReadOnly Property UsedBounds As OutputRectangle
Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
Public ReadOnly Property OccupiedRectangles As IReadOnlyList(Of OutputRectangle)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`, `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`.

Tests: `CompositeOutputTests`, `CompositeOutputPlanTests`.

### `CompositeOutputPlanner` — I

[Declaration L155](../../BESHStat.Excel.Common/src/Output/CompositeOutput.vb#L155). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class CompositeOutputPlanner
Public Shared Function Create(documentId As String, document As OutputDocument, items As IEnumerable(Of CompositeOutputItem), decision As OutputTargetDecision, Optional options As OutputLayoutOptions = Nothing, Optional reservations As IEnumerable(Of OutputRectangle) = Nothing) As CompositeOutputPlan
Public Shared Function NextBounds(startRow As Integer, startColumn As Integer, previousEnd As Long, rows As Integer, columns As Integer, limits As OutputLayoutOptions, reservations As IEnumerable(Of OutputRectangle)) As OutputRectangle
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`.

Tests: `CompositeOutputTests`, `CompositeOutputPlanTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`

Owner/consumers: Existing compiled analysis/host callers.

Constructor/factory structural guards below; implementation methods are in-process only. Source algorithms/typed registries remain authoritative; no serializer or durable identity is implied.

Evidence/fixtures: CompositeOutputTests, WriteResultTests, CompositeOutputPlanTests.

### `CompositeOutputExample` — I

[Declaration L6](../../BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb#L6). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class CompositeOutputExample
Public Shared Function Create(decision As OutputTargetDecision) As CompositeOutputPlan
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `CompositeOutputTests`, `WriteResultTests`, `CompositeOutputPlanTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`

Owner/consumers: Home synthetic demo -> JS writeCompositeOutputPlan.

HOST WIRE: independent composite wire v1, nests unchanged table wire v1. NewWorksheet only; complete ordered item metadata/state/bounds, exact tables through existing adapter. Read-only graph. Null optional fields retained in actual camelCase serialization. Strict JS field set; no implied extension tolerance.

Evidence/fixtures: CompositeOutputTests; composite-output three fixtures; JS compositeOutputWriter.

### `OfficeCompositeOutputItem` — H

[Declaration L10](../../BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OfficeCompositeOutputItem
Friend Sub New(placement As CompositeOutputPlacement, source As OutputDocument)
Public ReadOnly Property Id As String
Public ReadOnly Property Kind As String
Public ReadOnly Property Index As Integer
Public ReadOnly Property SourceIndex As Integer?
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property RelatedArtifactId As String
Public ReadOnly Property Bounds As OutputRectangle
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property Text As String
Public ReadOnly Property LinkTarget As String
Public ReadOnly Property State As String
Public ReadOnly Property Caption As String
Public ReadOnly Property Severity As String
Public ReadOnly Property Code As String
Public ReadOnly Property FieldPath As String
Public ReadOnly Property Message As String
Public ReadOnly Property ReferenceKey As String
Public ReadOnly Property Purpose As String
Public ReadOnly Property Description As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

### `OfficeCompositeOutputPlan` — H

[Declaration L52](../../BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb#L52). **Version/exposure:** ReadOnly SchemaVersion=1; independent prototype adapter wire; camelCase with explicit null fields.

```vb
Public NotInheritable Class OfficeCompositeOutputPlan
Friend Sub New(plan As CompositeOutputPlan, tablePlan As OfficeOutputPlan)
Public ReadOnly Property SchemaVersion As Integer = 1
Public ReadOnly Property DocumentId As String
Public ReadOnly Property Target As OfficeOutputTarget
Public ReadOnly Property Items As IReadOnlyList(Of OfficeCompositeOutputItem)
Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
Public ReadOnly Property UsedBounds As OutputRectangle
Public ReadOnly Property TablePlan As OfficeOutputPlan
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `CompositeOutputTests`.

### `OfficeCompositeOutputPlanAdapter` — H

[Declaration L70](../../BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb#L70). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OfficeCompositeOutputPlanAdapter
Public Shared Function Prepare(plan As CompositeOutputPlan, semantics As OutputSemanticPlan) As OfficeCompositeOutputPlan
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `CompositeOutputTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`

Owner/consumers: Home.razor -> officeBridge.js writeOutputPlan.

HOST WIRE: source comment explicitly separates prototype wire v1 from Common v1. Read-only camelCase DTO projection with explicit null union fields; source document reference validated. Exact Decimal->binary64 only; rejects inexact integers/Decimal, errors and formulas. Rows/columns local zero-based; bounds one-based. Strict JS field sets reject unknown fields. SchemaVersion=1 on plan, not values/cells.

Evidence/fixtures: OfficeOutputPlanAdapterTests; output-plans eight fixtures; JS outputPlanWriter.

### `OfficeOutputValue` — H

[Declaration L13](../../BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb#L13). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OfficeOutputValue
Private Sub New(kind As String, Optional number As Double? = Nothing, Optional text As String = Nothing, Optional booleanValue As Boolean? = Nothing)
Public ReadOnly Property Kind As String
Public ReadOnly Property NumberValue As Double?
Public ReadOnly Property TextValue As String
Public ReadOnly Property BooleanValue As Boolean?
Public Shared Function FromCell(cell As OutputCell) As OfficeOutputValue
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`.

### `OfficeOutputCell` — H

[Declaration L74](../../BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb#L74). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OfficeOutputCell
Friend Sub New(style As OutputCellStyle, numberFormat As String)
Public ReadOnly Property Row As Integer
Public ReadOnly Property Column As Integer
Public ReadOnly Property Value As OfficeOutputValue
Public ReadOnly Property Role As String
Public ReadOnly Property NumberFormat As String
Public ReadOnly Property IsPValue As Boolean
Public ReadOnly Property IsSignificant As Boolean
Public ReadOnly Property StrongEmphasis As Boolean
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `OfficeOutputPlanAdapterTests`.

### `OfficeOutputTarget` — H

[Declaration L95](../../BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb#L95). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OfficeOutputTarget
Friend Sub New(layout As OutputLayoutPlan)
Public ReadOnly Property Destination As Integer
Public ReadOnly Property WorksheetName As String
Public ReadOnly Property StartRow As Integer
Public ReadOnly Property StartColumn As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`.

Tests: `OfficeOutputPlanAdapterTests`.

### `OfficeOutputTable` — H

[Declaration L108](../../BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb#L108). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OfficeOutputTable
Friend Sub New(block As OutputBlockPlacement, ordinal As Integer, cells As IEnumerable(Of OfficeOutputCell), autoFit As Boolean)
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property DocumentIndex As Integer
Public ReadOnly Property TableOrdinal As Integer
Public ReadOnly Property Bounds As OutputRectangle
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property Cells As IReadOnlyList(Of OfficeOutputCell)
Public ReadOnly Property AutoFit As Boolean
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `OfficeOutputPlanAdapterTests`.

### `OfficeOutputPlan` — H

[Declaration L129](../../BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb#L129). **Version/exposure:** ReadOnly SchemaVersion=1; independent prototype adapter wire; camelCase with explicit null fields.

```vb
Public NotInheritable Class OfficeOutputPlan
Friend Sub New(layout As OutputLayoutPlan, sourceTableCount As Integer, tables As IEnumerable(Of OfficeOutputTable), format As String)
Public ReadOnly Property SchemaVersion As Integer = 1
Public ReadOnly Property Target As OfficeOutputTarget
Public ReadOnly Property ArtifactCount As Integer
Public ReadOnly Property SourceTableCount As Integer
Public ReadOnly Property TableCount As Integer
Public ReadOnly Property Tables As IReadOnlyList(Of OfficeOutputTable)
Public ReadOnly Property UsedBounds As OutputRectangle
Public ReadOnly Property PvalueNumberFormat As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`.

Tests: `OfficeOutputPlanAdapterTests`.

### `OfficeOutputPlanAdapter` — H

[Declaration L149](../../BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb#L149). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OfficeOutputPlanAdapter
Private Sub New()
Public Shared Function Prepare(document As OutputDocument, semantics As OutputSemanticPlan, layout As OutputLayoutPlan, Optional autoFit As Boolean = True) As OfficeOutputPlan
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputTargetPlannerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/OutputCell.vb`

Owner/consumers: OutputSemanticPlanner; OfficeOutputValue.

Closed scalar union. Finite Binary64 or exact Decimal (all integer kinds convert exactly); Nothing blank, empty String text. Nonfinite raw floats map to portable error codes. Explicit error preserves supplied metadata; explicit trusted formula never inferred from text. Exact probability/alpha comparison uses rational integers; no numerical result rewriting. CellAt is zero-based.

Evidence/fixtures: OutputCellTests; six OutputSemantics fixtures.

### `OutputCellKind` — F

[Declaration L10](../../BESHStat.Excel.Common/src/Output/OutputCell.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputCellKind
Blank = 0
Number = 1
Text = 2
BooleanValue = 3
[Error] = 4
Formula = 5
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `OutputCellTests`, `PValuePresentationPlanTests`.

### `OutputNumberKind` — F

[Declaration L19](../../BESHStat.Excel.Common/src/Output/OutputCell.vb#L19). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputNumberKind
Binary64 = 1
ExactDecimal = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `OutputCellTests`, `PValuePresentationPlanTests`.

### `OutputErrorKind` — F

[Declaration L24](../../BESHStat.Excel.Common/src/Output/OutputCell.vb#L24). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputErrorKind
Unknown = 0
NullIntersection = 1
DivisionByZero = 2
InvalidValue = 3
InvalidReference = 4
UnknownName = 5
InvalidNumber = 6
NotAvailable = 7
GettingData = 8
NonFinite = 9
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Tests: `GateBOutputContractCompatibilityTests`, `OutputCellTests`.

### `OutputCell` — F

[Declaration L42](../../BESHStat.Excel.Common/src/Output/OutputCell.vb#L42). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputCell
Private Sub New(kind As OutputCellKind, Optional numberKind As OutputNumberKind? = Nothing, Optional binaryValue As Double? = Nothing, Optional decimalValue As Decimal? = Nothing, Optional text As String = Nothing, Optional booleanValue As Boolean? = Nothing, Optional errorKind As OutputErrorKind? = Nothing, Optional errorCode As String = Nothing, Optional originalError As String = Nothing, Optional nativeCode As Integer? = Nothing, Optional safeDetail As String = Nothing, Optional formulaExpression As String = Nothing, Optional formulaLanguage As String = Nothing)
Public ReadOnly Property Kind As OutputCellKind
Public ReadOnly Property NumberKind As OutputNumberKind?
Public ReadOnly Property BinaryValue As Double?
Public ReadOnly Property DecimalValue As Decimal?
Public ReadOnly Property TextValue As String
Public ReadOnly Property BooleanValue As Boolean?
Public ReadOnly Property ErrorKind As OutputErrorKind?
Public ReadOnly Property ErrorCode As String
Public ReadOnly Property OriginalError As String
Public ReadOnly Property NativeErrorCode As Integer?
Public ReadOnly Property SafeErrorDetail As String
Public ReadOnly Property FormulaExpression As String
Public ReadOnly Property FormulaLanguage As String
Public Shared Function BlankCell() As OutputCell
Public Shared Function NumberCell(value As Double) As OutputCell
Public Shared Function DecimalCell(value As Decimal) As OutputCell
Public Shared Function TextCell(value As String) As OutputCell
Public Shared Function BooleanCell(value As Boolean) As OutputCell
Public Shared Function SpreadsheetError(token As String, Optional nativeCode As Integer? = Nothing, Optional safeDetail As String = Nothing) As OutputCell
Public Shared Function TrustedFormula(expression As String, language As String) As OutputCell
Public Shared Function FromRaw(value As Object) As OutputCell
Public Function TryGetProbability(ByRef value As Double) As Boolean
Public Function IsSignificantAt(alpha As Double) As Boolean
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`, `BESHStat.Excel.Common/src/Output/Styles.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `OfficeOutputPlanAdapterTests`, `OutputCellTests`, `OutputStylesTests`.

### `OutputCellTable` — F

[Declaration L220](../../BESHStat.Excel.Common/src/Output/OutputCell.vb#L220). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputCellTable
Public Sub New(artifact As OutputTableArtifact)
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public Function CellAt(row As Integer, column As Integer) As OutputCell
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If artifact Is Nothing Then Throw New ArgumentNullException(NameOf(artifact))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `OutputCellTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/OutputDocument.vb`

Owner/consumers: Semantic/layout planners; both writers through adapters.

Closed ordered immutable presentation family; ordinal document-local unique nonblank IDs. Tables validate zero array lower bounds and section dimensions, whitelist immutable CLR scalars, copy matrix/annotation lists on ingress/access; raw nonfinite retained. Invalid/duplicate p-value annotations retained exactly. Titles/footers are matrix cells; Caption adds no row. No production serializer.

Evidence/fixtures: OutputDocumentTests; Stage304 five goldens.

### `OutputArtifactKind` — F

[Declaration L10](../../BESHStat.Excel.Common/src/Output/OutputDocument.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputArtifactKind
Table = 1
Diagnostic = 2
Plot = 3
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb`, `BESHStat.Excel.Common/src/Output/Styles.vb`.

Tests: `CompositeOutputTests`, `GateBOutputContractCompatibilityTests`, `OutputDocumentTests`, `OutputLayoutPlannerTests`.

### `OutputDiagnosticSeverity` — F

[Declaration L16](../../BESHStat.Excel.Common/src/Output/OutputDocument.vb#L16). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputDiagnosticSeverity
Warning = 1
Fault = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `CompositeOutputTests`, `GateBOutputContractCompatibilityTests`, `OfficeOutputPlanAdapterTests`, `OutputDocumentTests`.

### `OutputArtifact` — F

[Declaration L22](../../BESHStat.Excel.Common/src/Output/OutputDocument.vb#L22). **Version/exposure:** No declared version; no wire promise.

```vb
Public MustInherit Class OutputArtifact
Friend Sub New(id As String, kind As OutputArtifactKind, caption As String)
Public ReadOnly Property Id As String
Public ReadOnly Property Kind As OutputArtifactKind
Public ReadOnly Property Caption As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(id) Then Throw New ArgumentException("Artifact ID is required.", NameOf(id))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `CompositeOutputTests`, `OutputDocumentTests`, `OutputLayoutPlannerTests`, `OutputStylesTests`.

### `OutputTableArtifact` — F

[Declaration L41](../../BESHStat.Excel.Common/src/Output/OutputDocument.vb#L41). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputTableArtifact
Inherits OutputArtifact
Public Sub New(id As String, model As Global.BESHStatNG.ResultTableOutputModel, Optional caption As String = Nothing)
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property Model As Global.BESHStatNG.ResultTableOutputModel
Public Shared Function FromTable(id As String, table As Global.BESHStatNG.ResultTable, Optional caption As String = Nothing) As OutputTableArtifact
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))`
- `If model.Values Is Nothing Then Throw New ArgumentException("Table values are required.", NameOf(model))`
- `If model.Values.GetLowerBound(0) <> 0 OrElse model.Values.GetLowerBound(1) <> 0 Then Throw New ArgumentException("Table values must have zero-based array bounds.", NameOf(model))`
- `If model.TitleRows < 0 OrElse model.HeaderTopRows < 0 OrElse model.FooterRows < 0 OrElse model.HeaderLeftColumns < 0 OrElse model.HeaderLeftColumns > model.ColumnCount OrElse CLng(model.TitleRows) + model.HeaderTopRows + model.FooterRows > model.RowCount Then Throw New ArgumentException("Table section counts exceed the matrix dimensions or are negative.", NameOf(model))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`, `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputCellTests`, `OutputDocumentTests`.

### `OutputDiagnosticArtifact` — F

[Declaration L116](../../BESHStat.Excel.Common/src/Output/OutputDocument.vb#L116). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputDiagnosticArtifact
Inherits OutputArtifact
Public Sub New(id As String, severity As OutputDiagnosticSeverity, code As String, message As String, Optional fieldPath As String = Nothing)
Public ReadOnly Property Severity As OutputDiagnosticSeverity
Public ReadOnly Property Code As String
Public ReadOnly Property Message As String
Public ReadOnly Property FieldPath As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If severity <> OutputDiagnosticSeverity.Warning AndAlso severity <> OutputDiagnosticSeverity.Fault Then Throw New ArgumentOutOfRangeException(NameOf(severity))`
- `If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("Diagnostic code is required.", NameOf(code))`
- `If String.IsNullOrWhiteSpace(message) Then Throw New ArgumentException("Diagnostic message is required.", NameOf(message))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`, `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputDocumentTests`, `OutputLayoutPlannerTests`.

### `OutputPlotArtifact` — F

[Declaration L143](../../BESHStat.Excel.Common/src/Output/OutputDocument.vb#L143). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputPlotArtifact
Inherits OutputArtifact
Public Sub New(id As String, referenceKey As String, purpose As String, Optional caption As String = Nothing, Optional description As String = Nothing)
Public ReadOnly Property ReferenceKey As String
Public ReadOnly Property Purpose As String
Public ReadOnly Property Description As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(referenceKey) Then Throw New ArgumentException("Plot reference key is required.", NameOf(referenceKey))`
- `If String.IsNullOrWhiteSpace(purpose) Then Throw New ArgumentException("Plot purpose is required.", NameOf(purpose))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`, `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputDocumentTests`, `OutputLayoutPlannerTests`.

### `OutputDocument` — F

[Declaration L166](../../BESHStat.Excel.Common/src/Output/OutputDocument.vb#L166). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputDocument
Public Sub New(artifacts As IEnumerable(Of OutputArtifact))
Public ReadOnly Property Artifacts As IReadOnlyList(Of OutputArtifact)
Public Shared Function FromTable(table As Global.BESHStatNG.ResultTable) As OutputDocument
Public Shared Function FromModel(model As Global.BESHStatNG.ResultTableOutputModel) As OutputDocument
Public Shared Function FromResult(result As SpreadsheetAnalysisResult) As OutputDocument
Public Shared Function FromAnalysis(source As AnalysisResultDocument, Optional trailingArtifacts As IEnumerable(Of OutputArtifact) = Nothing) As OutputDocument
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If artifacts Is Nothing Then Throw New ArgumentNullException(NameOf(artifacts))`
- `If artifact Is Nothing Then Throw New ArgumentException("Null output artifact.", NameOf(artifacts))`
- `If Not ids.Add(artifact.Id) Then Throw New ArgumentException("Duplicate output artifact ID: " & artifact.Id, NameOf(artifacts))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`, `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputDocumentTests`, `OutputLayoutPlannerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb`

Owner/consumers: Windows planned adapter; OfficeOutputPlanAdapter.

Positive-area one-based inclusive rectangles, immutable options/default gap=1. Ordered placements include nonspatial and zero-area entries without consuming rows/gaps. Index and nullable offsets zero-based. UsedBounds envelope includes holes, never a write permission. Reservations are logical constraints; collisions throw without shifting. Target values copied; names do not prove physical identity. No serializer/version.

Evidence/fixtures: OutputLayoutPlannerTests; OutputLayout seven references; C6 trace.

### `OutputRectangle` — F

[Declaration L10](../../BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputRectangle
Public Sub New(startRow As Integer, startColumn As Integer, endRow As Integer, endColumn As Integer)
Public ReadOnly Property StartRow As Integer
Public ReadOnly Property StartColumn As Integer
Public ReadOnly Property EndRow As Integer
Public ReadOnly Property EndColumn As Integer
Public Function Intersects(other As OutputRectangle) As Boolean
Public Function Contains(row As Integer, column As Integer) As Boolean
Public Function Contains(other As OutputRectangle) As Boolean
Public Overrides Function ToString() As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If startRow < 1 Then Throw New ArgumentOutOfRangeException(NameOf(startRow))`
- `If startColumn < 1 Then Throw New ArgumentOutOfRangeException(NameOf(startColumn))`
- `If endRow < startRow Then Throw New ArgumentOutOfRangeException(NameOf(endRow), "End row must include the start row.")`
- `If endColumn < startColumn Then Throw New ArgumentOutOfRangeException(NameOf(endColumn), "End column must include the start column.")`

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`.

Tests: `CompositeOutputTests`, `GateBOutputContractCompatibilityTests`, `OutputLayoutPlannerTests`, `WriteResultTests`.

### `OutputLayoutOptions` — F

[Declaration L48](../../BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb#L48). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputLayoutOptions
Public Sub New(Optional blankRowsBetweenTables As Integer = 1, Optional maxRows As Integer = 1048576, Optional maxColumns As Integer = 16384)
Public ReadOnly Property BlankRowsBetweenTables As Integer
Public ReadOnly Property MaxRows As Integer
Public ReadOnly Property MaxColumns As Integer
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If maxRows < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxRows), "Grid row limit must be positive.")`
- `If maxColumns < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxColumns), "Grid column limit must be positive.")`
- `If blankRowsBetweenTables < 0 OrElse blankRowsBetweenTables >= maxRows Then Throw New ArgumentOutOfRangeException(NameOf(blankRowsBetweenTables), "Gap must be nonnegative and smaller than the grid row limit.")`

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputLayoutPlannerTests`.

### `OutputBlockPlacement` — F

[Declaration L65](../../BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb#L65). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputBlockPlacement
Friend Sub New(artifact As OutputArtifact, index As Integer, rows As Integer, columns As Integer, bounds As OutputRectangle, targetRow As Integer, targetColumn As Integer, blankRowsBefore As Integer)
Public ReadOnly Property Artifact As OutputArtifact
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property Kind As OutputArtifactKind
Public ReadOnly Property Index As Integer
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property Bounds As OutputRectangle
Public ReadOnly Property IsPlaced As Boolean
Public ReadOnly Property RowOffset As Integer?
Public ReadOnly Property ColumnOffset As Integer?
Public ReadOnly Property BlankRowsBefore As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `OutputLayoutPlannerTests`.

### `OutputLayoutPlan` — F

[Declaration L113](../../BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb#L113). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputLayoutPlan
Friend Sub New(target As SpreadsheetOutputTarget, options As OutputLayoutOptions, placements As IEnumerable(Of OutputBlockPlacement), usedBounds As OutputRectangle, reservations As IEnumerable(Of OutputRectangle))
Public ReadOnly Property Destination As SpreadsheetOutputDestination
Public ReadOnly Property WorksheetName As String
Public ReadOnly Property StartRow As Integer
Public ReadOnly Property StartColumn As Integer
Public ReadOnly Property Options As OutputLayoutOptions
Public ReadOnly Property Placements As IReadOnlyList(Of OutputBlockPlacement)
Public ReadOnly Property UsedBounds As OutputRectangle
Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `OutputLayoutPlannerTests`.

### `OutputLayoutPlanner` — I

[Declaration L138](../../BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb#L138). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputLayoutPlanner
Private Sub New()
Public Shared Function Create(document As OutputDocument, Optional target As SpreadsheetOutputTarget = Nothing, Optional options As OutputLayoutOptions = Nothing, Optional occupiedRectangles As IEnumerable(Of OutputRectangle) = Nothing, Optional semanticPlan As OutputSemanticPlan = Nothing) As OutputLayoutPlan
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputLayoutPlannerTests`, `OutputTargetPlannerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`

Owner/consumers: OutputSemanticPlanner; Core display API.

Core-owned settings copied and normalized, no new display algorithm. Invalid alpha defaults 0.05 with diagnostic. Annotations one-based body-relative -> zero-based cells, invalids diagnosed, duplicates collapsed. Decimal primary values unchanged; preview alone projects to Double with explicit culture. Core settings and significance remain separate from historical Windows rendering.

Evidence/fixtures: PValuePresentationPlanTests; OutputStylesTests.

### `OutputPValuePolicy` — F

[Declaration L11](../../BESHStat.Excel.Common/src/Output/PValuePresentation.vb#L11). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputPValuePolicy
Public Sub New(settings As PValuePresentationSettings)
Public ReadOnly Property DecimalPlaces As Integer
Public ReadOnly Property SmallValueDisplay As PValueSmallValueDisplay
Public ReadOnly Property UseUpperBound As Boolean
Public ReadOnly Property LowerThreshold As Double
Public ReadOnly Property UpperThreshold As Double
Public Function Preview(value As Double, culture As CultureInfo) As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/Styles.vb`.

Tests: `OutputStylesTests`, `PValuePresentationPlanTests`.

### `OutputSemanticPlanner` — I

[Declaration L48](../../BESHStat.Excel.Common/src/Output/PValuePresentation.vb#L48). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputSemanticPlanner
Private Sub New()
Public Shared Function Create(table As OutputTableArtifact, settings As PValuePresentationSettings, alpha As Double, Optional previewCultureName As String = Nothing) As OutputSemanticPlan
Public Shared Function Create(document As OutputDocument, settings As PValuePresentationSettings, alpha As Double, Optional previewCultureName As String = Nothing) As OutputSemanticPlan
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputLayoutPlannerTests`, `OutputStylesTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb`

Owner/consumers: Legacy Windows formatter; deprecated JS exports.

Mutable legacy transport with defaults and no declared version. Factory copies scalar rows/annotation lists and target, calls EnsureDefaults on supplied target, normalizes NaN/inf to literal #N/A/#Pinf/#Ninf. Body addresses one-based, factory does not sanitize metadata. Not equivalent to lossless OutputCell union.

Evidence/fixtures: Stage304ContractSnapshotTests; Windows SharedOutputPlanTests.

### `ResultTableBodyCellAddress` — L

[Declaration L12](../../BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb#L12). **Version/exposure:** No declared version; legacy primitive serialized request representation. PascalCase observed in diagnostic tests; host interop naming is a separate adapter convention. Not the canonical lossless cell union.

```vb
Public Class ResultTableBodyCellAddress
Public Sub New()
Public Sub New(bodyRow As Integer, bodyColumn As Integer)
Public Property BodyRow As Integer
Public Property BodyColumn As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`.

### `ResultTableWriteRequest` — L

[Declaration L30](../../BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb#L30). **Version/exposure:** No declared version; legacy primitive serialized request representation. PascalCase observed in diagnostic tests; host interop naming is a separate adapter convention. Not the canonical lossless cell union.

```vb
Public Class ResultTableWriteRequest
Public Sub New()
Public Property Values As List(Of List(Of Object))
Public Property HeaderTopRows As Integer
Public Property HeaderLeftColumns As Integer
Public Property FooterRows As Integer
Public Property TitleRows As Integer
Public Property IsResultTable As Boolean
Public Property PvalueColumns As List(Of Integer)
Public Property PvalueCells As List(Of ResultTableBodyCellAddress)
Public Property PvalueNumberFormat As String
Public Property PvalueHighlightAlpha As Double
Public Property OutputTarget As SpreadsheetOutputTarget
Public Property AutoFit As Boolean
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `OutputDocumentTests`, `OutputLayoutPlannerTests`, `SharedExcelLayerTests`, `Stage304ContractSnapshotTests`.

### `ResultTableWriteRequestFactory` — L

[Declaration L56](../../BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb#L56). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class ResultTableWriteRequestFactory
Private Sub New()
Public Shared Function Create(model As Global.BESHStatNG.ResultTableOutputModel, pvalueSettings As PValuePresentationSettings, alpha As Double, outputTarget As SpreadsheetOutputTarget) As ResultTableWriteRequest
Public Shared Function CreateMany(models As IEnumerable(Of Global.BESHStatNG.ResultTableOutputModel), pvalueSettings As PValuePresentationSettings, alpha As Double, outputTarget As SpreadsheetOutputTarget) As List(Of ResultTableWriteRequest)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `OutputDocumentTests`, `OutputLayoutPlannerTests`, `PValuePresentationPlanTests`, `SharedExcelLayerTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/Styles.vb`

Owner/consumers: OutputSemanticPlanner; Office adapter; prospective consumers.

Resolved cells immutable, row-major; LocalId=row:column within artifact, zero-based. Section roles take precedence title/top/footer/left/body; p-value overlay retains role. Body numeric probability <= alpha adds emphasis/accessibility. Semantic plan explicitly SchemaVersion=1 in memory; normalized JSON is test-only. PreviewCultureName Nothing disables, empty selects invariant.

Evidence/fixtures: OutputStylesTests; PValuePresentationPlanTests; OutputSemantics six fixtures.

### `OutputStyleRole` — F

[Declaration L9](../../BESHStat.Excel.Common/src/Output/Styles.vb#L9). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputStyleRole
Body = 0
Title = 1
TopColumnHeader = 2
LeftRowHeader = 3
Footer = 4
DiagnosticWarning = 5
DiagnosticFault = 6
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `OutputStylesTests`, `PValuePresentationPlanTests`, `SharedOutputPlanTests`.

### `OutputNumberFormatKind` — F

[Declaration L19](../../BESHStat.Excel.Common/src/Output/Styles.vb#L19). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputNumberFormatKind
General = 0
Numeric = 1
Percentage = 2
Scientific = 3
PValue = 4
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `OutputStylesTests`.

### `OutputNumberFormatIntent` — F

[Declaration L28](../../BESHStat.Excel.Common/src/Output/Styles.vb#L28). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputNumberFormatIntent
Public Sub New(kind As OutputNumberFormatKind, Optional decimalPlaces As Integer? = Nothing)
Public ReadOnly Property Kind As OutputNumberFormatKind
Public ReadOnly Property DecimalPlaces As Integer?
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If Not [Enum].IsDefined(GetType(OutputNumberFormatKind), kind) Then Throw New ArgumentOutOfRangeException(NameOf(kind))`
- `If kind = OutputNumberFormatKind.General Then If decimalPlaces.HasValue Then Throw New ArgumentException("General format has no precision.", NameOf(decimalPlaces))`
- `If decimalPlaces.HasValue Then Throw New ArgumentException("General format has no precision.", NameOf(decimalPlaces))`
- `If Not decimalPlaces.HasValue OrElse decimalPlaces.Value < 0 OrElse decimalPlaces.Value > 16 Then Throw New ArgumentOutOfRangeException(NameOf(decimalPlaces))`
- `If kind = OutputNumberFormatKind.PValue AndAlso decimalPlaces.Value < 2 Then Throw New ArgumentOutOfRangeException(NameOf(decimalPlaces))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `OutputStylesTests`.

### `OutputCellStyle` — F

[Declaration L51](../../BESHStat.Excel.Common/src/Output/Styles.vb#L51). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputCellStyle
Friend Sub New(artifactId As String, row As Integer, column As Integer, cell As OutputCell, role As OutputStyleRole, numberFormat As OutputNumberFormatIntent, isPValue As Boolean, significant As Boolean, preview As String)
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property LocalId As String
Public ReadOnly Property Row As Integer
Public ReadOnly Property Column As Integer
Public ReadOnly Property Cell As OutputCell
Public ReadOnly Property Role As OutputStyleRole
Public ReadOnly Property NumberFormat As OutputNumberFormatIntent
Public ReadOnly Property IsPValue As Boolean
Public ReadOnly Property IsSignificant As Boolean
Public ReadOnly Property StrongEmphasis As Boolean
Public ReadOnly Property AccessibilityIndicator As String
Public ReadOnly Property DisplayPreview As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `OutputLayoutPlannerTests`, `OutputStylesTests`, `PValuePresentationPlanTests`.

### `OutputArtifactStylePlan` — F

[Declaration L83](../../BESHStat.Excel.Common/src/Output/Styles.vb#L83). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputArtifactStylePlan
Friend Sub New(artifact As OutputArtifact, rows As Integer, columns As Integer, cells As IEnumerable(Of OutputCellStyle), diagnosticRole As OutputStyleRole?)
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property Kind As OutputArtifactKind
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property Cells As IReadOnlyList(Of OutputCellStyle)
Public ReadOnly Property DiagnosticRole As OutputStyleRole?
Public ReadOnly Property AccessibilityIndicator As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `OutputLayoutPlannerTests`, `OutputStylesTests`, `PValuePresentationPlanTests`.

### `OutputPlanDiagnostic` — F

[Declaration L105](../../BESHStat.Excel.Common/src/Output/Styles.vb#L105). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputPlanDiagnostic
Friend Sub New(artifactId As String, code As String, sourceIndex As Integer?)
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property Code As String
Public ReadOnly Property SourceIndex As Integer?
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`.

Tests: `OutputStylesTests`.

### `OutputSemanticPlan` — F

[Declaration L124](../../BESHStat.Excel.Common/src/Output/Styles.vb#L124). **Version/exposure:** ReadOnly SchemaVersion=1; Common in-memory semantic/outcome contract, not a generic JSON protocol.

```vb
Public NotInheritable Class OutputSemanticPlan
Friend Sub New(sourceDocument As OutputDocument, policy As OutputPValuePolicy, alpha As Double, previewCultureName As String, artifacts As IEnumerable(Of OutputArtifactStylePlan), diagnostics As IEnumerable(Of OutputPlanDiagnostic))
Public ReadOnly Property SchemaVersion As Integer = 1
Public ReadOnly Property Policy As OutputPValuePolicy
Public ReadOnly Property Alpha As Double
Public ReadOnly Property PreviewCultureName As String
Public ReadOnly Property Artifacts As IReadOnlyList(Of OutputArtifactStylePlan)
Public ReadOnly Property Diagnostics As IReadOnlyList(Of OutputPlanDiagnostic)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeCompositeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/OutputLayoutPlanner.vb`.

Tests: `OutputLayoutPlannerTests`, `OutputStylesTests`, `PValuePresentationPlanTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/TargetPlanner.vb`

Owner/consumers: Host preflight; both planned writers.

Facts immutable nullable observations, Nothing unverified. Known failure rejects; unknown optional facts retain legacy admission but never grant overwrite. Decision copies requested/resolved names/start/host; effective destination equals requested. ToTarget returns a fresh mutable DTO after EnsureSupported. Names are labels, never persistent target IDs.

Evidence/fixtures: OutputTargetPlannerTests; HostCapabilitiesTests.

### `OutputTargetFacts` — F

[Declaration L9](../../BESHStat.Excel.Common/src/Output/TargetPlanner.vb#L9). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputTargetFacts
Public Sub New(Optional workbookAvailable As Boolean? = Nothing, Optional sheetExists As Boolean? = Nothing, Optional identityUnambiguous As Boolean? = Nothing, Optional writable As Boolean? = Nothing, Optional protectedSheet As Boolean? = Nothing, Optional mergedConflict As Boolean? = Nothing, Optional canCreateWorksheet As Boolean? = Nothing, Optional resolvedWorksheetName As String = Nothing, Optional worksheetNameValid As Boolean? = Nothing, Optional newWorksheetNameCollision As Boolean? = Nothing, Optional maxRows As Integer = 1048576, Optional maxColumns As Integer = 16384)
Public ReadOnly Property WorkbookAvailable As Boolean?
Public ReadOnly Property SheetExists As Boolean?
Public ReadOnly Property IdentityUnambiguous As Boolean?
Public ReadOnly Property Writable As Boolean?
Public ReadOnly Property ProtectedSheet As Boolean?
Public ReadOnly Property MergedConflict As Boolean?
Public ReadOnly Property CanCreateWorksheet As Boolean?
Public ReadOnly Property ResolvedWorksheetName As String
Public ReadOnly Property WorksheetNameValid As Boolean?
Public ReadOnly Property NewWorksheetNameCollision As Boolean?
Public ReadOnly Property MaxRows As Integer
Public ReadOnly Property MaxColumns As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`.

Tests: `OutputTargetPlannerTests`, `SharedOutputPlanTests`.

### `OutputTargetDecision` — F

[Declaration L50](../../BESHStat.Excel.Common/src/Output/TargetPlanner.vb#L50). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class OutputTargetDecision
Friend Sub New(target As SpreadsheetOutputTarget, host As HostCapabilitySnapshot, facts As OutputTargetFacts, result As CapabilityPreflightResult)
Public ReadOnly Property RequestedDestination As SpreadsheetOutputDestination
Public ReadOnly Property EffectiveDestination As SpreadsheetOutputDestination
Public ReadOnly Property RequestedWorksheetName As String
Public ReadOnly Property WorksheetName As String
Public ReadOnly Property StartRow As Integer
Public ReadOnly Property StartColumn As Integer
Public ReadOnly Property HostName As String
Public ReadOnly Property Platform As String
Public ReadOnly Property SelectedCapability As String
Public ReadOnly Property Facts As OutputTargetFacts
Public ReadOnly Property Result As CapabilityPreflightResult
Public Sub EnsureSupported()
Public Function ToTarget() As SpreadsheetOutputTarget
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/CompositeOutput.vb`, `BESHStat.Excel.Common/src/Output/CompositeOutputExample.vb`.

Tests: `CompositeOutputTests`.

### `TargetPlanner` — I

[Declaration L91](../../BESHStat.Excel.Common/src/Output/TargetPlanner.vb#L91). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class TargetPlanner
Private Sub New()
Public Shared Function Create(requested As SpreadsheetOutputTarget, host As HostCapabilitySnapshot, Optional facts As OutputTargetFacts = Nothing) As OutputTargetDecision
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`, `OutputTargetPlannerTests`, `WriteResultTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Output/WriteResult.vb`

Owner/consumers: SafeWrite, Windows shared orchestration; semantic basis for JS outcomes.

Intent snapshots bounded identifiers/target/lists; exact nonoverlapping write regions, reservation disjointness, max 256 entries. Inspection validates state/occupied IDs, copies lists; consent exact intent reference + ordered observation, not durable permission. SafeWrite rechecks before actions and marks possible mutation before invocation; result validates ordered confirmations/state/consent. SchemaVersion=1 only on WriteResult; undo/rollback all False. No Common serializer/remote authorization or single-use token store.

Evidence/fixtures: WriteResultTests; GateBOutputContractCompatibilityTests; JS safeOverwrite; Windows SharedOutputPlanTests.

### `OutputOccupancy` — F

[Declaration L9](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L9). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputOccupancy
Empty = 0
Occupied = 1
Unknown = 2
Rejected = 3
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`, `SharedOutputPlanTests`.

### `OutputWriteState` — F

[Declaration L16](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L16). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum OutputWriteState
Completed = 0
CancelledByUser = 1
RejectedBeforeWrite = 2
FailedBeforeWrite = 3
PartiallyWritten = 4
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`, `SharedOutputPlanTests`.

### `WriteCodes` — F

[Declaration L24](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L24). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class WriteCodes
Public Const Completed As String = "output.completed"
Public Const Cancelled As String = "output.cancelled"
Public Const Empty As String = "output.occupancy-empty"
Public Const Occupied As String = "output.occupancy-occupied"
Public Const Unverified As String = "output.occupancy-unverified"
Public Const Rejected As String = "output.rejected"
Public Const Changed As String = "output.consent-stale"
Public Const ConfirmationRequired As String = "output.confirmation-required"
Public Const Failed As String = "output.host-failed"
Public Const MutationUncertain As String = "output.mutation-uncertain"
Private Sub New()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`, `SharedOutputPlanTests`.

### `WriteRegion` — F

[Declaration L41](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L41). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class WriteRegion
Public Sub New(artifactId As String, bounds As OutputRectangle, Optional nonCellMutation As Boolean = False)
Public ReadOnly Property ArtifactId As String
Public ReadOnly Property Bounds As OutputRectangle
Public ReadOnly Property MayMutate As Boolean
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If bounds IsNot Nothing AndAlso (bounds.EndRow > 1048576 OrElse bounds.EndColumn > 16384) Then Throw New ArgumentException("Write region outside Excel grid.")`

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`, `SharedOutputPlanTests`.

### `WriteIntent` — F

[Declaration L57](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L57). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class WriteIntent
Public Sub New(planIdentity As String, targetIdentity As String, target As SpreadsheetOutputTarget, regions As IEnumerable(Of WriteRegion), Optional reservations As IEnumerable(Of OutputRectangle) = Nothing, Optional guards As IEnumerable(Of OutputRectangle) = Nothing, Optional workbookIdentity As String = Nothing, Optional worksheetIdentity As String = Nothing)
Public ReadOnly Property PlanIdentity As String
Public ReadOnly Property TargetIdentity As String
Public ReadOnly Property WorkbookIdentity As String
Public ReadOnly Property WorksheetIdentity As String
Public ReadOnly Property WorksheetName As String
Public ReadOnly Property Destination As SpreadsheetOutputDestination
Public ReadOnly Property StartRow As Integer
Public ReadOnly Property StartColumn As Integer
Public ReadOnly Property Regions As IReadOnlyList(Of WriteRegion)
Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
Public ReadOnly Property Guards As IReadOnlyList(Of OutputRectangle)
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If target Is Nothing OrElse Not [Enum].IsDefined(GetType(SpreadsheetOutputDestination), target.Destination) OrElse target.StartRow < 1 OrElse target.StartRow > 1048576 OrElse target.StartColumn < 1 OrElse target.StartColumn > 16384 Then Throw New ArgumentException("Invalid write target.")`
- `If regions Is Nothing Then Throw New ArgumentNullException(NameOf(regions))`
- `If item Is Nothing OrElse items.Count >= 256 OrElse Not ids.Add(item.ArtifactId) Then Throw New ArgumentException("Invalid ordered write identities.")`
- `If item.Bounds IsNot Nothing AndAlso items.Any(Function(p) p.Bounds IsNot Nothing AndAlso p.Bounds.Intersects(item.Bounds)) Then Throw New ArgumentException("Overlapping writes.")`
- `If Me.Reservations.Any(Function(r) items.Any(Function(p) p.Bounds IsNot Nothing AndAlso p.Bounds.Intersects(r))) Then Throw New ArgumentException("Write intersects reservation.")`

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`, `SharedOutputPlanTests`.

### `WriteInspection` — F

[Declaration L113](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L113). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class WriteInspection
Public Sub New(intent As WriteIntent, state As OutputOccupancy, observationIdentity As String, occupiedRegionIds As IEnumerable(Of String), Optional hasValues As Boolean = False, Optional hasFormulas As Boolean = False, Optional explanation As String = Nothing)
Public ReadOnly Property Intent As WriteIntent
Public ReadOnly Property State As OutputOccupancy
Public ReadOnly Property ObservationIdentity As String
Public ReadOnly Property OccupiedRegionIds As IReadOnlyList(Of String)
Public ReadOnly Property HasValues As Boolean
Public ReadOnly Property HasFormulas As Boolean
Public ReadOnly Property Message As String
Public ReadOnly Property ConfirmationRequired As Boolean
Public ReadOnly Property Code As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))`
- `If Not [Enum].IsDefined(GetType(OutputOccupancy), state) Then Throw New ArgumentOutOfRangeException(NameOf(state))`
- `If ids.Count >= 256 OrElse ids.Contains(id) OrElse Not intent.Regions.Any(Function(r) r.ArtifactId = id AndAlso r.MayMutate) Then Throw New ArgumentException("Invalid occupied identities.")`
- `If (state = OutputOccupancy.Empty AndAlso (ids.Count > 0 OrElse hasValues OrElse hasFormulas)) OrElse (state = OutputOccupancy.Occupied AndAlso ids.Count = 0) Then Throw New ArgumentException("Contradictory occupancy.")`

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`, `SharedOutputPlanTests`.

### `WriteConsent` — F

[Declaration L166](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L166). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class WriteConsent
Public Sub New(inspection As WriteInspection, approved As Boolean)
Public ReadOnly Property Inspection As WriteInspection
Public ReadOnly Property Approved As Boolean
Public Function Authorizes(current As WriteInspection) As Boolean
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If inspection Is Nothing OrElse inspection.State <> OutputOccupancy.Occupied Then Throw New ArgumentException("Consent requires known occupancy.")`

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`.

### `WriteResult` — F

[Declaration L184](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L184). **Version/exposure:** ReadOnly SchemaVersion=1; Common in-memory semantic/outcome contract, not a generic JSON protocol.

```vb
Public NotInheritable Class WriteResult
Public Sub New(state As OutputWriteState, code As String, message As String, intent As WriteIntent, mutationMayHaveOccurred As Boolean, Optional confirmedWrittenIds As IEnumerable(Of String) = Nothing, Optional confirmationRequired As Boolean? = Nothing, Optional overwriteApproved As Boolean? = Nothing)
Public ReadOnly Property SchemaVersion As Integer = 1
Public ReadOnly Property State As OutputWriteState
Public ReadOnly Property Code As String
Public ReadOnly Property Message As String
Public ReadOnly Property Intent As WriteIntent
Public ReadOnly Property ConfirmedWritten As IReadOnlyList(Of WriteRegion)
Public ReadOnly Property MutationMayHaveOccurred As Boolean
Public ReadOnly Property ConfirmationRequired As Boolean?
Public ReadOnly Property OverwriteApproved As Boolean?
Public ReadOnly Property UndoSupported As Boolean = False
Public ReadOnly Property RollbackAttempted As Boolean = False
Public ReadOnly Property RollbackSucceeded As Boolean = False
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If Not [Enum].IsDefined(GetType(OutputWriteState), state) Then Throw New ArgumentOutOfRangeException(NameOf(state))`
- `If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))`
- `If (overwriteApproved.HasValue AndAlso Not confirmationRequired.GetValueOrDefault()) OrElse (state = OutputWriteState.CancelledByUser AndAlso (Not confirmationRequired.GetValueOrDefault() OrElse Not overwriteApproved.HasValue OrElse overwriteApproved.Value)) OrElse (state = OutputWriteState.Completed AndAlso confirmationRequired.GetValueOrDefault() AndAlso Not overwriteApproved.GetValueOrDefault()) Then Throw New ArgumentException("Contradictory consent outcome.")`
- `If index <= previous OrElse confirmed.Count >= 256 OrElse Not intent.Regions(index).MayMutate Then Throw New ArgumentException("Invalid confirmed artifact order.")`
- `If (safe AndAlso (mutationMayHaveOccurred OrElse confirmed.Count > 0)) OrElse (state = OutputWriteState.PartiallyWritten AndAlso Not mutationMayHaveOccurred) OrElse (state = OutputWriteState.Completed AndAlso (confirmed.Count <> intent.Regions.Where(Function(r) r.MayMutate).Count() OrElse (confirmed.Count > 0 AndAlso Not mutationMayHaveOccurred))) Then Throw New ArgumentException("Contradictory write outcome.")`

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `GateBOutputContractCompatibilityTests`, `WriteResultTests`, `SharedOutputPlanTests`.

### `SafeWrite` — I

[Declaration L230](../../BESHStat.Excel.Common/src/Output/WriteResult.vb#L230). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class SafeWrite
Private Sub New()
Public Shared Function Execute(intent As WriteIntent, inspect As Func(Of WriteInspection), confirm As Func(Of WriteInspection, Boolean), writes As IReadOnlyList(Of Action), Optional destinationCreated As Boolean = False) As WriteResult
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`.

Tests: `WriteResultTests`, `SharedOutputPlanTests`.

## BESHStat.Excel.Common — `BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb`

Owner/consumers: SharedAnalysisApplicationService, OutputDocument.FromAnalysis, diagnostics.

Independent analysis v1. Tables copied on ingress/access (scalar-cell shallow matrix copy), metrics ordinal-sorted with unique names, table sequence zero-based. Faults determine failure; issues require code/message. Detail is an in-process reference, not deep copied. Snapshot is mutable, structural only (no Values/raw detail), emitted SchemaVersion=1; direct snapshot construction defaults version to 0.

Evidence/fixtures: AnalysisResultDocumentTests; five Stage304 fixtures.

### `AnalysisDocumentStatus` — F

[Declaration L10](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L10). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum AnalysisDocumentStatus
Succeeded = 0
Failed = 1
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Tests: `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`.

### `AnalysisArtifactKind` — F

[Declaration L15](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L15). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum AnalysisArtifactKind
Table = 1
Metric = 2
TypedDetail = 3
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Tests: `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`.

### `AnalysisMetricRole` — F

[Declaration L21](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L21). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum AnalysisMetricRole
Statistic = 0
PValue = 1
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Tests: `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`.

### `AnalysisFaultCodes` — F

[Declaration L26](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L26). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisFaultCodes
Private Sub New()
Public Const InvalidRequest As String = "analysis.invalid-request"
Public Const UnsupportedAnalysis As String = "analysis.unsupported"
Public Const ExecutionFailed As String = "analysis.execution-failed"
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`.

Tests: `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`.

### `AnalysisWarningCodes` — F

[Declaration L34](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L34). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisWarningCodes
Private Sub New()
Public Const AlphaDefaulted As String = "options.alpha.defaulted"
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`.

Tests: `AnalysisResultDocumentTests`.

### `AnalysisWarning` — F

[Declaration L41](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L41). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisWarning
Public Sub New(code As String, message As String, Optional fieldPath As String = Nothing)
Public ReadOnly Property Code As String
Public ReadOnly Property Message As String
Public ReadOnly Property FieldPath As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("Warning code is required.", NameOf(code))`
- `If String.IsNullOrWhiteSpace(message) Then Throw New ArgumentException("Warning message is required.", NameOf(message))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/AnalysisHandlerRegistry.vb`, `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`, `BESHStat.Excel.Common/src/Output/OutputDocument.vb`.

Tests: `AnalysisResultDocumentTests`, `OutputDocumentTests`.

### `AnalysisFault` — F

[Declaration L54](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L54). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisFault
Public Sub New(code As String, message As String, Optional fieldPath As String = Nothing)
Public ReadOnly Property Code As String
Public ReadOnly Property Message As String
Public ReadOnly Property FieldPath As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("Fault code is required.", NameOf(code))`
- `If String.IsNullOrWhiteSpace(message) Then Throw New ArgumentException("Fault message is required.", NameOf(message))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`, `BESHStat.Excel.Common/src/Output/OutputDocument.vb`.

Tests: `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`, `OutputDocumentTests`.

### `AnalysisResultArtifact` — F

[Declaration L68](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L68). **Version/exposure:** No declared version; no wire promise.

```vb
Public MustInherit Class AnalysisResultArtifact
Protected Sub New(kind As AnalysisArtifactKind)
Public ReadOnly Property Kind As AnalysisArtifactKind
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `AnalysisResultDocumentTests`.

### `AnalysisTableArtifact` — F

[Declaration L79](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L79). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisTableArtifact
Inherits AnalysisResultArtifact
Public Sub New(index As Integer, model As Global.BESHStatNG.ResultTableOutputModel)
Public ReadOnly Property Index As Integer
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
Public ReadOnly Property Model As Global.BESHStatNG.ResultTableOutputModel
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If index < 0 Then Throw New ArgumentOutOfRangeException(NameOf(index))`
- `If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))`

Direct source consumers: `BESHStat.Excel.Common/src/Output/OutputDocument.vb`.

Tests: `AnalysisResultDocumentTests`.

### `AnalysisMetricArtifact` — F

[Declaration L119](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L119). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisMetricArtifact
Inherits AnalysisResultArtifact
Public Sub New(name As String, value As Double, role As AnalysisMetricRole, Optional unit As String = Nothing)
Public ReadOnly Property Name As String
Public ReadOnly Property Value As Double
Public ReadOnly Property Role As AnalysisMetricRole
Public ReadOnly Property Unit As String
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Metric name is required.", NameOf(name))`

Tests: `AnalysisResultDocumentTests`.

### `AnalysisDetailArtifact` — I

[Declaration L140](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L140). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class AnalysisDetailArtifact
Inherits AnalysisResultArtifact
Private Sub New(value As Object)
Public ReadOnly Property DetailTypeName As String
Public Shared Function FromValue(Of TDetail As Class)(value As TDetail) As AnalysisDetailArtifact
Public Function TryGet(Of TDetail As Class)(ByRef detail As TDetail) As Boolean
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`.

Tests: `OutputDocumentTests`.

### `AnalysisResultDocument` — F

[Declaration L169](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L169). **Version/exposure:** Const SchemaVersion=1; independent analysis envelope/ToContractSnapshot.

```vb
Public NotInheritable Class AnalysisResultDocument
Public Const SchemaVersion As Integer = 1
Public Sub New(analysisId As String, tables As IEnumerable(Of Global.BESHStatNG.ResultTableOutputModel), metrics As IEnumerable(Of KeyValuePair(Of String, Double)), detail As AnalysisDetailArtifact, Optional warnings As IEnumerable(Of AnalysisWarning) = Nothing, Optional faults As IEnumerable(Of AnalysisFault) = Nothing)
Public ReadOnly Property AnalysisId As String
Public ReadOnly Property Status As AnalysisDocumentStatus
Public ReadOnly Property Artifacts As IReadOnlyList(Of AnalysisResultArtifact)
Public ReadOnly Property Tables As IReadOnlyList(Of AnalysisTableArtifact)
Public ReadOnly Property Metrics As IReadOnlyList(Of AnalysisMetricArtifact)
Public ReadOnly Property Warnings As IReadOnlyList(Of AnalysisWarning)
Public ReadOnly Property Faults As IReadOnlyList(Of AnalysisFault)
Public ReadOnly Property TypedDetail As AnalysisDetailArtifact
Public Function TryGetDetail(Of TDetail As Class)(ByRef value As TDetail) As Boolean
Public Function ToContractSnapshot() As AnalysisResultDocumentSnapshot
Public Shared Function Failure(analysisId As String, fault As AnalysisFault) As AnalysisResultDocument
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(analysisId) Then Throw New ArgumentException("Analysis ID is required.", NameOf(analysisId))`
- `If tables Is Nothing Then Throw New ArgumentNullException(NameOf(tables))`
- `If metrics Is Nothing Then Throw New ArgumentNullException(NameOf(metrics))`
- `If item Is Nothing Then Throw New ArgumentException("Null warning.", NameOf(warnings))`
- `If item Is Nothing Then Throw New ArgumentException("Null fault.", NameOf(faults))`
- `If model Is Nothing Then Throw New ArgumentException("Null result table.", NameOf(tables))`
- `If Not names.Add(pair.Key) Then Throw New ArgumentException("Duplicate metric: " & pair.Key, NameOf(metrics))`

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/SharedAnalysisApplicationService.vb`, `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`, `BESHStat.Excel.Common/src/Output/OutputDocument.vb`.

Tests: `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`, `OutputDocumentTests`.

### `AnalysisResultDocumentSnapshot` — F

[Declaration L347](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L347). **Version/exposure:** Mutable SchemaVersion (default 0); ToContractSnapshot emits 1; explicit structural representation.

```vb
Public NotInheritable Class AnalysisResultDocumentSnapshot
Public Property SchemaVersion As Integer
Public Property AnalysisId As String
Public Property Status As AnalysisDocumentStatus
Public Property DetailTypeName As String
Public Property Tables As List(Of AnalysisTableSnapshot) = New List(Of AnalysisTableSnapshot)()
Public Property Metrics As List(Of AnalysisMetricSnapshot) = New List(Of AnalysisMetricSnapshot)()
Public Property Warnings As List(Of AnalysisIssueSnapshot) = New List(Of AnalysisIssueSnapshot)()
Public Property Faults As List(Of AnalysisIssueSnapshot) = New List(Of AnalysisIssueSnapshot)()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Tests: `AnalysisResultDocumentTests`, `GateBOutputContractCompatibilityTests`, `PValuePresentationPlanTests`.

### `AnalysisTableSnapshot` — F

[Declaration L358](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L358). **Version/exposure:** No own version; mutable nested representation of AnalysisResultDocumentSnapshot v1. Existing diagnostic JSON serialization/reading, not full output-cell wire.

```vb
Public NotInheritable Class AnalysisTableSnapshot
Public Property Index As Integer
Public Property RowCount As Integer
Public Property ColumnCount As Integer
Public Property TitleRows As Integer
Public Property HeaderTopRows As Integer
Public Property HeaderLeftColumns As Integer
Public Property FooterRows As Integer
Public Property IsResultTable As Boolean
Public Property PvalueColumns As List(Of Integer) = New List(Of Integer)()
Public Property PvalueCells As List(Of AnalysisCellAddressSnapshot) = New List(Of AnalysisCellAddressSnapshot)()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

### `AnalysisCellAddressSnapshot` — F

[Declaration L371](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L371). **Version/exposure:** No own version; mutable nested representation of AnalysisResultDocumentSnapshot v1. Existing diagnostic JSON serialization/reading, not full output-cell wire.

```vb
Public NotInheritable Class AnalysisCellAddressSnapshot
Public Property BodyRow As Integer
Public Property BodyColumn As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

### `AnalysisMetricSnapshot` — F

[Declaration L376](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L376). **Version/exposure:** No own version; mutable nested representation of AnalysisResultDocumentSnapshot v1. Existing diagnostic JSON serialization/reading, not full output-cell wire.

```vb
Public NotInheritable Class AnalysisMetricSnapshot
Public Property Name As String
Public Property Value As Nullable(Of Double)
Public Property NumericKind As String
Public Property Role As AnalysisMetricRole
Public Property Unit As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

### `AnalysisIssueSnapshot` — F

[Declaration L384](../../BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb#L384). **Version/exposure:** No own version; mutable nested representation of AnalysisResultDocumentSnapshot v1. Existing diagnostic JSON serialization/reading, not full output-cell wire.

```vb
Public NotInheritable Class AnalysisIssueSnapshot
Public Property Code As String
Public Property Message As String
Public Property FieldPath As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

## BESHStat.Core — `BESHStat.Core/src/DM/ResultWriter/ResultTableOutput.vb`

Owner/consumers: Common document/factory; legacy writers.

Core compatibility model: mutable Values matrix/list contents despite read-only properties; result-table writer APIs/cursor remain legacy. P-value addresses one-based body-relative; assembled matrix zero-based includes actual titles/headers/footers. Common OutputTableArtifact supplies stronger validation/scalar isolation.

Evidence/fixtures: Stage304ContractSnapshotTests; OutputDocumentTests; Windows SharedOutputPlanTests.

### `ResultTableCellAddress` — L

[Declaration L9](../../BESHStat.Core/src/DM/ResultWriter/ResultTableOutput.vb#L9). **Version/exposure:** No declared version; no wire promise.

```vb
Public Structure ResultTableCellAddress
Public Sub New(bodyRow As Integer, bodyColumn As Integer)
Public ReadOnly Property BodyRow As Integer
Public ReadOnly Property BodyColumn As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`, `BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb`, `BESHStat.Excel.Common/src/Results/AnalysisResultDocument.vb`.

Tests: `OfficeOutputPlanAdapterTests`, `OutputDocumentTests`, `OutputLayoutPlannerTests`, `OutputStylesTests`.

### `ResultTableOutputModel` — L

[Declaration L28](../../BESHStat.Core/src/DM/ResultWriter/ResultTableOutput.vb#L28). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class ResultTableOutputModel
Public ReadOnly Property Values As Object(,)
Public ReadOnly Property HeaderTopRows As Integer
Public ReadOnly Property HeaderLeftColumns As Integer
Public ReadOnly Property FooterRows As Integer
Public ReadOnly Property PvalueColumns As List(Of Integer)
Public ReadOnly Property PvalueCells As List(Of ResultTableCellAddress)
Public ReadOnly Property TitleRows As Integer
Public ReadOnly Property IsResultTable As Boolean
Public Sub New(values As Object(,), Optional headerTopRows As Integer = 0, Optional headerLeftColumns As Integer = 0, Optional footerRows As Integer = 0, Optional pvalueColumns As IEnumerable(Of Integer) = Nothing, Optional titleRows As Integer = 0, Optional isResultTable As Boolean = True, Optional pvalueCells As IEnumerable(Of ResultTableCellAddress) = Nothing)
Public ReadOnly Property RowCount As Integer
Public ReadOnly Property ColumnCount As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb`, `BESHStat.Excel.Common/src/Analysis/OneWayAnovaApplicationService.vb`, `BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb`.

Tests: `AnalysisHandlerRegistryTests`, `AnalysisResultDocumentTests`, `CompositeOutputTests`, `OfficeOutputPlanAdapterTests`.

### `ResultTableOutputBlock` — L

[Declaration L74](../../BESHStat.Core/src/DM/ResultWriter/ResultTableOutput.vb#L74). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class ResultTableOutputBlock
Public ReadOnly Property StartRow As Integer
Public ReadOnly Property StartColumn As Integer
Public ReadOnly Property Model As ResultTableOutputModel
Public Sub New(startRow As Integer, startColumn As Integer, model As ResultTableOutputModel)
Public ReadOnly Property EndRow As Integer
Public ReadOnly Property EndColumn As Integer
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`.

Tests: `CompositeOutputPlanTests`, `SharedOutputPlanTests`.

### `ResultTableWriterBase` — L

[Declaration L104](../../BESHStat.Core/src/DM/ResultWriter/ResultTableOutput.vb#L104). **Version/exposure:** No declared version; no wire promise.

```vb
Public MustInherit Class ResultTableWriterBase
Protected Sub New(Optional row As Integer = 1, Optional col As Integer = 1)
Public ReadOnly Property RowID() As Integer
Public ReadOnly Property ColID() As Integer
Public Sub setRowPointer(Optional r As Integer = 1)
Public Sub shiftRowPointer(Optional by As Integer = 1)
Public Sub setColumnPointer(Optional c As Integer = 1)
Public Sub shiftColumnPointer(Optional by As Integer = 1)
Public Overridable Sub write(ds As Object, Optional bTall As Boolean = False)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`.

## BESHStat.Core — `BESHStat.Core/src/DM/ResultWriter/ResultTable.vb`

Owner/consumers: Core analysis producers; Common OutputDocument.FromTable.

Legacy mutable table builder/assembler, not a new Common DTO. returnSelf/ToOutputModel assemble and pad title/header/body/footer sections. Original p-value additions use one-based body addresses and filter duplicate/invalid values. Preserve this behavior; Common snapshot conversion adds isolation without recomputing statistics.

Evidence/fixtures: Stage304 five fixtures; OutputDocumentTests; Core/Windows result table tests.

### `ResultTable` — L

[Declaration L43](../../BESHStat.Core/src/DM/ResultWriter/ResultTable.vb#L43). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class ResultTable
Public bLeftHeaderAdjustUp As Boolean = False
Public ReadOnly Property TotalRows() As Integer
Public ReadOnly Property TotalCols() As Integer
Public ReadOnly Property PvalColumns() As List(Of Integer)
Public ReadOnly Property PvalCells() As List(Of ResultTableCellAddress)
Public ReadOnly Property HeadersTopCount() As Integer
Public ReadOnly Property HeadersLeftCount() As Integer
Public ReadOnly Property FootersCount() As Integer
Public ReadOnly Property TitlesCount() As Integer
Public Sub SetBody(b(,) As Object)
Public Sub SetBody(b() As Object)
Public Sub SetBody(b(,) As Double)
Public Sub SetBody(b(,) As Integer)
Public Sub AddFootnote(x As String)
Public Sub AddTitle(x As String)
Public Sub AddPvalueToFormat(columnNumber As Integer)
Public Sub AddPvalueCellToFormat(bodyRow As Integer, bodyColumn As Integer)
Public Sub AddHeaderTopRow(header() As String, Optional bPadleft As Boolean = False)
Public Sub AddHeaderLeftRow(header() As String, Optional bPadleft As Boolean = False)
Public Function returnSelf() As Object(,)
Public Function ToOutputModel() As ResultTableOutputModel
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Analysis/KruskalWallisApplicationService.vb`, `BESHStat.Excel.Common/src/Analysis/OneWayAnovaApplicationService.vb`, `BESHStat.Excel.Common/src/Output/OutputDocument.vb`.

Tests: `OutputDocumentTests`, `SharedOutputPlanTests`.

### `ProcessListofResultTables` — L

[Declaration L560](../../BESHStat.Core/src/DM/ResultWriter/ResultTable.vb#L560). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class ProcessListofResultTables
Public Sub New(xlist As List(Of ResultTable))
Public ReadOnly Property TotRows(Optional layout As String = "below") As Integer
Public ReadOnly Property TotCols(Optional layout As String = "below") As Integer
Public Sub SetSeparators(x As List(Of Object(,)))
Public Sub writeToSheet(w As ResultTableWriterBase, Optional bDefaultBlankRowSep As Boolean = False)
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If xlist Is Nothing Then Throw New ArgumentNullException(NameOf(xlist))`

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`, `BESHstatNG/src/UI/UibyID.vb`.

## BESHStat.Core — `BESHStat.Core/src/AppInfrastructure/AppSettings.vb`

Owner/consumers: Core p-value policy; existing Windows XML preference store.

Core owns PValuePresentationSettings (decimals 2..16 default 8; enum 0/1/2, upper bound True), mutable normalized on demand. Existing BeshStatNgSettings XML version=3 is independent of Common v1; settings store filesystem behavior is legacy and is not proposed as the future Common preference contract. 03.01 must review migration separately.

Evidence/fixtures: Core SettingsAndPValueTests; Common PValuePresentationPlanTests.

### `BeshStatNgSettings` — L

[Declaration L11](../../BESHStat.Core/src/AppInfrastructure/AppSettings.vb#L11). **Version/exposure:** Constructor Version=3; XML attribute version; independent legacy settings storage.

```vb
Public Class BeshStatNgSettings
Public Sub New()
Public Property Version As Integer
Public Property Diagnostics As DiagnosticsSettings
Public Property PValuePresentation As PValuePresentationSettings
Public Property DefaultAlpha As Double
Public Property DefaultRandomSeed As Integer
Public Sub EnsureDefaults()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

### `DiagnosticsSettings` — L

[Declaration L60](../../BESHStat.Core/src/AppInfrastructure/AppSettings.vb#L60). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class DiagnosticsSettings
Public Sub New()
Public Property TraceExecutionLoggingEnabled As Boolean
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

### `PValueSmallValueDisplay` — F

[Declaration L72](../../BESHStat.Core/src/AppInfrastructure/AppSettings.vb#L72). **Version/exposure:** No declared version; no wire promise.

```vb
Public Enum PValueSmallValueDisplay
LessThanThreshold = 0
ScientificNotation = 1
FixedDecimal = 2
```

No constructor; preserve the existing Int32 meanings above. New members still require consumer/closed-family review.

Direct source consumers: `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`, `BESHStat.WasmSmoke/Pages/Home.razor`.

Tests: `PValuePresentationPlanTests`, `SharedExcelLayerTests`, `Stage304ContractSnapshotTests`.

### `PValuePresentationSettings` — F

[Declaration L81](../../BESHStat.Core/src/AppInfrastructure/AppSettings.vb#L81). **Version/exposure:** No own version; existing mutable Core settings nested in legacy XML settings version 3; Common-facing display policy is the proposed freeze boundary.

```vb
Public Class PValuePresentationSettings
Public Const MinimumDecimalPlaces As Integer = 2
Public Const MaximumDecimalPlaces As Integer = 16
Public Const DefaultDecimalPlaces As Integer = 8
Public Sub New()
Public Property DecimalPlaces As Integer
Public Property SmallValueDisplay As PValueSmallValueDisplay
Public Property UseUpperBound As Boolean
Public Sub EnsureDefaults()
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`, `BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb`.

Tests: `OutputDocumentTests`, `PValuePresentationPlanTests`, `SharedExcelLayerTests`, `Stage304ContractSnapshotTests`.

### `BeshStatNgSettingsStore` — L

[Declaration L110](../../BESHStat.Core/src/AppInfrastructure/AppSettings.vb#L110). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class BeshStatNgSettingsStore
Public Sub New(baseDirectory As String)
Public ReadOnly Property SettingsPath As String
Public Function Load() As BeshStatNgSettings
Public Sub Save(settings As BeshStatNgSettings)
Public Shared Function CreateDefault() As BeshStatNgSettings
```

Direct constructor guards (source expressions; helper/base and parser/method validation remains in the linked source):

- `If String.IsNullOrWhiteSpace(baseDirectory) Then Throw New ArgumentException("baseDirectory must not be empty.", NameOf(baseDirectory))`

## BESHStat.Core — `BESHStat.Core/src/DM/ResultWriter/PValuePresentation.vb`

Owner/consumers: Common OutputPValuePolicy; legacy request factory/host writers.

Core display algorithm ownership; existing threshold/scientific/fixed modes, including legacy nonfinite display fallback. Strict lower/upper display boundaries, explicit provider supported; display text never replaces a primary statistical numeric value. No serializer/version.

Evidence/fixtures: Core SettingsAndPValueTests; Common PValuePresentationPlanTests.

### `PValuePresentation` — I

[Declaration L15](../../BESHStat.Core/src/DM/ResultWriter/PValuePresentation.vb#L15). **Version/exposure:** No declared version; no wire promise.

```vb
Public NotInheritable Class PValuePresentation
Private Sub New()
Public Shared Function LowerThreshold(settings As PValuePresentationSettings) As Double
Public Shared Function UpperThreshold(settings As PValuePresentationSettings) As Double
Public Shared Function BuildExcelNumberFormat(settings As PValuePresentationSettings) As String
Public Shared Function FormatForDisplay(value As Double, settings As PValuePresentationSettings, Optional formatProvider As IFormatProvider = Nothing) As String
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHStat.Excel.Common/src/Output/OfficeOutputPlanAdapter.vb`, `BESHStat.Excel.Common/src/Output/PValuePresentation.vb`, `BESHStat.Excel.Common/src/Output/ResultTableWriteRequest.vb`.

Tests: `PValuePresentationPlanTests`, `Stage304ContractSnapshotTests`, `CompositeOutputPlanTests`, `SharedOutputPlanTests`.

## BESHStatNG — `BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb`

Owner/consumers: Existing Windows UI and shared planned orchestration.

Windows-only public legacy renderer; COM ranges, row/column cursor and current-culture numeric-text highlight semantics remain. No serialization/version. Shared plan adapter uses exact bounds but retains historical formatter and AutoFit=False in UI. Internal target/composite seams are catalogued separately below.

Evidence/fixtures: Windows SharedOutputPlanTests; CompositeOutputPlanTests.

### `ExcelDnaResultWriter` — H

[Declaration L14](../../BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb#L14). **Version/exposure:** No declared version; no wire promise.

```vb
Public Class ExcelDnaResultWriter
Inherits ResultTableWriterBase
Public wb As Workbook
Public ws As Worksheet
Public Sub New(Optional row As Integer = 1, Optional col As Integer = 1)
Public Sub WriteSharedRequest(request As Global.BESHStatNG.ExcelCommon.ResultTableWriteRequest)
```

Construction: no direct throwing guard in this type; restricted/factory/helper/base construction is shown where declared. Apply the family snapshot/mutation rules; this does not establish absence of validation in called helpers.

Direct source consumers: `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`, `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`, `BESHstatNG/src/UI/UibyID.vb`.

Tests: `CompositeOutputPlanTests`, `SharedOutputPlanTests`.

## Host/internal surfaces and unresolved ownership

These types are included even where their accessibility excludes them from the public Common assembly surface. They are externally observable adapter behavior, not additional Common DTO authorities.

| Path / types or API | Class | Version / invariants / consumers / evidence |
|---|---|---|
| `BESHstatNG/src/DM/ResultWriter/ExcelDnaSharedOutputPlanAdapter.vb`: ExcelDnaSharedOutputHost, ExcelDnaSharedOutputRun, ExcelDnaSharedWriteCommand, ExcelDnaPreparedSharedOutput, ExcelDnaSharedOutputPlanAdapter | H | Internal .NET Framework 4.8 orchestration/command snapshots, no wire version; exact tables plus supplements, noncell native charts, separate chart decision; consumed by UibyID; SharedOutputPlanTests. |
| `BESHstatNG/src/DM/ResultWriter/ExcelDnaOutputTargetAdapter.vb`: ExcelDnaChartSafetySnapshot, ExcelDnaOccupancySnapshot, ExcelDnaOutputTargetAdapter | H | Internal transient COM target HMAC identity; native Value2/Formula reads (one-based COM arrays); shape inspection separate; no persistent serialized IDs; SharedOutputPlanTests. |
| `BESHstatNG/src/DM/ResultWriter/ExcelDnaCompositeOutputAdapter.vb`: ExcelDnaCompositeCommand, ExcelDnaPreparedCompositeOutput, ExcelDnaCompositeOutputAdapter | H | Internal new-sheet insertion/recording seam, literal-block sink required; no live production renderer; CompositeOutputPlanTests. |
| `BESHStat.WasmSmoke/wwwroot/js/officeBridge.js`: writeOutputPlan, writeCompositeOutputPlan, outcome, pendingOverwrites | H | Strict camelCase table wire 1 and composite wire 1; host outcome schemaVersion 1 with string final states plus intermediate AwaitingConfirmation; single-use expiring tokens, exact observation revalidation; JS fixtures/safeOverwrite tests. Does not deserialize Common WriteResult. |
| `BESHStat.WasmSmoke/Pages/Home.razor`: private OfficeWriteResult, OfficeOverwriteConfirmation | H | Mutable task-pane response projection (State/Code/Message, nullable consent/destination, Planned/ConfirmedWritten JSON elements, addresses/format; confirmation Token/WorksheetName/Rectangles/OccupiedRegionCount/Message). No independent version declared; confirm invokes same prepared plan, no analysis rerun; JS task-pane wiring tests. |
| `officeBridge.js`: writeResultTable(s); Common ResultTableWriteRequest/BodyCellAddress | L | Deprecated signatures; current-sheet bypass explicitly rejected output.legacy-current-unsupported; legacy new-sheet compatibility remains, no new lossless transport promise. Stage304/JS compatibility tests. |
| Durable workbook IDs; full analysis-family detail serialization; native ChartDocument; future production Office transport/settings DTOs | U | Not implemented by Gate B. No COM pointer/name-based persistence, fake SchemaVersion or inferred protocol equality. Phase 03/04/04A changes require separate authorization, migration tests and owner review. |

Inventory classification totals for declaration entries above: F=147, H=10, I=32, L=17. Host/internal table rows are additional scoped surfaces, not included in those counts. No production enum, schema, dependency or API was changed by this ledger.

## Task-pane response projection and JS observation fields

The following is the existing private task-pane C# projection, classified H, not a public Common DTO. JSON interop supplies camelCase names; primitive defaults permit omitted fields in host responses. In particular it does not declare SchemaVersion and does not prove validation/version negotiation. Source: `BESHStat.WasmSmoke/Pages/Home.razor`.

```csharp
private sealed class OfficeWriteResult
{
    public string State { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool MutationMayHaveOccurred { get; set; }
    public bool UndoSupported { get; set; }
    public bool RollbackAttempted { get; set; }
    public bool RollbackSucceeded { get; set; }
    public bool? ConfirmationRequired { get; set; }
    public bool? OverwriteApproved { get; set; }
    public int? Destination { get; set; }
    public System.Text.Json.JsonElement[] Planned { get; set; } = Array.Empty<System.Text.Json.JsonElement>();
    public System.Text.Json.JsonElement[] ConfirmedWritten { get; set; } = Array.Empty<System.Text.Json.JsonElement>();
    public OfficeOverwriteConfirmation? Confirmation { get; set; }
    public string WorksheetName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string[] TableAddresses { get; set; } = Array.Empty<string>();
    public int[] PvalueColumns { get; set; } = Array.Empty<int>();
    public string PvalueNumberFormat { get; set; } = string.Empty;
}
private sealed class OfficeOverwriteConfirmation
{
    public string Token { get; set; } = string.Empty;
    public string WorksheetName { get; set; } = string.Empty;
    public string[] Rectangles { get; set; } = Array.Empty<string>();
    public int OccupiedRegionCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

`officeBridge.js` outcome() always supplies schemaVersion:number=1, state/code/message:string, mutationMayHaveOccurred:boolean, undoSupported/rollbackAttempted/rollbackSucceeded:boolean=False, nullable confirmationRequired/overwriteApproved, worksheetName/address:string and tableAddresses/planned/confirmedWritten arrays. This is H, independently versioned. Final states are strings, not serialized Common enum integers. AwaitingConfirmation is an intermediate state only. planned entries contain artifactId:string and bounds (four one-based inclusive Integer fields); successful confirmedWritten entries also carry host address:string. Office failure after queued operations confirms no speculative subset.

Optional response fields depend on execution path: destination:null or numeric enum, startRow/startColumn:positive Integer, occupancy:string, occupiedRegionCount:Integer, hasValues/hasFormulas:Boolean, pvalueColumns:array, pvalueNumberFormat:string, and hostCode:null or bounded safe string. AwaitingConfirmation adds confirmation {token:string, worksheetName:string, rectangles:string[], occupiedRegionCount:Integer, message:string}. Neither token nor observation content becomes a durable workbook ID; raw values/formulas and the private fingerprint stay inside the bridge closure. Confirmation input is the same prepared plan plus {token:string, approved:Boolean}; it is not serialized Common WriteConsent.

The projection intentionally selects only fields Home uses; fields present in the JS response but absent from the private C# class are not evidence of lost Common contract members. Tests: `safeOverwrite.test.cjs`, `outputPlanWriter.test.cjs`, `compositeOutputWriter.test.cjs`; see proposal version/host matrices.
