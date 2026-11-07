Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace Global.Batch34NativeStateFixtures
Public Module LegacyNativeState34
    Public Function ShouldKeepLegendEntry(observation As LadderPlotObservation,
                                                  observationIndex As Integer,
                                                  appearance As LadderPlotAppearance,
                                                  seenLegendGroups As HashSet(Of Integer)) As Boolean
        If Not appearance.ShowLegend Then Return False

        Select Case appearance.ColorMode
            Case LadderColorMode.SingleColor
                Return observationIndex = 0
            Case LadderColorMode.ByObservation
                Return True
            Case LadderColorMode.ByGroup
                If seenLegendGroups.Add(observation.GroupLevelIndex) Then Return True
                Return False
            Case Else
                Return observationIndex = 0
        End Select
    End Function
    Public Function FormatEndpointValue(value As Double,
                                                axisValueKind As DumbbellAxisValueKind,
                                                requestedFormat As String) As String
        If axisValueKind = DumbbellAxisValueKind.DateValue Then
            Try
                Dim dt As DateTime = DateTime.FromOADate(value)
                Dim fmt As String = If(String.IsNullOrWhiteSpace(requestedFormat), "dd-MMM-yyyy", requestedFormat.Trim())
                Return dt.ToString(fmt, CultureInfo.CurrentCulture)
            Catch
                Return value.ToString("G", CultureInfo.CurrentCulture)
            End Try
        End If

        If Not String.IsNullOrWhiteSpace(requestedFormat) Then
            Try
                Return value.ToString(requestedFormat.Trim(), CultureInfo.CurrentCulture)
            Catch
            End Try
        End If
        Return value.ToString("0.###", CultureInfo.CurrentCulture)
    End Function
End Module
End Namespace
