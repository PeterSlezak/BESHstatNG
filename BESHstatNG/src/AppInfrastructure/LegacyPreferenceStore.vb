Option Explicit On
Option Strict On

Imports System.IO
Imports System.Threading.Tasks
Imports System.Xml
Imports BESHStatNG.ExcelCommon

Namespace AppInfrastructure
    ''' <summary>Windows-only facade over the unchanged XLL-adjacent XML store.</summary>
    Public NotInheritable Class LegacyPreferenceStore
        Implements IPreferenceStore
        Private ReadOnly _store As BeshStatNgSettingsStore
        Public Sub New(store As BeshStatNgSettingsStore)
            If store Is Nothing Then Throw New ArgumentNullException(NameOf(store))
            _store = store
        End Sub

        Public Shared Function FromLegacy(settings As BeshStatNgSettings) As PreferenceResult
            If settings Is Nothing Then Return PreferenceSnapshot.Normalize(Nothing)
            If settings.Version < 1 OrElse settings.Version > 3 Then Return Unsupported()
            Dim data As New PreferenceData With {.DefaultAlpha = settings.DefaultAlpha,
                .DefaultRandomSeed = settings.DefaultRandomSeed,
                .TraceEnabled = If(settings.Diagnostics Is Nothing, CType(Nothing, Boolean?), settings.Diagnostics.TraceExecutionLoggingEnabled)}
            If settings.PValuePresentation IsNot Nothing Then
                data.PValues = New PValuePreferenceData With {.DecimalPlaces = settings.PValuePresentation.DecimalPlaces,
                    .SmallValueDisplay = settings.PValuePresentation.SmallValueDisplay,
                    .UseUpperBound = settings.PValuePresentation.UseUpperBound}
            End If
            Dim result = PreferenceSnapshot.Normalize(data)
            Return New PreferenceResult(result.Snapshot, If(settings.Version < 3, PreferenceStatus.Migrated, result.Status),
                PreferencePersistence.SessionOnly, result.Codes)
        End Function

        Public Shared Function ToLegacy(snapshot As PreferenceSnapshot) As BeshStatNgSettings
            If snapshot Is Nothing Then Throw New ArgumentNullException(NameOf(snapshot))
            Return New BeshStatNgSettings With {.DefaultAlpha = snapshot.DefaultAlpha,
                .DefaultRandomSeed = snapshot.DefaultRandomSeed,
                .Diagnostics = New DiagnosticsSettings With {.TraceExecutionLoggingEnabled = snapshot.TraceEnabled},
                .PValuePresentation = snapshot.CreateDisplaySettings()}
        End Function

        Private Shared Function Unsupported() As PreferenceResult
            Return New PreferenceResult(PreferenceSnapshot.Defaults(), PreferenceStatus.UnsupportedVersion,
                PreferencePersistence.SessionOnly, {"preferences.legacy-version.unsupported"})
        End Function

        ' Inspect just the root before the legacy store can normalize or recover/rewrite.
        ' DTD processing is prohibited. No XML bytes or exception contents enter diagnostics.
        Private Function ReadVersion() As Integer
            If Not File.Exists(_store.SettingsPath) Then Return 0
            Using stream As New FileStream(_store.SettingsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using reader = XmlReader.Create(stream, New XmlReaderSettings With {.DtdProcessing = DtdProcessing.Prohibit, .XmlResolver = Nothing})
                    reader.MoveToContent()
                    If reader.LocalName <> "BeshStatNgSettings" Then Throw New XmlException("Invalid settings root.")
                    Dim raw = reader.GetAttribute("version")
                    If raw Is Nothing Then Return 3 ' Existing constructor default when attribute absent.
                    Dim version As Integer
                    ' An unrepresentable/unknown version may be newer required data.
                    ' Preserve it rather than treating it as a recoverable corrupt body.
                    If Not Integer.TryParse(raw, Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture, version) Then Return Integer.MaxValue
                    Return version
                End Using
            End Using
        End Function

        Public Function Load() As PreferenceResult
            Try
                Dim version As Integer
                Dim malformed As Boolean = False
                Try
                    version = ReadVersion()
                Catch ex As XmlException
                    malformed = True ' Retain established legacy recovery for malformed XML.
                End Try
                If Not malformed AndAlso File.Exists(_store.SettingsPath) AndAlso (version < 1 OrElse version > 3) Then Return Unsupported()
                Dim before As PreferenceResult = Nothing
                If File.Exists(_store.SettingsPath) AndAlso Not malformed Then
                    Try
                        before = FromLegacy(ReadRaw())
                    Catch ex As InvalidOperationException
                        malformed = True ' XmlSerializer failure follows the existing recovery path.
                    Catch ex As XmlException
                        malformed = True
                    End Try
                End If
                Dim result = FromLegacy(_store.Load())
                Dim status = If(malformed OrElse version = 0, PreferenceStatus.Defaulted,
                    If(version < 3, PreferenceStatus.Migrated, If(before Is Nothing, result.Status, before.Status)))
                ' Recovery can return defaults even when its write failed. Verify saved equivalence
                ' without another legacy Load (which could write again).
                Dim persisted As Boolean = False
                Try
                    Dim raw = ReadRaw()
                    Dim check = FromLegacy(raw)
                    persisted = raw.Version = 3 AndAlso check.Status = PreferenceStatus.Loaded AndAlso SameSnapshot(check.Snapshot, result.Snapshot)
                Catch ex As Exception
                    persisted = False
                End Try
                Return New PreferenceResult(result.Snapshot, status, If(persisted, PreferencePersistence.Persisted,
                    PreferencePersistence.SessionOnly), If(before Is Nothing, result.Codes, before.Codes))
            Catch ex As Exception
                Return New PreferenceResult(PreferenceSnapshot.Defaults(), PreferenceStatus.StorageUnavailable,
                    PreferencePersistence.Unavailable, {"preferences.load.failed"})
            End Try
        End Function

        Private Function ReadRaw() As BeshStatNgSettings
            Dim serializer As New System.Xml.Serialization.XmlSerializer(GetType(BeshStatNgSettings))
            Using reader = XmlReader.Create(_store.SettingsPath, New XmlReaderSettings With {.DtdProcessing = DtdProcessing.Prohibit, .XmlResolver = Nothing})
                Return DirectCast(serializer.Deserialize(reader), BeshStatNgSettings)
            End Using
        End Function

        Private Shared Function SameSnapshot(left As PreferenceSnapshot, right As PreferenceSnapshot) As Boolean
            Return left.DefaultAlpha = right.DefaultAlpha AndAlso left.DefaultRandomSeed = right.DefaultRandomSeed AndAlso
                left.TraceEnabled = right.TraceEnabled AndAlso left.DecimalPlaces = right.DecimalPlaces AndAlso
                left.SmallValueDisplay = right.SmallValueDisplay AndAlso left.UseUpperBound = right.UseUpperBound
        End Function

        Public Function Save(snapshot As PreferenceSnapshot) As PreferenceResult
            If snapshot Is Nothing Then Throw New ArgumentNullException(NameOf(snapshot))
            Try
                Dim version As Integer
                Dim malformed As Boolean = False
                Try
                    version = ReadVersion()
                Catch ex As XmlException
                    malformed = True ' An intentional save may replace malformed historical data.
                End Try
                If Not malformed AndAlso File.Exists(_store.SettingsPath) AndAlso (version < 1 OrElse version > 3) Then Return Unsupported()
                _store.Save(ToLegacy(snapshot))
                Return New PreferenceResult(snapshot, PreferenceStatus.Saved, PreferencePersistence.Persisted)
            Catch ex As Exception
                Return New PreferenceResult(snapshot, PreferenceStatus.StorageUnavailable,
                    PreferencePersistence.Unavailable, {"preferences.save.failed"})
            End Try
        End Function

        Public Function LoadAsync() As Task(Of PreferenceResult) Implements IPreferenceStore.LoadAsync
            Return Task.FromResult(Load())
        End Function
        Public Function SaveAsync(snapshot As PreferenceSnapshot) As Task(Of PreferenceResult) Implements IPreferenceStore.SaveAsync
            Return Task.FromResult(Save(snapshot))
        End Function
    End Class
End Namespace
