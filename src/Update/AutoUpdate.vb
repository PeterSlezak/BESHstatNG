Option Explicit On

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Xml
Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration

Namespace BESHStatUpdate

    ''' <summary>
    ''' Non-blocking update checker for the BESH-Stat-NG Excel-DNA add-in.
    '''
    ''' Goals:
    '''  - Minimal impact on Excel startup: all network work runs on background thread.
    '''  - Show a small UI when an update exists with options:
    '''        Download now / Remind in 7 days / Ignore this version.
    '''  - Persist user choice (remind/ignore) across sessions.
    ''' </summary>
    Public NotInheritable Class AutoUpdate

        Private Shared _started As Integer = 0

        ''' <summary>
        ''' Starts the update check in the background (safe to call multiple times).
        ''' </summary>
        ''' <param name="startupDelayMs">Delay before the first check to avoid affecting startup.</param>
        Public Shared Sub Start(Optional startupDelayMs As Integer = 4000)
            If Interlocked.Exchange(_started, 1) = 1 Then Return

            Task.Run(Async Function()
                         Try
                             If startupDelayMs > 0 Then Await Task.Delay(startupDelayMs).ConfigureAwait(False)
                             Await CheckForUpdateAndPromptAsync().ConfigureAwait(False)
                         Catch ex As Exception
                             ' Never surface update-check exceptions to the user.
                             Try
                                 AppGlobals.BSlogg.Log($"AutoUpdate failed: {ex.Message}", AppGlobals.LogMsgType.Debug)
                             Catch
                             End Try
                         End Try
                     End Function)
        End Sub

        ' -------------------------
        ' State persistence
        ' -------------------------
        Private Shared ReadOnly Property StateFolder As String
            Get
                Dim basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                Return Path.Combine(basePath, AppGlobals.gsAPP_TITLE)
            End Get
        End Property

        Private Shared ReadOnly Property StateFile As String
            Get
                Return Path.Combine(StateFolder, "update-state.xml")
            End Get
        End Property

        Private Class UpdateState
            Public Property IgnoreVersion As String = ""
            Public Property RemindAfterUtc As DateTime = DateTime.MinValue
            Public Property LastCheckUtc As DateTime = DateTime.MinValue
        End Class

        Private Shared Function LoadState() As UpdateState
            Dim st As New UpdateState()
            Try
                If Not File.Exists(StateFile) Then Return st
                Dim doc As New XmlDocument()
                doc.Load(StateFile)
                Dim root = doc.SelectSingleNode("/UpdateState")
                If root Is Nothing Then Return st

                Dim nIgnore = root.SelectSingleNode("IgnoreVersion")
                If nIgnore IsNot Nothing Then st.IgnoreVersion = nIgnore.InnerText.Trim()

                Dim nRemind = root.SelectSingleNode("RemindAfterUtc")
                If nRemind IsNot Nothing Then
                    Dim dt As DateTime
                    If DateTime.TryParse(nRemind.InnerText, dt) Then st.RemindAfterUtc = DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                End If

                Dim nLast = root.SelectSingleNode("LastCheckUtc")
                If nLast IsNot Nothing Then
                    Dim dt As DateTime
                    If DateTime.TryParse(nLast.InnerText, dt) Then st.LastCheckUtc = DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                End If
            Catch
                ' Ignore malformed state.
            End Try
            Return st
        End Function

        Private Shared Sub SaveState(st As UpdateState)
            Try
                Directory.CreateDirectory(StateFolder)

                Dim doc As New XmlDocument()
                Dim root = doc.CreateElement("UpdateState")
                doc.AppendChild(root)

                Dim eIgnore = doc.CreateElement("IgnoreVersion")
                eIgnore.InnerText = If(st.IgnoreVersion, "")
                root.AppendChild(eIgnore)

                Dim eRemind = doc.CreateElement("RemindAfterUtc")
                eRemind.InnerText = If(st.RemindAfterUtc = DateTime.MinValue, "", st.RemindAfterUtc.ToString("o"))
                root.AppendChild(eRemind)

                Dim eLast = doc.CreateElement("LastCheckUtc")
                eLast.InnerText = If(st.LastCheckUtc = DateTime.MinValue, "", st.LastCheckUtc.ToString("o"))
                root.AppendChild(eLast)

                doc.Save(StateFile)
            Catch
            End Try
        End Sub

        ' -------------------------
        ' Core logic
        ' -------------------------
        Private Shared Async Function CheckForUpdateAndPromptAsync() As Task
            Dim st = LoadState()
            Dim nowUtc = DateTime.UtcNow

            ' Throttle: at most once per day.
            If st.LastCheckUtc <> DateTime.MinValue AndAlso (nowUtc - st.LastCheckUtc) < TimeSpan.FromHours(24) Then
                Return
            End If
            st.LastCheckUtc = nowUtc
            SaveState(st)

            ' Respect remind date.
            If st.RemindAfterUtc <> DateTime.MinValue AndAlso nowUtc < st.RemindAfterUtc Then
                Return
            End If

            Dim info As UpdateInfo = Await TryGetUpdateInfoAsync().ConfigureAwait(False)
            If info Is Nothing Then Return

            If Not IsNewerVersion(AppGlobals.gAddinVersion, info.NewVersion) Then Return
            If String.Equals(st.IgnoreVersion, info.NewVersion, StringComparison.OrdinalIgnoreCase) Then Return

            ' Prompt on Excel UI thread.
            ExcelAsyncUtil.QueueAsMacro(Sub()
                                            Try
                                                ShowPromptAndPersist(info)
                                            Catch
                                            End Try
                                        End Sub)
        End Function

        Private Class UpdateInfo
            Public Property NewVersion As String
            Public Property DetailsUrl As String
        End Class

        Private Shared Async Function TryGetUpdateInfoAsync() As Task(Of UpdateInfo)
            Try
                Using client As New HttpClient()
                    client.Timeout = TimeSpan.FromSeconds(3)
                    Dim xmlText = Await client.GetStringAsync(AppGlobals.gCheckUpdateUrl).ConfigureAwait(False)
                    If String.IsNullOrWhiteSpace(xmlText) Then Return Nothing

                    Dim doc As New XmlDocument()
                    doc.LoadXml(xmlText)
                    Dim nodes = doc.SelectNodes("root/CurrentVersion/update")
                    If nodes Is Nothing OrElse nodes.Count = 0 Then Return Nothing
                    Dim n = nodes(0)

                    Dim ver = n.Attributes?.GetNamedItem("version")?.Value
                    Dim url = n.Attributes?.GetNamedItem("detailsURL")?.Value
                    If String.IsNullOrWhiteSpace(ver) OrElse String.IsNullOrWhiteSpace(url) Then Return Nothing

                    Return New UpdateInfo With {.NewVersion = ver.Trim(), .DetailsUrl = url.Trim()}
                End Using
            Catch
                Return Nothing
            End Try
        End Function

        Private Shared Function IsNewerVersion(strCurrentVersion As String, strNewVersion As String) As Boolean
            Try
                Dim currentVer() As String = strCurrentVersion.Split("."c)
                Dim newVer() As String = strNewVersion.Split("."c)
                Dim l As Integer = Math.Min(currentVer.Length, newVer.Length)
                For i = 0 To l - 1
                    Dim c = Integer.Parse(currentVer(i))
                    Dim n = Integer.Parse(newVer(i))
                    If c = n Then Continue For
                    Return n > c
                Next
            Catch
            End Try
            Return False
        End Function

        Private Shared Sub ShowPromptAndPersist(info As UpdateInfo)
            Dim st = LoadState()

            Using frm As New UiUpdateAvailable(info.NewVersion, info.DetailsUrl)
                frm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
                Dim owner As System.Windows.Forms.IWin32Window = Nothing
                Try
                    owner = New ExcelWindowWrapper(AppGlobals.ExcelMainHwnd)
                Catch
                End Try

                Dim res As System.Windows.Forms.DialogResult
                If owner IsNot Nothing Then
                    res = frm.ShowDialog(owner)
                Else
                    res = frm.ShowDialog()
                End If

                Select Case res
                    Case System.Windows.Forms.DialogResult.OK ' Download now
                        Try
                            Process.Start(info.DetailsUrl)
                        Catch
                        End Try

                    Case System.Windows.Forms.DialogResult.Retry ' Remind in 7 days
                        st.RemindAfterUtc = DateTime.UtcNow.AddDays(7)
                        SaveState(st)

                    Case System.Windows.Forms.DialogResult.Ignore ' Ignore this version
                        st.IgnoreVersion = info.NewVersion
                        st.RemindAfterUtc = DateTime.MinValue
                        SaveState(st)
                End Select
            End Using
        End Sub

    End Class

End Namespace
