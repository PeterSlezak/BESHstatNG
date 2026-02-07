Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports System.Runtime.Serialization

Public Module GifAnimator

    Public Sub CreateAnimatedGif(inputGifPaths As IEnumerable(Of String),
                                 outputGifPath As String,
                                 frameDelaysMs As IList(Of Integer),
                                 loopCount As UShort,
                                 Optional includeAllFramesFromAnimatedInputs As Boolean = False,
                                 Optional progressBar As System.Windows.Forms.ProgressBar = Nothing)

        If inputGifPaths Is Nothing Then BSerr.LogAndThrow(New ArgumentNullException(NameOf(inputGifPaths)))
        If frameDelaysMs Is Nothing Then BSerr.LogAndThrow(New ArgumentNullException(NameOf(frameDelaysMs)))

        Dim paths = inputGifPaths.Where(Function(p) Not String.IsNullOrWhiteSpace(p)).ToList()
        If paths.Count = 0 Then BSerr.LogAndThrow(New InvalidOperationException("No input GIF paths provided."))
        For Each p In paths
            If Not File.Exists(p) Then BSerr.LogAndThrow(New FileNotFoundException("Input GIF not found.", p))
        Next

        ' 1) Figure out how many OUTPUT frames we will write (needed for per-frame delay array)
        Dim totalFrames As Integer = If(includeAllFramesFromAnimatedInputs,
                                        CountAllFrames(paths),
                                        paths.Count)

        If frameDelaysMs.Count <> totalFrames Then
            BSerr.LogAndThrow(New ArgumentException($"frameDelaysMs count ({frameDelaysMs.Count}) must match output frame count ({totalFrames})."))
        End If

        ' Convert delays to centiseconds (1/100 sec)
        Dim delaysCs(totalFrames - 1) As Integer
        For i = 0 To totalFrames - 1
            delaysCs(i) = MsToCentiseconds(frameDelaysMs(i))
        Next

        ' 2) Determine output size from the first output frame
        Dim outW As Integer, outH As Integer
        GetFirstFrameSize(paths(0), includeAllFramesFromAnimatedInputs, outW, outH)

        Dim gifEncoder = GetEncoder(ImageFormat.Gif)
        If gifEncoder Is Nothing Then BSerr.LogAndThrow(New InvalidOperationException("GIF encoder not found."))

        Dim encSaveFlag = System.Drawing.Imaging.Encoder.SaveFlag
        Dim ep As New EncoderParameters(1)

        Dim frameIndex As Integer = 0
        Dim firstFrameImage As Image = Nothing

        Try
            ' 3) Write frames one-by-one
            For Each path In paths
                Using img As Image = Image.FromFile(path)
                    Dim frameCount As Integer = img.GetFrameCount(FrameDimension.Time)
                    Dim takeCount As Integer = If(includeAllFramesFromAnimatedInputs, frameCount, 1)

                    For fi = 0 To takeCount - 1
                        If frameCount > 1 Then img.SelectActiveFrame(FrameDimension.Time, fi)

                        Using bmp As Bitmap = RenderToCanvas(img, outW, outH)
                            If frameIndex = 0 Then
                                ' First frame: set metadata BEFORE saving
                                firstFrameImage = CType(bmp.Clone(), Image)

                                SetFrameDelayProperty(firstFrameImage, delaysCs)
                                SetLoopCountProperty(firstFrameImage, loopCount)

                                ep.Param(0) = New EncoderParameter(encSaveFlag, CLng(EncoderValue.MultiFrame))
                                firstFrameImage.Save(outputGifPath, gifEncoder, ep)
                            Else
                                ep.Param(0) = New EncoderParameter(encSaveFlag, CLng(EncoderValue.FrameDimensionTime))
                                firstFrameImage.SaveAdd(bmp, ep)
                            End If
                        End Using

                        frameIndex += 1
                        UpdateProgress(progressBar, frameIndex, totalFrames)
                    Next
                End Using
            Next

            ' 4) Flush/close
            ep.Param(0) = New EncoderParameter(encSaveFlag, CLng(EncoderValue.Flush))
            firstFrameImage.SaveAdd(ep)

        Finally
            If firstFrameImage IsNot Nothing Then firstFrameImage.Dispose()
        End Try
    End Sub

    ' ---------- helpers ----------

    Private Function CountAllFrames(paths As List(Of String)) As Integer
        Dim total As Integer = 0
        For Each p In paths
            Using img As Image = Image.FromFile(p)
                total += Math.Max(1, img.GetFrameCount(FrameDimension.Time))
            End Using
        Next
        Return total
    End Function

    Private Sub GetFirstFrameSize(firstPath As String, includeAllFrames As Boolean, ByRef w As Integer, ByRef h As Integer)
        Using img As Image = Image.FromFile(firstPath)
            If includeAllFrames AndAlso img.GetFrameCount(FrameDimension.Time) > 1 Then
                img.SelectActiveFrame(FrameDimension.Time, 0)
            End If
            w = img.Width
            h = img.Height
        End Using
    End Sub

    Private Function RenderToCanvas(src As Image, w As Integer, h As Integer) As Bitmap
        ' Normalizes size (currently stretches). Change drawing logic if you want letterboxing.
        Dim bmp As New Bitmap(w, h, PixelFormat.Format32bppArgb)
        Using g As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)
            g.DrawImage(src, 0, 0, w, h)
        End Using
        Return bmp
    End Function

    Private Sub UpdateProgress(pb As System.Windows.Forms.ProgressBar, done As Integer, total As Integer)
        If pb Is Nothing Then Return
        Dim pct As Integer = CInt(Math.Truncate(100.0 * done / total))
        If pct < 0 Then pct = 0
        If pct > 100 Then pct = 100
        pb.Invoke(Sub() pb.Value = pct)
        System.Windows.Forms.Application.DoEvents()
    End Sub

    Private Function MsToCentiseconds(ms As Integer) As Integer
        Dim safeMs = If(ms < 0, 0, ms)
        Return Math.Max(1, CInt(Math.Round(safeMs / 10.0)))
    End Function

    Private Function GetEncoder(fmt As ImageFormat) As ImageCodecInfo
        Return ImageCodecInfo.GetImageDecoders().FirstOrDefault(Function(c) c.FormatID = fmt.Guid)
    End Function

    Private Function CreatePropertyItem(id As Integer, type As Short, valueBytes As Byte()) As PropertyItem
        Dim pi = CType(FormatterServices.GetUninitializedObject(GetType(PropertyItem)), PropertyItem)
        pi.Id = id
        pi.Type = type
        pi.Len = valueBytes.Length
        pi.Value = valueBytes
        Return pi
    End Function

    ' 0x5100 FrameDelay: type=4 (Long), array of int32 delays in 1/100 sec
    Private Sub SetFrameDelayProperty(img As Image, delaysCs As Integer())
        Dim bytes(4 * delaysCs.Length - 1) As Byte
        For i = 0 To delaysCs.Length - 1
            Dim b = BitConverter.GetBytes(delaysCs(i))
            Buffer.BlockCopy(b, 0, bytes, i * 4, 4)
        Next
        img.SetPropertyItem(CreatePropertyItem(&H5100, 4S, bytes))
    End Sub

    ' 0x5101 LoopCount: type=3 (Short), UInt16. 0=infinite.
    Private Sub SetLoopCountProperty(img As Image, loopCount As UShort)
        img.SetPropertyItem(CreatePropertyItem(&H5101, 3S, BitConverter.GetBytes(loopCount)))
    End Sub

End Module
