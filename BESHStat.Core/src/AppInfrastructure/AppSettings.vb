Option Explicit On
Option Strict On
Imports System.IO
Imports System.Diagnostics
Imports System.Text
Imports System.Xml.Serialization

Namespace AppInfrastructure

    <XmlRoot("BeshStatNgSettings")>
    Public Class BeshStatNgSettings

        Public Sub New()
            Version = 3
            Diagnostics = New DiagnosticsSettings()
            PValuePresentation = New PValuePresentationSettings()
            DefaultAlpha = 0.05
            DefaultRandomSeed = Integer.MinValue
        End Sub

        <XmlAttribute("version")>
        Public Property Version As Integer

        Public Property Diagnostics As DiagnosticsSettings

        ''' <summary>
        ''' Controls how p-values are displayed in generated result tables. These settings affect
        ''' presentation only; the underlying numeric values retain their full precision.
        ''' </summary>
        Public Property PValuePresentation As PValuePresentationSettings

        ''' <summary>
        ''' Default two-sided significance level used to initialize UI alpha controls.
        ''' Also used as the default threshold for p-value highlighting and similar decision rules.
        ''' </summary>
        Public Property DefaultAlpha As Double

        ''' <summary>
        ''' Global default pseudo-random seed used by workflows that support stochastic behavior but do not expose
        ''' a dedicated seed input on their own form. The sentinel <see cref="Integer.MinValue"/> means to use a time-based seed.
        ''' </summary>
        Public Property DefaultRandomSeed As Integer

        Public Sub EnsureDefaults()
            If Diagnostics Is Nothing Then Diagnostics = New DiagnosticsSettings()

            If PValuePresentation Is Nothing Then PValuePresentation = New PValuePresentationSettings()
            PValuePresentation.EnsureDefaults()

            If Version < 3 Then Version = 3

            If DefaultAlpha <= 0.0 OrElse DefaultAlpha >= 1.0 Then
                DefaultAlpha = 0.05
            End If
            
            if DefaultRandomSeed = 0 then DefaultRandomSeed = Integer.MinValue
        End Sub
    End Class

    Public Class DiagnosticsSettings

        Public Sub New()
            TraceExecutionLoggingEnabled = True
        End Sub

        Public Property TraceExecutionLoggingEnabled As Boolean
    End Class

    ''' <summary>
    ''' Determines how a p-value smaller than the fixed-decimal display threshold is presented.
    ''' </summary>
    Public Enum PValueSmallValueDisplay
        LessThanThreshold = 0
        ScientificNotation = 1
        FixedDecimal = 2
    End Enum

    ''' <summary>
    ''' Persisted global options for p-value presentation.
    ''' </summary>
    Public Class PValuePresentationSettings

        Public Const MinimumDecimalPlaces As Integer = 2
        Public Const MaximumDecimalPlaces As Integer = 16
        Public Const DefaultDecimalPlaces As Integer = 8

        Public Sub New()
            DecimalPlaces = DefaultDecimalPlaces
            SmallValueDisplay = PValueSmallValueDisplay.LessThanThreshold
            UseUpperBound = True
        End Sub

        Public Property DecimalPlaces As Integer

        Public Property SmallValueDisplay As PValueSmallValueDisplay

        Public Property UseUpperBound As Boolean

        Public Sub EnsureDefaults()
            If DecimalPlaces < MinimumDecimalPlaces OrElse DecimalPlaces > MaximumDecimalPlaces Then
                DecimalPlaces = DefaultDecimalPlaces
            End If

            If Not [Enum].IsDefined(GetType(PValueSmallValueDisplay), SmallValueDisplay) Then
                SmallValueDisplay = PValueSmallValueDisplay.LessThanThreshold
            End If
        End Sub
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
                Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Warn($"Failed to load settings file '{_settingsPath}'. Defaults will be recreated. {ex.Message}")

                Dim defaults = CreateDefault()

                Try
                    Save(defaults)
                    Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Info($"Default settings file recreated at '{_settingsPath}'.")
                Catch saveEx As Exception
                    Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Error(saveEx, $"Failed to recreate default settings file '{_settingsPath}'.")
                End Try

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