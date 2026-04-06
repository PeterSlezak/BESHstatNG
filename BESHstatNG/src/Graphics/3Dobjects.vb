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
            Dim r As Double = Diameter / 2.0
            minX = Cx - r : maxX = Cx + r
            minY = Cy - r : maxY = Cy + r
            minZ = Cz - r : maxZ = Cz + r
        End Sub

        Public Sub Draw(owner As XYZscatter, figure As Chart) Implements IXYZDrawable3D.Draw
            If figure Is Nothing Then Exit Sub
            If Diameter <= 0 Then Exit Sub

            Dim r As Double = Diameter / 2.0
            Dim pts As Integer = Math.Max(12, PointsPerRing)
            Dim lat As Integer = Math.Max(0, LatitudeRings)
            Dim lon As Integer = Math.Max(1, LongitudeRings)

            Dim nCircles As Integer = lat + lon
            If nCircles <= 0 Then Exit Sub

            Dim stride As Integer = (pts + 2) ' (pts+1 closed) + break
            ReDim Xs(nCircles * stride - 1)
            ReDim ys(nCircles * stride - 1)

            Dim twoPi As Double = 2.0 * Math.PI
            Dim writeIdx As Integer = 0
            Dim x2d As Double, y2d As Double

            'Latitude rings (exclude poles)
            For iLat As Integer = 1 To lat
                Dim t As Double = iLat / CDbl(lat + 1)
                Dim phi As Double = (-Math.PI / 2.0) + t * Math.PI
                Dim ringR As Double = r * Math.Cos(phi)
                Dim z0 As Double = Cz + r * Math.Sin(phi)

                For k As Integer = 0 To pts
                    Dim theta As Double = twoPi * (k / CDbl(pts))
                    owner.ProjectRawPoint(Cx + ringR * Math.Cos(theta),
                                          Cy + ringR * Math.Sin(theta),
                                          z0, x2d, y2d)
                    Xs(writeIdx) = x2d : ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = owner.BreakValue
                ys(writeIdx) = owner.BreakValue
                writeIdx += 1
            Next

            'Longitude meridians
            For iLon As Integer = 0 To lon - 1
                Dim theta0 As Double = twoPi * (iLon / CDbl(lon))

                For k As Integer = 0 To pts
                    Dim t As Double = k / CDbl(pts)
                    Dim phi As Double = (-Math.PI / 2.0) + t * Math.PI

                    owner.ProjectRawPoint(Cx + r * Math.Cos(theta0) * Math.Cos(phi),
                                          Cy + r * Math.Sin(theta0) * Math.Cos(phi),
                                          Cz + r * Math.Sin(phi), x2d, y2d)
                    Xs(writeIdx) = x2d : ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = owner.BreakValue
                ys(writeIdx) = owner.BreakValue
                writeIdx += 1
            Next

            Dim s As Series = figure.SeriesCollection.NewSeries()
            With s
                .Name = SeriesName
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                .XValues = Xs
                .Values = ys
                .Border.Weight = XlBorderWeight.xlThin
                .Border.Color = RGB(ColorR, ColorG, ColorB)
                '.Format.Line.Visible = True
                '.Format.Line.Weight = 0.75
                '.Format.Line.Visible = True
                '.Format.Line.ForeColor.RGB = RGB(150, 0, 0)

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
            Dim rx As Double = DiameterX / 2.0
            Dim ry As Double = DiameterY / 2.0
            Dim rz As Double = DiameterZ / 2.0
            minX = Cx - rx : maxX = Cx + rx
            minY = Cy - ry : maxY = Cy + ry
            minZ = Cz - rz : maxZ = Cz + rz
        End Sub

        Public Sub Draw(owner As XYZscatter, figure As Chart) Implements IXYZDrawable3D.Draw
            If figure Is Nothing Then Exit Sub
            If DiameterX <= 0 OrElse DiameterY <= 0 OrElse DiameterZ <= 0 Then Exit Sub

            Dim a As Double = DiameterX / 2.0 'semi-axis X
            Dim b As Double = DiameterY / 2.0 'semi-axis Y
            Dim c As Double = DiameterZ / 2.0 'semi-axis Z

            Dim pts As Integer = Math.Max(12, PointsPerRing)
            Dim lat As Integer = Math.Max(0, LatitudeRings)
            Dim lon As Integer = Math.Max(1, LongitudeRings)

            Dim nCircles As Integer = lat + lon
            If nCircles <= 0 Then Exit Sub

            Dim stride As Integer = (pts + 2) ' (pts+1 closed) + break
            ReDim Xs(nCircles * stride - 1)
            ReDim Ys(nCircles * stride - 1)

            Dim twoPi As Double = 2.0 * Math.PI
            Dim writeIdx As Integer = 0
            Dim x2d As Double, y2d As Double

            'Latitude rings (exclude poles)
            'phi = latitude angle [-pi/2..+pi/2]
            For iLat As Integer = 1 To lat
                Dim t As Double = iLat / CDbl(lat + 1)
                Dim phi As Double = (-Math.PI / 2.0) + t * Math.PI

                Dim ringA As Double = a * Math.Cos(phi) 'semi-axis of ring in X
                Dim ringB As Double = b * Math.Cos(phi) 'semi-axis of ring in Y
                Dim z0 As Double = Cz + c * Math.Sin(phi)

                For k As Integer = 0 To pts
                    Dim theta As Double = twoPi * (k / CDbl(pts))
                    Dim xRaw As Double = Cx + ringA * Math.Cos(theta)
                    Dim yRaw As Double = Cy + ringB * Math.Sin(theta)
                    Dim zRaw As Double = z0

                    owner.ProjectRawPoint(xRaw, yRaw, zRaw, x2d, y2d)
                    Xs(writeIdx) = x2d : Ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = owner.BreakValue
                Ys(writeIdx) = owner.BreakValue
                writeIdx += 1
            Next

            'Longitude meridians
            'theta0 fixed; phi varies
            For iLon As Integer = 0 To lon - 1
                Dim theta0 As Double = twoPi * (iLon / CDbl(lon))

                For k As Integer = 0 To pts
                    Dim t As Double = k / CDbl(pts)
                    Dim phi As Double = (-Math.PI / 2.0) + t * Math.PI

                    Dim xRaw As Double = Cx + a * Math.Cos(theta0) * Math.Cos(phi)
                    Dim yRaw As Double = Cy + b * Math.Sin(theta0) * Math.Cos(phi)
                    Dim zRaw As Double = Cz + c * Math.Sin(phi)

                    owner.ProjectRawPoint(xRaw, yRaw, zRaw, x2d, y2d)
                    Xs(writeIdx) = x2d : Ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = owner.BreakValue
                Ys(writeIdx) = owner.BreakValue
                writeIdx += 1
            Next

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

