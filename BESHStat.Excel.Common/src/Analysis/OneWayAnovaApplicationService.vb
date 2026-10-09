Option Explicit On
Option Strict On

Namespace ExcelCommon

    ''' <summary>
    ''' Application-layer result for the shared one-way ANOVA workflow.
    ''' </summary>
    Public Class OneWayAnovaApplicationResult

        Public Sub New(table As Global.BESHStatNG.ResultTableOutputModel,
                       fStatistic As Double,
                       pValue As Double,
                       groupNames As String(),
                       groupCounts As Integer())
            Me.Table = table
            Me.FStatistic = fStatistic
            Me.PValue = pValue
            Me.GroupNames = groupNames
            Me.GroupCounts = groupCounts
        End Sub

        Public ReadOnly Property Table As Global.BESHStatNG.ResultTableOutputModel
        Public ReadOnly Property FStatistic As Double
        Public ReadOnly Property PValue As Double
        Public ReadOnly Property GroupNames As String()
        Public ReadOnly Property GroupCounts As Integer()
    End Class

    ''' <summary>
    ''' Shared application service joining spreadsheet-neutral grouped input to BESHStat.Core.
    ''' Both a Windows Excel-DNA adapter and an Office.js/WASM adapter can call this same service.
    ''' </summary>
    Public NotInheritable Class OneWayAnovaApplicationService

        Private Sub New()
        End Sub

        Public Shared Function Run(input As SpreadsheetRangeData) As OneWayAnovaApplicationResult
            Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(input)

            Dim model As New Global.BESHStatNG.parametric.OneWayANOVA(grouped.Groups, grouped.GroupNames)
            Dim raw As Object(,) = model.compute()
            Dim tables As System.Collections.Generic.List(Of Global.BESHStatNG.ResultTable) = model.wrapResults()

            If tables Is Nothing OrElse tables.Count = 0 Then
                Throw New InvalidOperationException("One-way ANOVA did not produce a ResultTable.")
            End If

            Dim output As Global.BESHStatNG.ResultTableOutputModel = tables(0).ToOutputModel()
            Dim fStatistic As Double = Convert.ToDouble(raw(0, 3), Globalization.CultureInfo.InvariantCulture)
            Dim pValue As Double = Convert.ToDouble(raw(0, 4), Globalization.CultureInfo.InvariantCulture)

            Return New OneWayAnovaApplicationResult(
                output,
                fStatistic,
                pValue,
                DirectCast(grouped.GroupNames.Clone(), String()),
                grouped.GroupCounts)
        End Function
    End Class

End Namespace
