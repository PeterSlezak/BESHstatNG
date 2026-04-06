Option Explicit On
Imports System.IO
Imports System.Diagnostics
Imports System.Text
Imports System.Xml.Serialization

Namespace AppInfrastructure

    <XmlRoot("BeshStatNgSettings")>
    Public Class BeshStatNgSettings

        Public Sub New()
            Version = 2
            Diagnostics = New DiagnosticsSettings()
            DefaultAlpha = 0.05
        End Sub

        <XmlAttribute("version")>
        Public Property Version As Integer

        Public Property Diagnostics As DiagnosticsSettings

        ''' <summary>
        ''' Default two-sided significance level used to initialize UI alpha controls.
        ''' Also used as the default threshold for p-value highlighting and similar decision rules.
        ''' </summary>
        Public Property DefaultAlpha As Double

        Public Sub EnsureDefaults()
            If Diagnostics Is Nothing Then Diagnostics = New DiagnosticsSettings()

            If DefaultAlpha <= 0.0 OrElse DefaultAlpha >= 1.0 Then
                DefaultAlpha = 0.05
            End If
        End Sub
    End Class

    Public Class DiagnosticsSettings

        Public Sub New()
            TraceExecutionLoggingEnabled = True
        End Sub

        Public Property TraceExecutionLoggingEnabled As Boolean
    End Class

    Public Class BeshStatNgSettingsStore

        Private ReadOnly _settingsPath As String

        Public Sub New(baseDirectory As String)
            If String.IsNullOrWhiteSpace(baseDirectory) Then
                Throw New ArgumentException("baseDirectory must not be empty.", NameOf(baseDirectory))
            End If

            _settingsPath = Path.Combine(baseDirectory, "BESHStatNG.settings.xml")
        End Sub

        Public ReadOnly Property SettingsPath As String
            Get
                Return _settingsPath
            End Get
        End Property

        Public Function Load() As BeshStatNgSettings
            If Not File.Exists(_settingsPath) Then
                Dim defaults = CreateDefault()
                Save(defaults)
                Return defaults
            End If

            Try
                Dim serializer As New XmlSerializer(GetType(BeshStatNgSettings))

                Using fs As New FileStream(_settingsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Dim settings = DirectCast(serializer.Deserialize(fs), BeshStatNgSettings)
                    If settings Is Nothing Then settings = CreateDefault()
                    settings.EnsureDefaults()
                    Return settings
                End Using
            Catch ex As Exception
                Debug.WriteLine("Failed to load settings file: " & ex.Message)

                Dim defaults = CreateDefault()
                Save(defaults)
                Return defaults
            End Try
        End Function

        Public Sub Save(settings As BeshStatNgSettings)
            If settings Is Nothing Then
                Throw New ArgumentNullException(NameOf(settings))
            End If

            settings.EnsureDefaults()

            Dim settingsDirectory = Path.GetDirectoryName(_settingsPath)
            If Not String.IsNullOrWhiteSpace(settingsDirectory) Then
                System.IO.Directory.CreateDirectory(settingsDirectory)
            End If

            Dim tempPath = _settingsPath & ".tmp"
            Dim serializer As New XmlSerializer(GetType(BeshStatNgSettings))

            Using fs As New FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None)
                Using writer As New StreamWriter(fs, New UTF8Encoding(False))
                    serializer.Serialize(writer, settings)
                End Using
            End Using

            If File.Exists(_settingsPath) Then
                File.Copy(tempPath, _settingsPath, True)
                File.Delete(tempPath)
            Else
                File.Move(tempPath, _settingsPath)
            End If
        End Sub

        Public Shared Function CreateDefault() As BeshStatNgSettings
            Return New BeshStatNgSettings()
        End Function
    End Class

End Namespace