Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports BESHStatNG.AppInfrastructure

Namespace ExcelCommon
    ''' <summary>Mutable transport only. Nothing means absent, never numeric zero/False.</summary>
    Public NotInheritable Class PreferenceData
        Public Property SchemaVersion As Integer = 1
        Public Property DefaultAlpha As Double?
        Public Property DefaultRandomSeed As Integer?
        Public Property TraceEnabled As Boolean?
        Public Property PValues As PValuePreferenceData
    End Class

    Public NotInheritable Class PValuePreferenceData
        Public Property DecimalPlaces As Integer?
        Public Property SmallValueDisplay As PValueSmallValueDisplay?
        Public Property UseUpperBound As Boolean?
    End Class

    Public Enum PreferenceStatus
        Loaded = 0
        Defaulted = 1
        Normalized = 2
        Migrated = 3
        Saved = 4
        UnsupportedVersion = 5
        InvalidData = 6
        StorageUnavailable = 7
        Failed = 8
    End Enum

    ''' <summary>Persisted describes the returned snapshot, not merely storage availability.</summary>
    Public Enum PreferencePersistence
        Unknown = 0
        Persisted = 1
        SessionOnly = 2
        Unavailable = 3
    End Enum

    Public NotInheritable Class PreferenceResult
        Public Sub New(snapshot As PreferenceSnapshot, status As PreferenceStatus,
                       persistence As PreferencePersistence, Optional codes As IEnumerable(Of String) = Nothing)
            If snapshot Is Nothing Then Throw New ArgumentNullException(NameOf(snapshot))
            Me.Snapshot = snapshot
            Me.Status = status
            Me.Persistence = persistence
            Me.Codes = New List(Of String)(If(codes, Array.Empty(Of String)())).AsReadOnly()
        End Sub
        Public ReadOnly Property Snapshot As PreferenceSnapshot
        Public ReadOnly Property Status As PreferenceStatus
        Public ReadOnly Property Persistence As PreferencePersistence
        Public ReadOnly Property Codes As IReadOnlyList(Of String)
        Public ReadOnly Property CanUseStoredData As Boolean
            Get
                Return Status = PreferenceStatus.Loaded OrElse Status = PreferenceStatus.Defaulted OrElse
                    Status = PreferenceStatus.Normalized OrElse Status = PreferenceStatus.Migrated OrElse Status = PreferenceStatus.Saved
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Hosts own store and snapshot lifetimes. Reads never rewrite incompatible data.
    ''' Save is intentional; failure must preserve the active snapshot. A successful save
    ''' may be session-only, and must say so. Async accommodates browser interop without
    ''' importing a browser or imposing a cancellation promise on an indivisible write.
    ''' </summary>
    Public Interface IPreferenceStore
        Function LoadAsync() As Task(Of PreferenceResult)
        Function SaveAsync(snapshot As PreferenceSnapshot) As Task(Of PreferenceResult)
    End Interface

    ''' <summary>
    ''' Immutable effective preferences v1. Independent of Windows XML v3, analysis
    ''' document v1, and Office prototype output/storage wire versions. No global state.
    ''' </summary>
    Public NotInheritable Class PreferenceSnapshot
        Public Const CurrentSchemaVersion As Integer = 1
        Private Sub New(alpha As Double, seed As Integer, trace As Boolean, display As PValuePresentationSettings)
            DefaultAlpha = alpha
            DefaultRandomSeed = seed
            TraceEnabled = trace
            DecimalPlaces = display.DecimalPlaces
            SmallValueDisplay = display.SmallValueDisplay
            UseUpperBound = display.UseUpperBound
        End Sub
        Public ReadOnly Property SchemaVersion As Integer = CurrentSchemaVersion
        Public ReadOnly Property DefaultAlpha As Double
        Public ReadOnly Property DefaultRandomSeed As Integer
        Public ReadOnly Property TraceEnabled As Boolean
        Public ReadOnly Property DecimalPlaces As Integer
        Public ReadOnly Property SmallValueDisplay As PValueSmallValueDisplay
        Public ReadOnly Property UseUpperBound As Boolean

        Public Shared Function Defaults() As PreferenceSnapshot
            Return New PreferenceSnapshot(0.05, AnalysisDefaultsOptions.NoExplicitRandomSeed, True, New PValuePresentationSettings())
        End Function

        ''' <summary>
        ''' Only schema 1 exists. Unknown older/newer schemas are explicit failures, with
        ''' session defaults, never an invented migration. Missing fields default separately.
        ''' Core finite alpha / seed and display normalization are the sole policy.
        ''' </summary>
        Public Shared Function Normalize(data As PreferenceData) As PreferenceResult
            If data Is Nothing Then Return New PreferenceResult(Defaults(), PreferenceStatus.Defaulted,
                PreferencePersistence.SessionOnly, {"preferences.missing"})
            If data.SchemaVersion <> CurrentSchemaVersion Then Return New PreferenceResult(Defaults(),
                PreferenceStatus.UnsupportedVersion, PreferencePersistence.SessionOnly, {"preferences.version.unsupported"})
            Dim codes As New List(Of String)()
            Dim runtime As New AnalysisDefaultsOptions(data.DefaultAlpha.GetValueOrDefault(0.05),
                data.DefaultRandomSeed.GetValueOrDefault(AnalysisDefaultsOptions.NoExplicitRandomSeed))
            If Not data.DefaultAlpha.HasValue Then
                codes.Add("preferences.alpha.missing")
            ElseIf data.DefaultAlpha.Value <> runtime.DefaultAlpha Then
                codes.Add("preferences.alpha.defaulted")
            End If
            If Not data.DefaultRandomSeed.HasValue Then
                codes.Add("preferences.seed.missing")
            ElseIf data.DefaultRandomSeed.Value <> runtime.DefaultRandomSeed Then
                codes.Add("preferences.seed.defaulted")
            End If
            If Not data.TraceEnabled.HasValue Then codes.Add("preferences.trace.missing")
            Dim display As New PValuePresentationSettings()
            If data.PValues Is Nothing Then
                codes.Add("preferences.display.missing")
            Else
                If data.PValues.DecimalPlaces.HasValue Then
                    display.DecimalPlaces = data.PValues.DecimalPlaces.Value
                Else
                    codes.Add("preferences.digits.missing")
                End If
                If data.PValues.SmallValueDisplay.HasValue Then
                    display.SmallValueDisplay = data.PValues.SmallValueDisplay.Value
                Else
                    codes.Add("preferences.mode.missing")
                End If
                display.UseUpperBound = data.PValues.UseUpperBound.GetValueOrDefault(True)
                If Not data.PValues.UseUpperBound.HasValue Then codes.Add("preferences.upper-bound.missing")
                display.EnsureDefaults()
                If data.PValues.DecimalPlaces.HasValue AndAlso data.PValues.DecimalPlaces.Value <> display.DecimalPlaces Then codes.Add("preferences.digits.defaulted")
                If data.PValues.SmallValueDisplay.HasValue AndAlso data.PValues.SmallValueDisplay.Value <> display.SmallValueDisplay Then codes.Add("preferences.mode.defaulted")
            End If
            Return New PreferenceResult(New PreferenceSnapshot(runtime.DefaultAlpha, runtime.DefaultRandomSeed,
                data.TraceEnabled.GetValueOrDefault(True), display), If(codes.Count = 0, PreferenceStatus.Loaded,
                PreferenceStatus.Normalized), PreferencePersistence.SessionOnly, codes)
        End Function

        Public Function ToData() As PreferenceData
            Return New PreferenceData With {.SchemaVersion = SchemaVersion, .DefaultAlpha = DefaultAlpha,
                .DefaultRandomSeed = DefaultRandomSeed, .TraceEnabled = TraceEnabled,
                .PValues = New PValuePreferenceData With {.DecimalPlaces = DecimalPlaces,
                    .SmallValueDisplay = SmallValueDisplay, .UseUpperBound = UseUpperBound}}
        End Function

        Public Function CreateDisplaySettings() As PValuePresentationSettings
            Return New PValuePresentationSettings With {.DecimalPlaces = DecimalPlaces,
                .SmallValueDisplay = SmallValueDisplay, .UseUpperBound = UseUpperBound}
        End Function

        Public Function CreateAnalysisDefaults() As AnalysisDefaultsOptions
            Return New AnalysisDefaultsOptions(DefaultAlpha, DefaultRandomSeed)
        End Function

        ''' <summary>Explicit host application; never called implicitly during load/save.</summary>
        Public Sub ApplyToCoreDefaults()
            CoreServices.AnalysisDefaults = CreateAnalysisDefaults()
            CoreServices.TraceExecutionLoggingEnabled = TraceEnabled
        End Sub

        Public Function CreateAnovaOptions() As OneWayAnovaAnalysisOptions
            Return New OneWayAnovaAnalysisOptions With {.Alpha = DefaultAlpha}
        End Function
        Public Function CreateKruskalWallisOptions() As KruskalWallisAnalysisOptions
            Return New KruskalWallisAnalysisOptions With {.Alpha = DefaultAlpha}
        End Function
    End Class

    ''' <summary>Deterministic host-owned test/session store. No durable persistence claim.</summary>
    Public NotInheritable Class InMemoryPreferenceStore
        Implements IPreferenceStore
        Private _snapshot As PreferenceSnapshot
        Public Function LoadAsync() As Task(Of PreferenceResult) Implements IPreferenceStore.LoadAsync
            Return Task.FromResult(New PreferenceResult(If(_snapshot, PreferenceSnapshot.Defaults()),
                If(_snapshot Is Nothing, PreferenceStatus.Defaulted, PreferenceStatus.Loaded), PreferencePersistence.SessionOnly))
        End Function
        Public Function SaveAsync(snapshot As PreferenceSnapshot) As Task(Of PreferenceResult) Implements IPreferenceStore.SaveAsync
            If snapshot Is Nothing Then Throw New ArgumentNullException(NameOf(snapshot))
            _snapshot = snapshot ' Every reachable value is immutable.
            Return Task.FromResult(New PreferenceResult(snapshot, PreferenceStatus.Saved, PreferencePersistence.SessionOnly))
        End Function
    End Class
End Namespace
