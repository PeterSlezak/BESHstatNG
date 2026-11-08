Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.Multivariate
Namespace Global.Frozen37
Public Class OriginalSeries37
    'Pure inclusion/order/array assignments transcribed from the archived CreateExcelChart.
    'Source ordinal is the original cluster slot, independent of the plotted series position.
    Public Shared Function DendrogramSeries(layout As DendrogramLayout, labelMode As DendrogramLabelMode) As Object
        Dim result As New List(Of Object)
        result.Add(New Object(){"Dendrogram", layout.PolylineX, layout.PolylineY, -1})
        If layout.ClusterPolylineX IsNot Nothing AndAlso layout.ClusterPolylineY IsNot Nothing Then
            Dim nColored As Integer = Math.Min(layout.ClusterPolylineX.Count, layout.ClusterPolylineY.Count)
            For i As Integer = 0 To nColored - 1
                If layout.ClusterPolylineX(i) Is Nothing OrElse layout.ClusterPolylineY(i) Is Nothing Then Continue For
                If layout.ClusterPolylineX(i).Length = 0 OrElse layout.ClusterPolylineY(i).Length = 0 Then Continue For
                result.Add(New Object(){$"Cluster {i + 1}", layout.ClusterPolylineX(i), layout.ClusterPolylineY(i), i})
            Next
        End If
        If layout.CutLineX IsNot Nothing AndAlso layout.CutLineY IsNot Nothing AndAlso layout.CutLineX.Length >= 2 AndAlso layout.CutLineY.Length >= 2 Then
            result.Add(New Object(){"Cut", layout.CutLineX, layout.CutLineY, -1})
        End If
        If labelMode = DendrogramLabelMode.DataLabels AndAlso layout.LeafX IsNot Nothing AndAlso layout.LeafLabels IsNot Nothing AndAlso layout.LeafLabels.Length > 0 Then
            result.Add(New Object(){"LeafLabels", layout.LeafX, layout.LeafY, layout.LeafLabels, layout.LeafOriginalRowOrder, CInt(layout.Orientation)})
        End If
        Return result.ToArray()
    End Function
    Public Shared Function FittedLayout(id As Integer) As DendrogramLayout
        Dim model As New HierarchicalClustering
        model.dataInputs(New Double(,){{0,0},{0.5,0},{5,4},{6,5},{9,1}}, {"dup","dup"," ","μ","long label"}, {"x","y"})
        model.settingsInputs(linkage:=HierarchicalLinkageMethod.Complete, distanceMetric:=HierarchicalDistanceMetric.Euclidean)
        model.Fit()
        Return model.Result.CreateDendrogramLayout(CType((id \ 4) Mod 2,DendrogramHeightMode), CType(id Mod 4,DendrogramOrientation), CType((id \ 8) Mod 2,HierarchicalMembershipDisplayMode), 1 + (id \ 16) Mod 5, New Double(){0,1,5,100}((id \ 16) Mod 4))
    End Function
End Class
End Namespace
