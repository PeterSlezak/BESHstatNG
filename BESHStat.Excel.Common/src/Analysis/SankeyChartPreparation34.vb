Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Namespace ExcelCommon.Chart34
Public Enum SankeyPercentageBasis
    ''' <summary>
    ''' Uses <see cref="SankeyPlotResult.ReferenceTotal"/>, i.e. the largest stage total.
    ''' This is useful when the chart represents one common overall population.
    ''' </summary>
    ReferenceTotal

    ''' <summary>
    ''' Uses the sum of node values in the node's own stage.
    ''' </summary>
    StageTotal
End Enum
Public Enum SankeyNodeLabelMode
    NameOnly
    NameAndValue
    NameAndPercentage
    NamePercentageAndValue
End Enum
Public NotInheritable Class SankeyChartSettings34
Public ReadOnly Property ValueNumberFormat As String
Public ReadOnly Property PercentageBasis As SankeyPercentageBasis
Public ReadOnly Property PercentageNumberFormat As String
Public ReadOnly Property NodeLabelMode As SankeyNodeLabelMode
Public Sub New(Optional inputValueNumberFormat As String = "0.###", Optional inputPercentageBasis As SankeyPercentageBasis = SankeyPercentageBasis.ReferenceTotal, Optional inputPercentageNumberFormat As String = "0%", Optional inputNodeLabelMode As SankeyNodeLabelMode = SankeyNodeLabelMode.NameOnly)
Me.ValueNumberFormat = inputValueNumberFormat
Me.PercentageBasis = inputPercentageBasis
Me.PercentageNumberFormat = inputPercentageNumberFormat
Me.NodeLabelMode = inputNodeLabelMode
End Sub
End Class
Public Module SankeyChartPreparation34


    Public Function FormatNodeLabel(node As SankeyNode,
                                            result As SankeyPlotResult,
                                            appearance As SankeyChartSettings34) As String
        Dim valueText As String = node.Value.ToString(appearance.ValueNumberFormat, CultureInfo.CurrentCulture)
        Dim denominator As Double

        Select Case appearance.PercentageBasis
            Case SankeyPercentageBasis.ReferenceTotal
                denominator = result.ReferenceTotal
            Case SankeyPercentageBasis.StageTotal
                Dim totals As Double() = result.StageTotals
                If node.StageIndex >= 0 AndAlso node.StageIndex < totals.Length Then
                    denominator = totals(node.StageIndex)
                End If
        End Select

        Dim percentage As Double = If(denominator > 0.0R, node.Value / denominator, 0.0R)
        Dim percentageText As String = percentage.ToString(appearance.PercentageNumberFormat, CultureInfo.CurrentCulture)

        Select Case appearance.NodeLabelMode
            Case SankeyNodeLabelMode.NameOnly
                Return node.Label
            Case SankeyNodeLabelMode.NameAndValue
                Return node.Label & " (" & valueText & ")"
            Case SankeyNodeLabelMode.NameAndPercentage
                Return node.Label & " " & percentageText
            Case SankeyNodeLabelMode.NamePercentageAndValue
                Return node.Label & " " & percentageText & " (" & valueText & ")"
            Case Else
                Return node.Label
        End Select
    End Function
End Module
End Namespace
