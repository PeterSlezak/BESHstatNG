Option Explicit On
Option Strict On
Imports System
Public Module WireObjectGeometryCore
    Public Function Sphere(context As XyzProjectionContext, Cx As Double, Cy As Double, Cz As Double, Diameter As Double, LatitudeRings As Integer, LongitudeRings As Integer, PointsPerRing As Integer, Optional breakValue As Double = -100000000000000.0) As WireSeriesData
        Dim Xs() As Double = Nothing, ys() As Double = Nothing
            If Diameter <= 0 Then Return New WireSeriesData(Array.Empty(Of Double)(), Array.Empty(Of Double)(), 1)

            Dim r As Double = Diameter / 2.0
            Dim pts As Integer = Math.Max(12, PointsPerRing)
            Dim lat As Integer = Math.Max(0, LatitudeRings)
            Dim lon As Integer = Math.Max(1, LongitudeRings)

            Dim nCircles As Integer = lat + lon
            If nCircles <= 0 Then Return New WireSeriesData(Array.Empty(Of Double)(), Array.Empty(Of Double)(), 1)

            Dim stride As Integer = (pts + 2) ' (pts+1 closed) + break
            ReDim Xs(nCircles * stride - 1)
            ReDim ys(nCircles * stride - 1)

            Dim twoPi As Double = 2.0 * Math.PI
            Dim writeIdx As Integer = 0
            Dim x2d As Double, y2d As Double

            For iLat As Integer = 1 To lat
                Dim t As Double = iLat / CDbl(lat + 1)
                Dim phi As Double = (-Math.PI / 2.0) + t * Math.PI
                Dim ringR As Double = r * Math.Cos(phi)
                Dim z0 As Double = Cz + r * Math.Sin(phi)

                For k As Integer = 0 To pts
                    Dim theta As Double = twoPi * (k / CDbl(pts))
                    context.ProjectRawPoint(Cx + ringR * Math.Cos(theta),
                                          Cy + ringR * Math.Sin(theta),
                                          z0, x2d, y2d)
                    Xs(writeIdx) = x2d : ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = breakValue
                ys(writeIdx) = breakValue
                writeIdx += 1
            Next

            For iLon As Integer = 0 To lon - 1
                Dim theta0 As Double = twoPi * (iLon / CDbl(lon))

                For k As Integer = 0 To pts
                    Dim t As Double = k / CDbl(pts)
                    Dim phi As Double = (-Math.PI / 2.0) + t * Math.PI

                    context.ProjectRawPoint(Cx + r * Math.Cos(theta0) * Math.Cos(phi),
                                          Cy + r * Math.Sin(theta0) * Math.Cos(phi),
                                          Cz + r * Math.Sin(phi), x2d, y2d)
                    Xs(writeIdx) = x2d : ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = breakValue
                ys(writeIdx) = breakValue
                writeIdx += 1
            Next

        Return New WireSeriesData(Xs, ys, stride)
    End Function
    Public Function SphereBounds(Cx As Double, Cy As Double, Cz As Double, Diameter As Double) As Double()
        Dim minX, maxX, minY, maxY, minZ, maxZ As Double
            Dim r As Double = Diameter / 2.0
            minX = Cx - r : maxX = Cx + r
            minY = Cy - r : maxY = Cy + r
            minZ = Cz - r : maxZ = Cz + r
        Return {minX, maxX, minY, maxY, minZ, maxZ}
    End Function
    Public Function Ellipsoid(context As XyzProjectionContext, Cx As Double, Cy As Double, Cz As Double, DiameterX As Double, DiameterY As Double, DiameterZ As Double, LatitudeRings As Integer, LongitudeRings As Integer, PointsPerRing As Integer, Optional breakValue As Double = -100000000000000.0) As WireSeriesData
        Dim Xs() As Double = Nothing, ys() As Double = Nothing
            If DiameterX <= 0 OrElse DiameterY <= 0 OrElse DiameterZ <= 0 Then Return New WireSeriesData(Array.Empty(Of Double)(), Array.Empty(Of Double)(), 1)

            Dim a As Double = DiameterX / 2.0 'semi-axis X
            Dim b As Double = DiameterY / 2.0 'semi-axis Y
            Dim c As Double = DiameterZ / 2.0 'semi-axis Z

            Dim pts As Integer = Math.Max(12, PointsPerRing)
            Dim lat As Integer = Math.Max(0, LatitudeRings)
            Dim lon As Integer = Math.Max(1, LongitudeRings)

            Dim nCircles As Integer = lat + lon
            If nCircles <= 0 Then Return New WireSeriesData(Array.Empty(Of Double)(), Array.Empty(Of Double)(), 1)

            Dim stride As Integer = (pts + 2) ' (pts+1 closed) + break
            ReDim Xs(nCircles * stride - 1)
            ReDim Ys(nCircles * stride - 1)

            Dim twoPi As Double = 2.0 * Math.PI
            Dim writeIdx As Integer = 0
            Dim x2d As Double, y2d As Double

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

                    context.ProjectRawPoint(xRaw, yRaw, zRaw, x2d, y2d)
                    Xs(writeIdx) = x2d : Ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = breakValue
                Ys(writeIdx) = breakValue
                writeIdx += 1
            Next

            For iLon As Integer = 0 To lon - 1
                Dim theta0 As Double = twoPi * (iLon / CDbl(lon))

                For k As Integer = 0 To pts
                    Dim t As Double = k / CDbl(pts)
                    Dim phi As Double = (-Math.PI / 2.0) + t * Math.PI

                    Dim xRaw As Double = Cx + a * Math.Cos(theta0) * Math.Cos(phi)
                    Dim yRaw As Double = Cy + b * Math.Sin(theta0) * Math.Cos(phi)
                    Dim zRaw As Double = Cz + c * Math.Sin(phi)

                    context.ProjectRawPoint(xRaw, yRaw, zRaw, x2d, y2d)
                    Xs(writeIdx) = x2d : Ys(writeIdx) = y2d
                    writeIdx += 1
                Next

                Xs(writeIdx) = breakValue
                Ys(writeIdx) = breakValue
                writeIdx += 1
            Next

        Return New WireSeriesData(Xs, ys, stride)
    End Function
    Public Function EllipsoidBounds(Cx As Double, Cy As Double, Cz As Double, DiameterX As Double, DiameterY As Double, DiameterZ As Double) As Double()
        Dim minX, maxX, minY, maxY, minZ, maxZ As Double
            Dim rx As Double = DiameterX / 2.0
            Dim ry As Double = DiameterY / 2.0
            Dim rz As Double = DiameterZ / 2.0
            minX = Cx - rx : maxX = Cx + rx
            minY = Cy - ry : maxY = Cy + ry
            minZ = Cz - rz : maxZ = Cz + rz
        Return {minX, maxX, minY, maxY, minZ, maxZ}
    End Function
End Module
