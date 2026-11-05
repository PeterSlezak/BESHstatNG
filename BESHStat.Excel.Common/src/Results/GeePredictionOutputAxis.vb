Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    ''' <summary>Observation of distinct existing axes; does not change output order or fit a model.</summary>
    Public NotInheritable Class GeePredictionOutputRow
        Friend Sub New(predictionPosition As Integer, inputPosition As Integer, inputRowId As Integer, clusterValue As Object)
            Me.PredictionPosition = predictionPosition
            Me.InputPosition = inputPosition
            Me.InputRowId = inputRowId
            Me.ClusterValue = clusterValue
        End Sub
        Public ReadOnly Property PredictionPosition As Integer
        Public ReadOnly Property InputPosition As Integer
        ''' <summary>Retained Core/input identity, not a manufactured worksheet coordinate.</summary>
        Public ReadOnly Property InputRowId As Integer
        Public ReadOnly Property ClusterValue As Object
    End Class

    ''' <summary>
    ''' Detached review descriptor only. Prediction positions are Core cluster-concatenated;
    ''' input positions are the axis used by the existing Predictions policy. No new GUI
    ''' sorting, text-cluster interpretation, worksheet row label or correction is activated.
    ''' </summary>
    Public NotInheritable Class GeePredictionOutputAxis
        Private Sub New()
        End Sub
        Public Shared Function Describe(intent As GeeWorkflowIntent) As IReadOnlyList(Of GeePredictionOutputRow)
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            Dim rows = intent.MeanRequest.Primary.RowIds
            Dim clusters = intent.Cluster.Values
            Dim order = intent.Order.ClusteredToInputIndices
            Dim result As New List(Of GeePredictionOutputRow)()
            For prediction As Integer = 0 To order.Length - 1
                Dim input As Integer = order(prediction)
                result.Add(New GeePredictionOutputRow(prediction + 1, input + 1, rows(input), clusters(input)))
            Next
            Return result.AsReadOnly()
        End Function
    End Class
End Namespace
