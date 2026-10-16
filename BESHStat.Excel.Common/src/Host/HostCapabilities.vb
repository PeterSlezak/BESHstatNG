Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>Stable capability keys shared by Office.js, Windows and Common.</summary>
    ''' <remarks>Capability IDs describe an actual host operation, not a statistical algorithm.</remarks>
    Public NotInheritable Class HostCapabilityIds
        Public Const ReadSelectedRange As String = "input.read-selected-range"
        Public Const WriteResultTables As String = "output.write-result-tables"
        Public Const CurrentWorksheet As String = "output.current-worksheet"
        Public Const NewWorksheet As String = "output.new-worksheet"
        Public Const NewWorkbook As String = "output.new-workbook"

        Private Sub New()
        End Sub

        Public Shared Function ForDestination(destination As SpreadsheetOutputDestination) As String
            Select Case destination
                Case SpreadsheetOutputDestination.CurrentWorksheet
                    Return CurrentWorksheet
                Case SpreadsheetOutputDestination.NewWorksheet
                    Return NewWorksheet
                Case SpreadsheetOutputDestination.NewWorkbook
                    Return NewWorkbook
                Case Else
                    Return Nothing
            End Select
        End Function
    End Class

    ''' <summary>Serializable host report. A host must advertise only capabilities it implements.</summary>
    ''' <remarks>Transport DTO is mutable for Office.js JSON interop. Preflight never mutates it.</remarks>
    Public NotInheritable Class HostCapabilitySnapshot
        Public Sub New()
            Host = String.Empty
            Platform = String.Empty
            Capabilities = New List(Of String)()
        End Sub

        Public Property Host As String
        Public Property Platform As String
        ''' <summary>Compatibility indicator: True only for an Excel host. Existing JSON payloads retain this field.</summary>
        Public Property IsExcel As Boolean
        ''' <summary>
        ''' Explicit spreadsheet-host declaration. Non-Excel hosts must set this to True;
        ''' pre-01.05A Excel reports continue to work through IsExcel=True.
        ''' This is an admission flag, not an assertion that any operation is available.
        ''' </summary>
        Public Property IsSpreadsheetHost As Boolean
        Public Property Capabilities As List(Of String)

        Friend ReadOnly Property IsUsableSpreadsheetHost As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(Host) AndAlso (IsExcel OrElse IsSpreadsheetHost)
            End Get
        End Property

        Public Function Supports(capabilityId As String) As Boolean
            If Not IsUsableSpreadsheetHost OrElse String.IsNullOrWhiteSpace(capabilityId) OrElse Capabilities Is Nothing Then Return False
            For Each item As String In Capabilities
                If String.Equals(item, capabilityId, StringComparison.Ordinal) Then Return True
            Next
            Return False
        End Function
    End Class

    ''' <summary>Known host profiles; Office.js itself must perform runtime feature detection.</summary>
    Public NotInheritable Class HostCapabilityProfiles
        Private Sub New()
        End Sub

        Public Shared Function WindowsExcelDna() As HostCapabilitySnapshot
            Return New HostCapabilitySnapshot With {
                .Host = "ExcelDna",
                .Platform = "Windows",
                .IsExcel = True,
                .IsSpreadsheetHost = True,
                .Capabilities = New List(Of String) From {
                    HostCapabilityIds.ReadSelectedRange,
                    HostCapabilityIds.WriteResultTables,
                    HostCapabilityIds.CurrentWorksheet,
                    HostCapabilityIds.NewWorksheet,
                    HostCapabilityIds.NewWorkbook
                }
            }
        End Function

        ''' <summary>Documented Office prototype capability set, for tests/diagnostics only.
        ''' Production Office uses the JS adapter's runtime-detected snapshot.</summary>
        Public Shared Function OfficeJsExcel(platform As String) As HostCapabilitySnapshot
            Return New HostCapabilitySnapshot With {
                .Host = "OfficeJs",
                .Platform = If(platform, String.Empty),
                .IsExcel = True,
                .IsSpreadsheetHost = True,
                .Capabilities = New List(Of String) From {
                    HostCapabilityIds.ReadSelectedRange,
                    HostCapabilityIds.WriteResultTables,
                    HostCapabilityIds.CurrentWorksheet,
                    HostCapabilityIds.NewWorksheet
                }
            }
        End Function
    End Class

    Public NotInheritable Class CapabilityPreflightCodes
        Public Const HostUnavailable As String = "host.unavailable"
        Public Const UnknownAnalysis As String = "analysis.unknown"
        Public Const UnsupportedAnalysis As String = "analysis.unsupported"
        Public Const MissingCapability As String = "host.capability-missing"
        Public Const UnsupportedDestination As String = "output.destination-unsupported"
        Public Const InvalidTarget As String = "output.target-invalid"

        Private Sub New()
        End Sub
    End Class

    ''' <summary>Preflight result independent of a host UI/error-reporting implementation.</summary>
    Public NotInheritable Class CapabilityPreflightResult
        Private Sub New(isSupported As Boolean, code As String, message As String, missingCapability As String)
            Me.IsSupported = isSupported
            Me.Code = code
            Me.Message = message
            Me.MissingCapability = missingCapability
        End Sub

        Public ReadOnly Property IsSupported As Boolean
        Public ReadOnly Property Code As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property MissingCapability As String

        Public Shared Function Success() As CapabilityPreflightResult
            Return New CapabilityPreflightResult(True, String.Empty, String.Empty, String.Empty)
        End Function

        Public Shared Function Rejected(code As String, message As String, Optional missingCapability As String = Nothing) As CapabilityPreflightResult
            Return New CapabilityPreflightResult(False, code, message, If(missingCapability, String.Empty))
        End Function

        Public Sub EnsureSupported()
            If Not IsSupported Then Throw New HostCapabilityException(Me)
        End Sub
    End Class

    Public NotInheritable Class HostCapabilityException
        Inherits NotSupportedException

        Public Sub New(result As CapabilityPreflightResult)
            MyBase.New(If(result Is Nothing, "Preflight result is required.", result.Message))
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
            Code = result.Code
            MissingCapability = result.MissingCapability
        End Sub

        Public ReadOnly Property Code As String
        Public ReadOnly Property MissingCapability As String
    End Class

    ''' <summary>Read-only, side-effect-free capability evaluation performed before host operations.</summary>
    Public NotInheritable Class HostCapabilityPreflight
        Private Sub New()
        End Sub

        Private Shared Function CheckHost(snapshot As HostCapabilitySnapshot) As CapabilityPreflightResult
            If snapshot Is Nothing OrElse Not snapshot.IsUsableSpreadsheetHost Then
                Return CapabilityPreflightResult.Rejected(CapabilityPreflightCodes.HostUnavailable,
                    "An identified spreadsheet host is required.")
            End If
            Return CapabilityPreflightResult.Success()
        End Function

        Private Shared Function Require(snapshot As HostCapabilitySnapshot, capabilityId As String, operationName As String,
                                        Optional destination As Boolean = False) As CapabilityPreflightResult
            If snapshot.Supports(capabilityId) Then Return CapabilityPreflightResult.Success()
            Return CapabilityPreflightResult.Rejected(
                If(destination, CapabilityPreflightCodes.UnsupportedDestination, CapabilityPreflightCodes.MissingCapability),
                operationName & " is not supported by the current spreadsheet host.", capabilityId)
        End Function

        Public Shared Function CheckSelection(snapshot As HostCapabilitySnapshot) As CapabilityPreflightResult
            Dim check As CapabilityPreflightResult = CheckHost(snapshot)
            If Not check.IsSupported Then Return check
            Return Require(snapshot, HostCapabilityIds.ReadSelectedRange, "Read selected range")
        End Function

        Public Shared Function CheckAnalysis(analysisId As String, snapshot As HostCapabilitySnapshot) As CapabilityPreflightResult
            Dim descriptor As IAnalysisDescriptor = Nothing
            If Not AnalysisCatalog.TryGetById(analysisId, descriptor) Then
                Return CapabilityPreflightResult.Rejected(CapabilityPreflightCodes.UnknownAnalysis, "Unknown analysis ID.")
            End If
            If descriptor.Availability <> AnalysisAvailability.ImplementedInCommon Then
                Return CapabilityPreflightResult.Rejected(CapabilityPreflightCodes.UnsupportedAnalysis, "Analysis is not implemented in Common.")
            End If
            Dim check As CapabilityPreflightResult = CheckHost(snapshot)
            If Not check.IsSupported Then Return check
            For Each requiredCapability As String In descriptor.RequiredCapabilities
                check = Require(snapshot, requiredCapability, "Analysis '" & descriptor.DisplayName & "'")
                If Not check.IsSupported Then Return check
            Next
            Return CapabilityPreflightResult.Success()
        End Function

        Public Shared Function CheckOutput(snapshot As HostCapabilitySnapshot,
                                           target As SpreadsheetOutputTarget) As CapabilityPreflightResult
            Dim check As CapabilityPreflightResult = CheckHost(snapshot)
            If Not check.IsSupported Then Return check
            If target Is Nothing OrElse target.StartRow < 1 OrElse target.StartColumn < 1 Then
                Return CapabilityPreflightResult.Rejected(CapabilityPreflightCodes.InvalidTarget,
                    "Output target requires positive start row and column.")
            End If
            Dim requiredDestination As String = HostCapabilityIds.ForDestination(target.Destination)
            If requiredDestination Is Nothing Then
                Return CapabilityPreflightResult.Rejected(CapabilityPreflightCodes.InvalidTarget,
                    "Unknown output destination.")
            End If
            check = Require(snapshot, HostCapabilityIds.WriteResultTables, "Write result tables")
            If Not check.IsSupported Then Return check
            Return Require(snapshot, requiredDestination, "Selected output destination", True)
        End Function

        Public Shared Function CheckRunAndWrite(analysisId As String,
                                                snapshot As HostCapabilitySnapshot,
                                                target As SpreadsheetOutputTarget,
                                                Optional readsSelection As Boolean = True) As CapabilityPreflightResult
            Dim check As CapabilityPreflightResult = CheckAnalysis(analysisId, snapshot)
            If Not check.IsSupported Then Return check
            If readsSelection Then
                check = CheckSelection(snapshot)
                If Not check.IsSupported Then Return check
            End If
            Return CheckOutput(snapshot, target)
        End Function
    End Class
End Namespace
