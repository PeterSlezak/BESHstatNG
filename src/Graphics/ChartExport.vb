Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Excel = Microsoft.Office.Interop.Excel

Module ChartExport

    Public Enum ExportFormat
        PNG
        TIFF
        JPG
        GIF
        BMP
        EMF
    End Enum


    Private Const CF_ENHMETAFILE As UInteger = 14

        <DllImport("user32.dll", SetLastError:=True)>
        Private Function OpenClipboard(hWndNewOwner As IntPtr) As Boolean
        End Function

        <DllImport("user32.dll", SetLastError:=True)>
        Private Function CloseClipboard() As Boolean
        End Function

        <DllImport("user32.dll", SetLastError:=True)>
        Private Function GetClipboardData(uFormat As UInteger) As IntPtr
        End Function

        <DllImport("gdi32.dll", SetLastError:=True)>
        Private Function GetEnhMetaFileBits(hemf As IntPtr, cbBuffer As UInteger, lpbBuffer As Byte()) As UInteger
        End Function

    Public Sub ExportChart(ch As Excel.Chart,
                           outputPath As String,
                           fmt As ExportFormat,
                           dpi As Integer,
                           widthPx As Integer,
                           heightPx As Integer,
                           Optional jpgquality As Integer = 92,
                           Optional maxWorkingSetMB As Integer = 0)
        ' (Same implementation as your ExportActiveChart overload,
        ' but use the passed-in ch instead of app.ActiveChart)
        If dpi < 72 OrElse dpi > 1200 Then BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(dpi), "DPI must be 72..1200"))
        If widthPx < 1 OrElse heightPx < 1 Then BSerr.LogAndThrow(New ArgumentOutOfRangeException("widthPx/heightPx", "Pixel size must be >= 1"))
        If maxWorkingSetMB <= 0 Then maxWorkingSetMB = DefaultMaxWorkingSetMB()
        If ch Is Nothing Then BSerr.LogAndThrow(New InvalidOperationException("No active chart."))

        ' Copy chart as metafile
        ch.CopyPicture(Appearance:=Excel.XlPictureAppearance.xlPrinter,
                   Format:=Excel.XlCopyPictureFormat.xlPicture)

        Dim emfBytes As Byte() = ReadEmfFromClipboardWithRetry(10, 30)
        If emfBytes Is Nothing OrElse emfBytes.Length = 0 Then
            BSerr.LogAndThrow(New InvalidOperationException("Could not retrieve EMF from clipboard."))
        End If

        ' Guardrail (no tiling)
        ' Guardrail + tiling
        Dim estWorking As Long = EstimateWorkingBytes(widthPx, heightPx, 4, 2.0)
        Dim limit As Long = CLng(maxWorkingSetMB) * 1024L * 1024L

        If estWorking > limit Then
            ' Only BMP can be streamed tile-by-tile without extra libraries.
            If fmt = ExportFormat.BMP Then
                ' Use a tile budget; for 32-bit keep tiles smaller.
                Dim tileBudgetBytes As Long = Math.Min(limit \ 2, 128L * 1024L * 1024L) ' <=128MB per tile
                Using ms As New IO.MemoryStream(emfBytes)
                    Using mf As New Metafile(ms)
                        ExportBmpTiledByBytes(mf, outputPath, dpi, widthPx, heightPx, tileBudgetBytes)
                    End Using
                End Using
                Return
            End If

            BSerr.LogAndThrow(New InvalidOperationException(
                            $"Requested bitmap is too large for current memory limit. " &
                            $"Estimated working set: {(estWorking / (1024.0 * 1024.0)):F0} MB, limit: {maxWorkingSetMB} MB. " &
                            $"Reduce pixel size/DPI, or export as BMP (tiled)."))
        End If


        Using ms As New IO.MemoryStream(emfBytes)
            Using mf As New Metafile(ms)
                Using bmp As New Bitmap(widthPx, heightPx, PixelFormat.Format32bppArgb)
                    bmp.SetResolution(dpi, dpi)

                    Using g As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bmp)
                        g.Clear(Color.White) ' RGB-only, no transparency
                        g.DrawImage(mf, New Rectangle(0, 0, widthPx, heightPx))
                    End Using

                    Select Case fmt
                        Case ExportFormat.PNG
                            bmp.Save(outputPath, ImageFormat.Png)

                        Case ExportFormat.GIF
                            bmp.Save(outputPath, ImageFormat.Gif)

                        Case ExportFormat.TIFF
                            SaveTiffRgb(outputPath, bmp)

                        Case ExportFormat.JPG
                            SaveJpeg(outputPath, bmp, jpgquality)

                        Case ExportFormat.BMP
                            bmp.Save(outputPath, ImageFormat.Bmp)

                        Case ExportFormat.EMF
                            IO.File.WriteAllBytes(outputPath, emfBytes)

                    End Select
                End Using
            End Using
        End Using

    End Sub


    Private Sub PointsToPixels(wPts As Double, hPts As Double, dpi As Integer, ByRef wPx As Integer, ByRef hPx As Integer)
        Dim wIn As Double = wPts / 72.0
        Dim hIn As Double = hPts / 72.0
        wPx = Math.Max(1, CInt(Math.Round(wIn * dpi)))
        hPx = Math.Max(1, CInt(Math.Round(hIn * dpi)))
    End Sub

    Private Sub SaveTiffRgb(path As String, bmp As Bitmap)
        Dim codec = ImageCodecInfo.GetImageEncoders().First(Function(c) c.MimeType = "image/tiff")
        Using ep As New EncoderParameters(1)
            ' LZW is widely supported; you can change this to other compressions if desired
            ep.Param(0) = New EncoderParameter(Encoder.Compression, CLng(EncoderValue.CompressionLZW))
            bmp.Save(path, codec, ep)
        End Using
    End Sub

    Private Sub SaveJpeg(path As String, bmp As Bitmap, Optional quality As Long = 92)
        Dim jpegCodec = ImageCodecInfo.GetImageEncoders().
                  First(Function(c) c.MimeType = "image/jpeg")

        Using ep As New EncoderParameters(1)
            ep.Param(0) = New EncoderParameter(Encoder.Quality, quality)
            bmp.Save(path, jpegCodec, ep)
        End Using
    End Sub

    Private Function ReadEmfFromClipboardWithRetry(attempts As Integer, delayMs As Integer) As Byte()
        For i = 1 To attempts
            Dim data = ReadEmfFromClipboard()
            If data IsNot Nothing AndAlso data.Length > 0 Then Return data
            Thread.Sleep(delayMs)
        Next
        Return Nothing
    End Function

    Private Function ReadEmfFromClipboard() As Byte()
        If Not OpenClipboard(IntPtr.Zero) Then Return Nothing
        Try
            Dim hemf As IntPtr = GetClipboardData(CF_ENHMETAFILE)
            If hemf = IntPtr.Zero Then Return Nothing

            Dim size As UInteger = GetEnhMetaFileBits(hemf, 0UI, Nothing)
            If size = 0UI Then Return Nothing

            Dim buffer(CInt(size - 1UI)) As Byte
            Dim read As UInteger = GetEnhMetaFileBits(hemf, size, buffer)
            If read = 0UI Then Return Nothing

            Return buffer
        Finally
            CloseClipboard()
        End Try
    End Function

    '=========================
    '  Tiled BMP export (streaming, no huge bitmap allocation)
    '=========================
    Private Sub ExportBmpTiledByBytes(mf As Metafile,
                                 outputPath As String,
                                 dpi As Integer,
                                 widthPx As Integer,
                                 heightPx As Integer,
                                 maxTileBytes As Long)

        Dim bytesPerPixel As Integer = 4
        Dim stride As Integer = Align4(widthPx * bytesPerPixel)
        Dim tileHeight As Integer = CInt(Math.Max(1, Math.Min(heightPx, maxTileBytes \ Math.Max(1, stride))))

        ' Write a TOP-DOWN BMP so we can write scanlines in natural order (y=0..height-1).
        Using fs As New IO.FileStream(outputPath, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read)
            WriteBmpHeaderTopDown32bpp(fs, widthPx, heightPx, dpi, stride)

            ' Pre-size file so we can Seek+Write
            Dim pixelDataOffset As Integer = 14 + 40
            fs.SetLength(pixelDataOffset + CLng(stride) * CLng(heightPx))

            Dim y As Integer = 0
            While y < heightPx
                Dim h As Integer = Math.Min(tileHeight, heightPx - y)

                Using tile As New Bitmap(widthPx, h, PixelFormat.Format32bppArgb)
                    tile.SetResolution(dpi, dpi)

                    Using g As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(tile)
                        g.Clear(Color.White)

                        ' Render the metafile shifted upward by y pixels so the tile captures [y..y+h)
                        g.TranslateTransform(0.0F, -CSng(y))
                        g.DrawImage(mf, New Rectangle(0, 0, widthPx, heightPx))
                        g.ResetTransform()
                    End Using

                    Dim rect As New Rectangle(0, 0, widthPx, h)
                    Dim bd As BitmapData = tile.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
                    Try
                        Dim bufSize As Integer = Math.Abs(bd.Stride) * h
                        Dim buffer(bufSize - 1) As Byte
                        Marshal.Copy(bd.Scan0, buffer, 0, buffer.Length)

                        ' Write tile rows directly into the BMP pixel array:
                        ' TOP-DOWN => row 0 is y=0, so file offset = header + (y * stride)
                        Dim destOffset As Long = pixelDataOffset + CLng(y) * CLng(stride)
                        fs.Position = destOffset
                        fs.Write(buffer, 0, buffer.Length)

                    Finally
                        tile.UnlockBits(bd)
                    End Try
                End Using

                y += h
            End While
        End Using
    End Sub

    Private Sub WriteBmpHeaderTopDown32bpp(fs As IO.FileStream,
                                      widthPx As Integer,
                                      heightPx As Integer,
                                      dpi As Integer,
                                      stride As Integer)

        Dim fileHeaderSize As Integer = 14
        Dim infoHeaderSize As Integer = 40
        Dim pixelDataOffset As Integer = fileHeaderSize + infoHeaderSize

        Dim imageSize As Integer = stride * heightPx
        Dim fileSize As Integer = pixelDataOffset + imageSize

        ' DPI to pixels-per-meter
        Dim ppm As Integer = CInt(Math.Round(dpi * 39.370078740157481)) ' 1 inch = 0.0254 m

        Using bw As New IO.BinaryWriter(fs, System.Text.Encoding.ASCII, leaveOpen:=True)

            ' BITMAPFILEHEADER (14 bytes)
            bw.Write(CUShort(&H4D42US))              ' "BM"
            bw.Write(fileSize)                      ' bfSize
            bw.Write(CUShort(0US))                  ' bfReserved1
            bw.Write(CUShort(0US))                  ' bfReserved2
            bw.Write(pixelDataOffset)               ' bfOffBits

            ' BITMAPINFOHEADER (40 bytes)
            bw.Write(infoHeaderSize)                ' biSize
            bw.Write(widthPx)                       ' biWidth
            bw.Write(-heightPx)                     ' biHeight (negative => TOP-DOWN)
            bw.Write(CUShort(1US))                  ' biPlanes
            bw.Write(CUShort(32US))                 ' biBitCount
            bw.Write(0)                             ' biCompression = BI_RGB (0)
            bw.Write(imageSize)                     ' biSizeImage
            bw.Write(ppm)                           ' biXPelsPerMeter
            bw.Write(ppm)                           ' biYPelsPerMeter
            bw.Write(0)                             ' biClrUsed
            bw.Write(0)                             ' biClrImportant

            bw.Flush()
        End Using
    End Sub

    Private Function Align4(n As Integer) As Integer
        Return (n + 3) And Not 3
    End Function

    ' Raw bytes for a bitmap with given dimensions and bytes/pixel
    Private Function EstimateRawBitmapBytes(widthPx As Integer, heightPx As Integer, bytesPerPixel As Integer) As Long
        Dim stride As Integer = Align4(widthPx * bytesPerPixel)
        Return CLng(stride) * CLng(heightPx)
    End Function

    ' Working bytes estimate includes headroom for GDI+ and encoders
    Private Function EstimateWorkingBytes(widthPx As Integer,
                                      heightPx As Integer,
                                      bytesPerPixel As Integer,
                                      Optional safetyFactor As Double = 2.0) As Long
        Dim raw As Long = EstimateRawBitmapBytes(widthPx, heightPx, bytesPerPixel)
        Return CLng(Math.Ceiling(raw * safetyFactor))
    End Function

    Private Function DefaultMaxWorkingSetMB() As Integer
        ' Different defaults for 32/64-bit Excel
        If Environment.Is64BitProcess Then
            Return 900
        Else
            Return 250
        End If
    End Function

End Module
