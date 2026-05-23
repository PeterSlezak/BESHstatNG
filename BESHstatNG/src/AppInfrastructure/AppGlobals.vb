Option Explicit On
Option Strict On

Imports System.Linq
Imports System.Reflection
Imports Excel = Microsoft.Office.Interop.Excel

Namespace AppInfrastructure

    ''' <summary>
    ''' Excel add-in facade for application constants, settings, and legacy global accessors.
    ''' Host-neutral logging/error/progress abstractions now live in CoreInfrastructure.vb.
    ''' Excel-DNA/WinForms-specific services live in ExcelDnaHostInfrastructure.vb.
    ''' </summary>
    Public Module AppGlobals

        Public Const gsAPP_TITLE As String = "BESH-Stat-NG"
        Public Const gCheckUpdateUrl As String = "http://beshstat.eu/wp-content/uploads/addinversion/versionNG.xml"

        Public Property gXllName As String
            Get
                Return ExcelDnaHost.XllName
            End Get
            Set(value As String)
                ExcelDnaHost.XllName = value
            End Set
        End Property

        Public Property gXllPath As String
            Get
                Return ExcelDnaHost.XllPath
            End Get
            Set(value As String)
                ExcelDnaHost.XllPath = value
            End Set
        End Property

        Public Property app As Excel.Application
            Get
                Return ExcelDnaHost.ExcelApplication
            End Get
            Set(value As Excel.Application)
                ExcelDnaHost.ExcelApplication = value
            End Set
        End Property

        Public gLogFile As String
        Public gSettingsStore As BeshStatNgSettingsStore
        Public gSettings As BeshStatNgSettings

        Public Property gLogger As IAppLogger
            Get
                Return CoreServices.Logger
            End Get
            Set(value As IAppLogger)
                CoreServices.Logger = value
            End Set
        End Property

        Public ReadOnly Property gAddinVersion As String
            Get
                Dim v = Assembly.GetExecutingAssembly().GetName().Version
                Return v.ToString()
            End Get
        End Property

        Public Function GetBuildDateIso() As String
            Dim asm = Assembly.GetExecutingAssembly()
            Dim meta = asm.GetCustomAttributes(GetType(AssemblyMetadataAttribute), False) _
                      .OfType(Of AssemblyMetadataAttribute)() _
                      .FirstOrDefault(Function(a) a.Key = "BuildDate")

            Return If(meta?.Value, "")
        End Function

        Public ReadOnly Property ExcelMainHwnd As IntPtr
            Get
                Return ExcelDnaHost.ExcelMainHwnd
            End Get
        End Property

        Public Function GetCurrentSettings() As BeshStatNgSettings
            If gSettings Is Nothing Then
                gSettings = New BeshStatNgSettings()
                gSettings.EnsureDefaults()
                ConfigureAnalysisDefaults(gSettings)
            End If

            Return gSettings
        End Function

        Private Sub ConfigureAnalysisDefaults(settings As BeshStatNgSettings)
            If settings Is Nothing Then settings = New BeshStatNgSettings()
            settings.EnsureDefaults()
            CoreServices.AnalysisDefaults = New AnalysisDefaultsOptions(settings.DefaultAlpha, settings.DefaultRandomSeed)
        End Sub

        Public ReadOnly Property TraceExecutionLoggingEnabled As Boolean
            Get
                Return GetCurrentSettings().Diagnostics.TraceExecutionLoggingEnabled
            End Get
        End Property

        Public ReadOnly Property DefaultAlpha As Double
            Get
                Return CoreServices.AnalysisDefaults.DefaultAlpha
            End Get
        End Property

        Public Function GetDefaultAlphaDecimal(minimum As Decimal, maximum As Decimal) As Decimal
            Dim value As Decimal = CDec(DefaultAlpha)
            If value < minimum Then value = minimum
            If value > maximum Then value = maximum
            Return value
        End Function

        Public ReadOnly Property DefaultRandomSeed As Integer
            Get
                Return CoreServices.AnalysisDefaults.DefaultRandomSeed
            End Get
        End Property

        Public Function GetDefaultRandomSeedText() As String
            Dim seed = DefaultRandomSeed
            If seed = Integer.MinValue Then Return String.Empty
            Return seed.ToString(Globalization.CultureInfo.InvariantCulture)
        End Function

        Public Function CreateRandom(Optional explicitSeed As Integer = Integer.MinValue) As Random
            Return CoreServices.AnalysisDefaults.CreateRandom(explicitSeed)
        End Function

        Public Sub ApplySettings(settings As BeshStatNgSettings)
            If settings Is Nothing Then settings = New BeshStatNgSettings()
            settings.EnsureDefaults()
            gSettings = settings
            ConfigureAnalysisDefaults(settings)
            CoreServices.TraceExecutionLoggingEnabled = settings.Diagnostics.TraceExecutionLoggingEnabled
        End Sub

    End Module
End Namespace