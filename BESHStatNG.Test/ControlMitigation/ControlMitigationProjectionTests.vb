Option Strict On
Option Explicit On
Option Infer On

Imports System.Globalization
Imports System.Reflection
Imports System.Windows.Forms
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Executes a disclosed source projection, NOT the shipping constructor or native UI.
' It uses initialized real managed controls, never Show/Excel/COM/global mutation.
<TestClass>
Public Class ControlMitigationProjectionTests
    Private Shared Sub OnSta(body As Action)
        Dim failure As Exception = Nothing
        Dim thread As New Threading.Thread(Sub()
                                              Try
                                                  body()
                                              Catch ex As Exception
                                                  failure = ex
                                              End Try
                                          End Sub)
        thread.SetApartmentState(Threading.ApartmentState.STA)
        thread.Start()
        Assert.IsTrue(thread.Join(30000), "Host-free test deadline")
        If failure IsNot Nothing Then Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw()
    End Sub

    <DataTestMethod>
    <DataRow(HelpTopic.OneWayANOVA)> <DataRow(HelpTopic.KruskalWallisTest)> <DataRow(HelpTopic.NormalityTests)>
    Public Sub SharedExtras_StateOnly_AllFourCombinationsDoNotResetOtherOptions(tag As Integer)
        OnSta(Sub()
                  Using form As New UibyIDControlProjection("Characterization", tag)
                      For mask As Integer = 0 To 3
                          Toggle(form, "ckDescriptiveStatistics", (mask And 1) <> 0)
                          Toggle(form, "ckBoxPlot", (mask And 2) <> 0)
                          Assert.AreEqual((mask And 1) <> 0, Checked(form, "ckDescriptiveStatistics"))
                          Assert.AreEqual((mask And 2) <> 0, Checked(form, "ckBoxPlot"))
                          Assert.IsTrue(Checked(form, "ckWelch"), "No option reset, including hidden retained controls")
                      Next
                      ' Actual report/chart branches are source-mapped only, not executed.
                  End Using
              End Sub)
    End Sub

    Private Shared Function Field(form As UibyIDControlProjection, name As String) As Control
        Dim flags = BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public
        Dim member = form.GetType().GetField(name, flags)
        If member IsNot Nothing Then Return DirectCast(member.GetValue(form), Control)
        ' VB WithEvents declarations compile to a property plus a backing field.
        Return DirectCast(form.GetType().GetProperty(name, flags).GetValue(form, Nothing), Control)
    End Function

    Private Shared Function Checked(form As UibyIDControlProjection, name As String) As Boolean
        Return DirectCast(Field(form, name), CheckBox).Checked
    End Function

    Private Shared Sub Toggle(form As UibyIDControlProjection, name As String, value As Boolean)
        DirectCast(Field(form, name), CheckBox).Checked = value
    End Sub

    Private Shared Function LocalVisible(control As Control) As Boolean
        ' .Visible is effectively False under a never-shown Form. Read the managed
        ' local visibility flag, not a native effective-visibility observation.
        Return CBool(GetType(Control).GetMethod("GetState", BindingFlags.Instance Or BindingFlags.NonPublic).
            Invoke(control, New Object() {2}))
    End Function

    Private Shared Sub ClickManaged(control As Control)
        ' Actual managed OnClick and Handles route; no UIA/native click or shown HWND.
        control.GetType().GetMethod("OnClick", BindingFlags.Instance Or BindingFlags.NonPublic).
            Invoke(control, New Object() {System.EventArgs.Empty})
    End Sub

    <DataTestMethod>
    <DataRow(HelpTopic.OneWayANOVA, True, True)>
    <DataRow(HelpTopic.KruskalWallisTest, False, True)>
    <DataRow(HelpTopic.NormalityTests, False, False)>
    Public Sub ModeProjection_ParentDefaultsAndLocalAvailability(tag As Integer, anova As Boolean, variance As Boolean)
        OnSta(Sub()
                  Using form As New UibyIDControlProjection("Characterization", tag)
                      Assert.IsFalse(form.Visible, "Never show the test projection")
                      Dim tabs = DirectCast(Field(form, "TabControl1"), TabControl)
                      CollectionAssert.AreEqual(New String() {"TabPage1", "TabPage_Options"}, tabs.TabPages.Cast(Of TabPage)().Select(Function(t) t.Name).ToArray())
                      Assert.AreSame(Field(form, "TabPage1"), Field(form, "grpInput").Parent)
                      Assert.AreSame(Field(form, "grpInput"), Field(form, "lblRefedit1").Parent)
                      Assert.AreSame(Field(form, "grpInput"), Field(form, "lblRefedit2").Parent)
                      Assert.AreSame(Field(form, "TabPage_Options"), Field(form, "grpANOVA1MCP").Parent)
                      Assert.AreSame(Field(form, "TabPage_Options"), Field(form, "grpHomogeneityVariances").Parent)
                      Assert.AreEqual(anova, LocalVisible(Field(form, "grpANOVA1MCP")))
                      Assert.AreEqual(anova, LocalVisible(Field(form, "ckWelch")))
                      Assert.AreEqual(variance, LocalVisible(Field(form, "grpHomogeneityVariances")))
                      Assert.IsFalse(LocalVisible(Field(form, "lblAlpha")))
                      Assert.IsTrue(LocalVisible(Field(form, "ckBoxPlot")))
                      Assert.IsTrue(LocalVisible(Field(form, "ckDescriptiveStatistics")))
                      For Each name In {"ckWelch", "ckLevene", "ckBartlett", "ckFlignerKilleen", "ckSquaredRanks",
                                        "ckLSD", "ckBonferroni", "ckTukey", "ckGamesHowell", "ckBoxPlot", "ckDescriptiveStatistics"}
                          Assert.IsTrue(Checked(form, name), name & " designer default")
                      Next
                      For Each name In {"RefEdit1", "RefEdit2", "RefEditOutput"}
                          Assert.IsNull(DirectCast(Field(form, name), Excel2007RefEdit).ExcelConnector)
                      Next
                  End Using
              End Sub)
    End Sub

    <DataTestMethod>
    <DataRow(HelpTopic.OneWayANOVA)> <DataRow(HelpTopic.KruskalWallisTest)> <DataRow(HelpTopic.NormalityTests)>
    Public Sub ActualManagedOrientationHandlers_ReentryAndEventOrder(tag As Integer)
        OnSta(Sub()
                  Using form As New UibyIDControlProjection("Characterization", tag)
                      Dim id = DirectCast(Field(form, "optByID"), RadioButton)
                      Dim column = DirectCast(Field(form, "optByColumn"), RadioButton)
                      Assert.IsTrue(id.Checked)
                      DirectCast(Field(form, "RefEdit1"), Excel2007RefEdit).Address = "'Data'!A1:A9"
                      ClickManaged(column)
                      Assert.IsTrue(column.Checked)
                      Assert.IsFalse(Field(form, "RefEdit1").Enabled)
                      Assert.IsFalse(Field(form, "lblRefedit1").Enabled)
                      Assert.AreEqual(String.Empty, DirectCast(Field(form, "RefEdit1"), Excel2007RefEdit).Address)
                      ClickManaged(column)
                      ClickManaged(id)
                      ClickManaged(id)
                      Assert.IsTrue(id.Checked)
                      Assert.IsTrue(Field(form, "RefEdit1").Enabled)
                      Assert.IsTrue(Field(form, "lblRefedit1").Enabled)
                      Assert.AreEqual("Group ID:", Field(form, "lblRefedit1").Text)
                      Assert.AreEqual("Data:", Field(form, "lblRefedit2").Text)
                      Assert.IsTrue(Checked(form, "ckLevene"), "Orientation does not reset options")
                  End Using
              End Sub)
    End Sub

    <DataTestMethod>
    <DataRow(-1)> <DataRow(Integer.MaxValue)>
    Public Sub UnsupportedTag_IsCharacterizedWithoutInventingValidation(tag As Integer)
        OnSta(Sub()
                  Using form As New UibyIDControlProjection(Nothing, tag)
                      Assert.AreEqual(1, DirectCast(Field(form, "TabControl1"), TabControl).TabPages.Count)
                      Assert.IsFalse(LocalVisible(Field(form, "ckBoxPlot")))
                      Assert.IsNull(Field(form, "TabPage_Options").Parent)
                      form.Tag = Nothing
                      Assert.AreEqual(HelpTopic.Home, HelpContext.ResolveTopic(form))
                  End Using
              End Sub)
    End Sub

    <TestMethod>
    Public Sub DisposeTwiceAndNewInstance_DoNotShareCheckboxState()
        OnSta(Sub()
                  Dim first As New UibyIDControlProjection("ANOVA", CInt(HelpTopic.OneWayANOVA))
                  Toggle(first, "ckWelch", False)
                  first.Dispose() : first.Dispose()
                  Assert.IsTrue(first.IsDisposed)
                  Using second As New UibyIDControlProjection("ANOVA", CInt(HelpTopic.OneWayANOVA))
                      Assert.IsTrue(Checked(second, "ckWelch"))
                  End Using
              End Sub)
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)> <DataRow(6)> <DataRow(7)>
    <DataRow(8)> <DataRow(9)> <DataRow(10)> <DataRow(11)> <DataRow(12)> <DataRow(13)> <DataRow(14)> <DataRow(15)>
    <DataRow(16)> <DataRow(17)> <DataRow(18)> <DataRow(19)> <DataRow(20)> <DataRow(21)> <DataRow(22)> <DataRow(23)>
    <DataRow(24)> <DataRow(25)> <DataRow(26)> <DataRow(27)> <DataRow(28)> <DataRow(29)> <DataRow(30)> <DataRow(31)>
    Public Sub ActualOptionInitializer_32Combinations_RealCommonRequestAndResultMetadata(mask As Integer)
        OnSta(Sub()
                  For Each culture In {CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("sk-SK")}
                      Dim saved = CultureInfo.CurrentCulture
                      Try
                          CultureInfo.CurrentCulture = culture
                          Using form As New UibyIDControlProjection("ANOVA", CInt(HelpTopic.OneWayANOVA))
                              Dim names = {"ckWelch", "ckLSD", "ckBonferroni", "ckTukey", "ckGamesHowell"}
                              For bit As Integer = 0 To 4
                                  Toggle(form, names(bit), (mask And (1 << bit)) <> 0)
                              Next
                              Dim options = form.CaptureOptions(0.05)
                              CollectionAssert.AreEqual(Enumerable.Range(0, 5).Select(Function(i) (mask And (1 << i)) <> 0).ToArray(),
                                  New Boolean() {options.IncludeWelch, options.IncludeFisherLsd, options.IncludeBonferroni,
                                      options.IncludeTukeyKramer, options.IncludeGamesHowell})
                              Dim data As New GroupedNumericData({"B", "A", "C"}, New Double()() {
                                  New Double() {1, 2, 4, 6}, New Double() {2, 4, 7}, New Double() {4, 8, 9, 12, 15}})
                              Dim request As New SpreadsheetAnalysisRequest(SpreadsheetAnalysisKind.OneWayAnova, data, options)
                              Dim result = SharedAnalysisApplicationService.Run(request)
                              Dim detail = DirectCast(result.Detail, OneWayAnovaApplicationResult)
                              Assert.AreEqual(options.IncludeWelch, detail.HasWelch)
                              Assert.AreEqual(options.IncludeFisherLsd, detail.FisherLsdPlotData IsNot Nothing)
                              Assert.AreEqual(options.IncludeBonferroni, detail.BonferroniPlotData IsNot Nothing)
                              Assert.AreEqual(options.IncludeTukeyKramer, detail.TukeyKramerPlotData IsNot Nothing)
                              Assert.AreEqual(options.IncludeGamesHowell, detail.GamesHowellPlotData IsNot Nothing)
                              CollectionAssert.AreEqual(New String() {"B", "A", "C"}, detail.GroupNames)
                              CollectionAssert.AreEqual(New Integer() {4, 3, 5}, detail.GroupCounts)
                              Assert.AreEqual(detail.PValue, CDbl(detail.Table.Values(1, 5)))
                              Assert.IsTrue(detail.Table.PvalueColumns.Contains(5))
                              If mask = 0 Then Assert.AreEqual(1, detail.Tables.Count, "No optional analysis")
                          End Using
                      Finally
                          CultureInfo.CurrentCulture = saved
                      End Try
                  Next
              End Sub)
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)> <DataRow(6)> <DataRow(7)>
    <DataRow(8)> <DataRow(9)> <DataRow(10)> <DataRow(11)> <DataRow(12)> <DataRow(13)> <DataRow(14)> <DataRow(15)>
    Public Sub ActualVarianceFunction_16Combinations_OrderAndPValueMetadata(mask As Integer)
        OnSta(Sub()
                  Using form As New UibyIDControlProjection("KW", CInt(HelpTopic.KruskalWallisTest))
                      Dim names = {"ckFlignerKilleen", "ckLevene", "ckSquaredRanks", "ckBartlett"}
                      Dim labels = {"Fligner-Killeen", "Levene", "Squared Ranks", "Bartlett"}
                      For bit As Integer = 0 To 3
                          Toggle(form, names(bit), (mask And (1 << bit)) <> 0)
                      Next
                      Dim input As New MultiGroupsUnpairedData With {.X = New Double()() {
                          New Double() {1, 2, 4, 6}, New Double() {2, 4, 7, 9}, New Double() {4, 8, 10, 15}}, .varNames = {"B", "A", "C"}}
                      Dim table = DirectCast(form.GetType().GetMethod("ComputeVarianceHomogeneity", BindingFlags.Instance Or BindingFlags.NonPublic).
                          Invoke(form, New Object() {input}), ResultTable)
                      If mask = 0 Then
                          Assert.AreEqual(1, table.TotalRows, "Existing empty ResultTable normalizes to a 1x1 matrix")
                          Assert.AreEqual(0, table.HeadersTopCount)
                          Assert.AreEqual(0, table.PvalCells.Count)
                          Assert.IsNull(table.ToOutputModel().Values(0, 0))
                      Else
                          Dim model = table.ToOutputModel()
                          Dim expected = Enumerable.Range(0, 4).Where(Function(i) (mask And (1 << i)) <> 0).Select(Function(i) labels(i)).ToArray()
                          ' Legacy HorizontalStackArrays appends rows, despite its name.
                          Dim actual = Enumerable.Range(0, expected.Length).Select(Function(i) CStr(model.Values(1 + i * 3, 0))).ToArray()
                          CollectionAssert.AreEqual(expected, actual)
                          Assert.AreEqual(1 + 3 * expected.Length, model.RowCount)
                          Assert.AreEqual(2, model.ColumnCount)
                          CollectionAssert.AreEqual(Enumerable.Range(1, expected.Length).Select(Function(i) i * 3).ToArray(),
                              model.PvalueCells.Select(Function(cell) cell.BodyRow).ToArray())
                          Assert.IsTrue(model.PvalueCells.All(Function(cell) cell.BodyColumn = 2))
                      End If
                  End Using
              End Sub)
    End Sub

    <TestMethod>
    Public Sub HelpResolverAndSubscriptionInventory_NoBrowserLaunch()
        OnSta(Sub()
                  Assert.AreEqual(HelpTopic.Home, HelpContext.ResolveTopic(Nothing))
                  Using form As New UibyIDControlProjection("Characterization", CInt(HelpTopic.NormalityTests))
                      Dim button = Field(form, "btnHelp")
                      Dim key = GetType(Control).GetField("EventClick", BindingFlags.Static Or BindingFlags.NonPublic).GetValue(Nothing)
                      Dim events = DirectCast(GetType(System.ComponentModel.Component).GetProperty("Events", BindingFlags.Instance Or BindingFlags.NonPublic).
                          GetValue(button, Nothing), System.ComponentModel.EventHandlerList)
                      Assert.AreEqual(1, events(key).GetInvocationList().Length)
                      HelpWiring.WireHelp(form, button)
                      Assert.AreEqual(2, events(key).GetInvocationList().Length, "Existing helper is not idempotent; no shipping repeated binding claimed")
                      form.Tag = HelpTopic.NormalityTests
                      Assert.AreEqual(HelpTopic.NormalityTests, HelpContext.ResolveTopic(form))
                      form.Tag = "OneWayANOVA"
                      Assert.AreEqual(HelpTopic.OneWayANOVA, HelpContext.ResolveTopic(form))
                      form.Tag = "not-a-topic"
                      Assert.AreEqual(HelpTopic.Home, HelpContext.ResolveTopic(form))
                      HelpWiring.WireHelp(DirectCast(Nothing, Form), button)
                      HelpWiring.WireHelp(form, DirectCast(Nothing, Control))
                      ' No OnClick/PerformClick for help: actual handler would open a browser.
                  End Using
              End Sub)
    End Sub
End Class
