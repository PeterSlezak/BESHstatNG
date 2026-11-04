Option Explicit On
Option Strict On
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports System.Text.RegularExpressions

<TestClass>
Public Class Spc21AdapterTests
    Private Shared ReadOnly Pins As New Dictionary(Of String,String) From {
{"InitializeOptionControls","29e5ced83120de7e83267f684f51280f560884e0a7a6e60a7644017abc6e3aa5"},
{"UpdateRoleAvailability","e2f7bbb2e35acaa33f3183fa36df428eda10a678dd9a0f61e76c0c50f598c641"},
{"UpdateLimitOptions","a1b5c12834ddf8ca15fd07549cf2fea5b2e25d6f0e40a7d6fbebd2a1b8d656e9"},
{"UpdateHistoricalColumns","bb29005ac015cd4c5f0a6eca8ce09ee877391d677a8e416fe178b02a96895ca5"},
{"UpdateTimeWeightedControls","5d149b3f2625ddc6c2fc15b363ae85486c9b30a3033c2287e3a12cd305d5a157"},
{"UpdateRuleAvailability","3ed7218919fd0e768e804252f0d03218eb91e01a15356eba50016b4ce01d2756"},
{"RestoreCustomRules","376c2af6e427ad19044734c7a7b1c23784a7e6568f781ba38413b6b2459383ba"},
{"BuildSpecificationLimits","48b853922701445b8cb6ad6fb01481796c8d74c93a3b2771d20f21a1cca6811f"},
{"BuildChartParameters","bb55013ac97df1d8ea8976c8f93eb12188829dba8d26e6a0bc0c8d50eed0f61a"},
{"LoadSelectedRulePreset","ab145bbf162e03a18c942b25ffb5270b12573b42246e9d2e3aff88c755172ed0"},
{"btCompute_Click","86baa5a6773a4d4eba8e306b9892389a5f7b5dee83f178aabdbcc7365b2f41ef"},
{"WriteResultsToNewWorkbook","0e623c8c0c8c0907e100525e689126647c0f66484f05586561b9cd4b9e0b876c"},
{"ReadObjectColumn","771162451594b0ff835c4c338f6aeef46a00af98362c7e0901700a96092b5da8"},
{"ReadNumericColumn","1b910a26388d09d411ac45992413982c57018e68976b753c91fbb9a33b36080f"},
{"ReadTextColumn","082c938c008e26ac81b3457f12af5350416394dce11ef7f12deee86aceb88ec7"},
{"NormalizeWorksheetValue","08eeb42394fc366a38a1944556b44a2011045025207e6f46c20eb7649430e6dd"},
{"IsExclusionIndicator","932ebd562cc55215d472eb12f2cd1d5458f189d21478f23a542085f332dac0f7"},
{"ParseRuleKind","963d9ca8a625b16c5e6690920913d15dc15d7fbc0f2f76b703def0d2c15a216c"},
{"ParseRuleSide","5915d9f0e44eaff047b20b96f7a3a95fa840fe3180f93a35d2926ddf2ad5b50a"},
{"ParseRuleScope","1494a7966ce374dd9b765ccacc1a735a8c15fbbd43405d0ed9010ee71fcdf04b"}}
    Private Shared Function Source() As String
        Dim folder As New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        While folder IsNot Nothing AndAlso Not File.Exists(Path.Combine(folder.FullName,"AGENTS.md")) : folder=folder.Parent : End While
        If folder Is Nothing Then Throw New InvalidOperationException("Repository source root not found.")
        Return File.ReadAllText(Path.Combine(folder.FullName,"BESHstatNG/src/UI/Ui21ControlCharts.vb")).Replace(vbCrLf,vbLf)
    End Function
    Public Shared Function PreservedMembers() As IEnumerable(Of Object())
        Return Pins.Keys.Select(Function(n) New Object() {n})
    End Function
    <DataTestMethod><DynamicData(NameOf(PreservedMembers),DynamicDataSourceType.Method)>
    Public Sub NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes(name As String)
        Dim body=Regex.Match(Source(),"^    (?:Private|Public)[^\n]*\b(?:Sub|Function) "+name+"\(.*?^    End (?:Sub|Function)",RegexOptions.Multiline Or RegexOptions.Singleline).Value
        Using sha=SHA256.Create()
            Assert.AreEqual(Pins(name),BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(body))).Replace("-","").ToLowerInvariant())
        End Using
    End Sub
    <TestMethod>
    Public Sub RealWindowsPreparationConsumersAndSingleFitAreSourceBacked()
        Dim t=Source()
        For Each name In New String() {"SpcInputPointPolicy.FromWorksheetRows","SpcPhaseStagePolicy.Quick","SpcPhaseStagePolicy.Collapse","SpcPhaseStagePolicy.ImportedRows","SpcPhaseStagePolicy.Manual","SpcExclusionPolicy.Import","SpcExclusionPolicy.CoreIndex","New SpcLimitRequest","New SpcRuleRequest","SpcRequestPreparation.Analysis","SpcRequestPreparation.Create","SpcRequestPreparation.HistoricalRow"}
            StringAssert.Contains(t,name)
        Next
        Assert.AreEqual(1,Regex.Matches(t,"SpcEngine\.Fit\(").Count)
        Dim request=Regex.Match(t,"    Private Function BuildRequest\(.*?    End Function",RegexOptions.Singleline).Value
        Assert.AreEqual(1,Regex.Matches(request,"SpcEngine\.Validate\(").Count)
        Assert.IsTrue(request.IndexOf("BuildStagesFromGrid(input)",StringComparison.Ordinal)<request.IndexOf("BuildExclusionsFromGrid(input)",StringComparison.Ordinal))
        Assert.IsTrue(request.IndexOf("BuildHistoricalParameters()",StringComparison.Ordinal)<request.IndexOf("BuildSpecificationLimits(chartType)",StringComparison.Ordinal))
    End Sub
    Public Shared Iterator Function IndicatorCases() As IEnumerable(Of Object())
        Dim values As Object()={Nothing,False,True,0,1,-2,0.0R,-0.5R,"", "yes","no","Y","n","x","Exclude","excluded","include","included","true","false"," ? ","NaN","Infinity","1,5","1.5"}
        For i=0 To values.Length-1 : Yield New Object() {i,values(i)} : Next
    End Function
    <DataTestMethod><DynamicData(NameOf(IndicatorCases),DynamicDataSourceType.Method)>
    Public Sub FrozenHostIndicatorCoercionFeedsTypedSharedCollapse(id As Integer,value As Object)
        For Each name In New String() {"","sk-SK"}
            Dim prior=CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(name)
                Dim flag As Boolean
                Dim ex=Spc21Fixture.CatchAction(Sub() flag=Spc21LegacyPreparation.IsExclusionIndicator(value))
                If id=20 Then
                    Assert.IsInstanceOfType(ex, GetType(ArgumentException))
                Else
                    Assert.IsNull(ex)
                    Dim map=SpcInputPointPolicy.FromRows(SpcDataLayout.StackedObservations,{101,105,118,131},{"A","B","A","B"})
                    Dim captured=SpcExclusionPolicy.Import(map,{flag,False,False,False},{"first","","on false",""})
                    Assert.AreEqual(If(flag,1,0),captured.Count)
                    If flag Then Assert.AreEqual("first; on false",captured(0).Reason)
                End If
            Finally
                CultureInfo.CurrentCulture=prior
            End Try
        Next
    End Sub
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    <DataTestMethod><DataRow(0)><DataRow(6)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)><DataRow(12)><DataRow(13)><DataRow(14)>
    Public Sub CoreTablesFromPortableRequestUseExistingWindowsProcessorWithoutExcel(id As Integer)
        Dim request=Spc21Profile.Request(id,True), fit=SpcEngine.Fit(request), writer As New MemoryWriter
        Dim tables=Spc21Fixture.Tables(fit)
        Dim processor As New BESHStatNG.ProcessListofResultTables(tables)
        processor.writeToSheet(writer,True)
        Assert.IsTrue(writer.Blocks.Count>=tables.Count)
        For Each table In tables
            Dim model=table.ToOutputModel()
            Assert.IsTrue(writer.Blocks.Any(Function(b) b.Model.RowCount=model.RowCount AndAlso b.Model.ColumnCount=model.ColumnCount AndAlso Object.Equals(b.Model.Values(0,0),model.Values(0,0))))
        Next
    End Sub
End Class
