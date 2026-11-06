Imports System.Drawing
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    Public Class WireSphere3D
        Implements IXYZDrawable3D, IXYZHasBounds

        Public Property Cx As Double
        Public Property Cy As Double
        Public Property Cz As Double
        Public Property Diameter As Double

        Public Property LatitudeRings As Integer = 8
        Public Property LongitudeRings As Integer = 12
        Public Property PointsPerRing As Integer = 120

        Public Property ColorR As Integer = 125
        Public Property ColorG As Integer = 0
        Public Property ColorB As Integer = 0

        Public Property SeriesName As String = "WireSphere"

        Private Xs As Double()
        Property ys As Double()

        Public Sub New(cx As Double, cy As Double, cz As Double, diameter As Double)
            Me.Cx = cx : Me.Cy = cy : Me.Cz = cz : Me.Diameter = diameter
        End Sub

        Public Sub GetRawBounds(ByRef minX As Double, ByRef maxX As Double,
                                ByRef minY As Double, ByRef maxY As Double,
                                ByRef minZ As Double, ByRef maxZ As Double) Implements IXYZHasBounds.GetRawBounds
            Dim bounds = WireObjectGeometryCore.SphereBounds(Cx, Cy, Cz, Diameter)
            minX = bounds(0) : maxX = bounds(1) : minY = bounds(2) : maxY = bounds(3) : minZ = bounds(4) : maxZ = bounds(5)
        End Sub

        Public Sub Draw(owner As XYZscatter, figure As Chart) Implements IXYZDrawable3D.Draw
            If figure Is Nothing Then Exit Sub
            If Diameter <= 0 Then Exit Sub

            Dim geometry = WireObjectGeometryCore.Sphere(owner.ProjectionContext(), Cx, Cy, Cz, Diameter, LatitudeRings, LongitudeRings, PointsPerRing, owner.BreakValue)
            Xs = geometry.X : ys = geometry.Y
            Dim s As Series = figure.SeriesCollection.NewSeries()
            With s
                .Name = SeriesName
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                .XValues = Xs
                .Values = ys
                .Border.Weight = XlBorderWeight.xlThin
                .Border.Color = RGB(ColorR, ColorG, ColorB)

                'Make ring separators invisible
                For i = 0 To Xs.Length - 1
                    If Xs(i) = owner.BreakValue Then
                        s.Points(i + 1).Format.Line.Visible = False
                        If i + 1 <= UBound(Xs) Then s.Points(i + 2).Format.Line.Visible = False
                        i += 1
                    End If
                Next
            End With
        End Sub

    End Class

    Public Class WireEllipsoid3D
        Implements IXYZDrawable3D, IXYZHasBounds

        Public Property Cx As Double
        Public Property Cy As Double
        Public Property Cz As Double

        'Diameters along each axis (full widths)
        Public Property DiameterX As Double
        Public Property DiameterY As Double
        Public Property DiameterZ As Double

        Public Property LatitudeRings As Integer = 8
        Public Property LongitudeRings As Integer = 12
        Public Property PointsPerRing As Integer = 120

        Public Property ColorR As Integer = 125
        Public Property ColorG As Integer = 0
        Public Property ColorB As Integer = 0

        Public Property SeriesName As String = "WireEllipsoid"

        Private Xs As Double()
        Private Ys As Double()

        Public Sub New(cx As Double, cy As Double, cz As Double,
                       diameterX As Double, diameterY As Double, diameterZ As Double)
            Me.Cx = cx : Me.Cy = cy : Me.Cz = cz
            Me.DiameterX = diameterX : Me.DiameterY = diameterY : Me.DiameterZ = diameterZ
        End Sub

        Public Sub GetRawBounds(ByRef minX As Double, ByRef maxX As Double,
                                ByRef minY As Double, ByRef maxY As Double,
                                ByRef minZ As Double, ByRef maxZ As Double) Implements IXYZHasBounds.GetRawBounds
            Dim bounds = WireObjectGeometryCore.EllipsoidBounds(Cx, Cy, Cz, DiameterX, DiameterY, DiameterZ)
            minX = bounds(0) : maxX = bounds(1) : minY = bounds(2) : maxY = bounds(3) : minZ = bounds(4) : maxZ = bounds(5)
        End Sub

        Public Sub Draw(owner As XYZscatter, figure As Chart) Implements IXYZDrawable3D.Draw
            If figure Is Nothing Then Exit Sub
            If DiameterX <= 0 OrElse DiameterY <= 0 OrElse DiameterZ <= 0 Then Exit Sub

            Dim geometry = WireObjectGeometryCore.Ellipsoid(owner.ProjectionContext(), Cx, Cy, Cz, DiameterX, DiameterY, DiameterZ, LatitudeRings, LongitudeRings, PointsPerRing, owner.BreakValue)
            Xs = geometry.X : ys = geometry.Y
            Dim s As Series = figure.SeriesCollection.NewSeries()
            With s
                .Name = SeriesName
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                .XValues = Xs
                .Values = Ys
                .Border.Weight = XlBorderWeight.xlThin
                .Border.Color = RGB(ColorR, ColorG, ColorB)

                'Make ring separators invisible
                For i As Integer = 0 To Xs.Length - 1
                    If Xs(i) = owner.BreakValue Then
                        s.Points(i + 1).Format.Line.Visible = False
                        If i + 1 <= UBound(Xs) Then s.Points(i + 2).Format.Line.Visible = False
                        i += 1
                    End If
                Next
            End With
        End Sub

    End Class
End Namespace

