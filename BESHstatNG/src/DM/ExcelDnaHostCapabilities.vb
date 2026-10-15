Option Explicit On
Option Strict On

Imports BESHStatNG.ExcelCommon

''' <summary>
''' Windows Excel-DNA host capability adapter. This reports only operations already
''' implemented by the existing COM import/output flows. It does not probe or open Excel.
''' </summary>
Public NotInheritable Class ExcelDnaHostCapabilities
    Private Sub New()
    End Sub

    Public Shared Function GetSnapshot() As HostCapabilitySnapshot
        Return HostCapabilityProfiles.WindowsExcelDna()
    End Function

    ''' <summary>Validate output before the first shared output write.</summary>
    Public Shared Sub EnsureCanWrite(analysisId As String, target As SpreadsheetOutputTarget)
        HostCapabilityPreflight.CheckRunAndWrite(analysisId, GetSnapshot(), target, False).EnsureSupported()
    End Sub
End Class
