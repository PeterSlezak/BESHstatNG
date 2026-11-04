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

Public NotInheritable Class Spc21Fixture
    Public Shared ReadOnly Charts As SpcChartType() = {SpcChartType.Individuals, SpcChartType.MovingRange, SpcChartType.IndividualsMovingRange, SpcChartType.XBar, SpcChartType.SubgroupRange, SpcChartType.SubgroupStandardDeviation, SpcChartType.XBarR, SpcChartType.XBarS, SpcChartType.PChart, SpcChartType.NpChart, SpcChartType.CChart, SpcChartType.UChart, SpcChartType.Cusum, SpcChartType.Ewma, SpcChartType.MovingAverage}
    Public Const DatasetSha As String = "7ea168ca8e7812a2bf9b925d5261413f20a057f79a087414b4032c08b2492a2b"
    Public Shared Function Wide() As Double(,)
        Return New Double(,) {{49.585R,49.78R,49.915R,49.995R,50.13R},
{49.78R,49.975R,50.065R,50.19R,50.325R},
{49.875R,50.025R,50.16R,50.285R,50.375R},
{49.665R,49.86R,49.995R,50.075R,50.21R},
{49.81R,50.005R,50.095R,50.22R,50.355R},
{49.635R,49.785R,49.92R,50.045R,50.135R},
{49.735R,49.93R,50.065R,50.145R,50.28R},
{49.84R,50.035R,50.125R,50.25R,50.385R},
{49.665R,49.815R,49.95R,50.075R,50.165R},
{49.705R,49.9R,50.035R,50.115R,50.25R},
{49.6R,49.795R,49.885R,50.01R,50.145R},
{49.795R,49.945R,50.08R,50.205R,50.295R},
{49.845R,50.04R,50.175R,50.255R,50.39R},
{49.68R,49.875R,49.965R,50.09R,50.225R},
{49.825R,49.975R,50.11R,50.235R,50.325R},
{49.605R,49.8R,49.935R,50.015R,50.15R},
{49.75R,49.945R,50.035R,50.16R,50.295R},
{49.855R,50.005R,50.14R,50.265R,50.355R},
{49.635R,49.83R,49.965R,50.045R,50.18R},
{49.72R,49.915R,50.005R,50.13R,50.265R},
{49.615R,49.765R,49.9R,50.025R,50.115R},
{49.765R,49.96R,50.095R,50.175R,50.31R},
{49.86R,50.055R,50.145R,50.27R,50.405R},
{49.695R,49.845R,49.98R,50.105R,50.195R},
{49.795R,49.99R,50.125R,50.205R,50.34R},
{49.62R,49.815R,49.905R,50.03R,50.165R},
{49.765R,49.915R,50.05R,50.175R,50.265R},
{49.825R,50.02R,50.155R,50.235R,50.37R},
{49.65R,49.845R,49.935R,50.06R,50.195R},
{49.735R,49.885R,50.02R,50.145R,50.235R},
{50.205R,50.4R,50.535R,50.615R,50.75R},
{50.4R,50.595R,50.685R,50.81R,50.945R},
{50.495R,50.645R,50.78R,50.905R,50.995R},
{50.285R,50.48R,50.615R,50.695R,50.83R},
{50.43R,50.625R,50.715R,50.84R,50.975R},
{50.255R,50.405R,50.54R,50.665R,50.755R},
{50.355R,50.55R,50.685R,50.765R,50.9R},
{50.46R,50.655R,50.745R,50.87R,51.005R},
{50.285R,50.435R,50.57R,50.695R,50.785R},
{50.325R,50.52R,50.655R,50.735R,50.87R},
{50.22R,50.415R,50.505R,50.63R,50.765R},
{51.565R,51.715R,51.85R,51.975R,52.065R},
{50.465R,50.66R,50.795R,50.875R,51.01R},
{50.3R,50.495R,50.585R,50.71R,50.845R},
{50.445R,50.595R,50.73R,50.855R,50.945R},
{50.225R,50.42R,50.555R,50.635R,50.77R},
{50.37R,50.565R,50.655R,50.78R,50.915R},
{50.475R,50.625R,50.76R,50.885R,50.975R},
{50.255R,50.45R,50.585R,50.665R,50.8R},
{50.34R,50.535R,50.625R,50.75R,50.885R},
{50.235R,50.385R,50.52R,50.645R,50.735R},
{50.385R,50.58R,50.715R,50.795R,50.93R},
{50.48R,50.675R,50.765R,50.89R,51.025R},
{50.315R,50.465R,50.6R,50.725R,50.815R},
{50.415R,50.61R,50.745R,50.825R,50.96R},
{50.24R,50.435R,50.525R,50.65R,50.785R},
{50.385R,50.535R,50.67R,50.795R,50.885R},
{50.445R,50.64R,50.775R,50.855R,50.99R},
{50.27R,50.465R,50.555R,50.68R,50.815R},
{50.355R,50.505R,50.64R,50.765R,50.855R}}
    End Function
    Public Shared Function Individuals() As Double()
        Return New Double() {49.82R,50.08R,49.95R,50.22R,49.89R,50.04R,50.16R,49.8R,50.02R,50.11R,49.92R,50.19R,49.86R,50.06R,49.99R,50.15R,49.91R,53.6R,50.1R,49.83R,50.05R,50.21R,49.88R,50R,50.13R,49.94R,50.17R,49.85R,50.07R,49.97R,50.28R,50.36R,50.31R,50.42R,50.34R,50.45R,50.38R,50.33R,50.47R,50.4R,50.35R,51.25R,50.39R,50.44R,50.3R,50.41R,50.37R,50.48R,50.32R,50.43R,50.4R,50.34R,50.46R,50.36R,50.49R,50.38R,50.44R,50.33R,50.47R,50.41R}
    End Function
    Public Shared Function Stacked() As Double()
        Return New Double() {49.66R,49.91R,50.07R,49.84R,50.09R,50.25R,49.92R,50.17R,50.33R,49.74R,49.99R,50.15R,49.87R,50.12R,50.28R,49.68R,49.93R,50.09R,49.81R,50.06R,50.22R,49.9R,50.15R,50.31R,49.71R,49.96R,50.12R,49.78R,50.03R,50.19R,50.44R,50.69R,50.85R,50.62R,50.87R,51.03R,50.7R,50.95R,51.11R,50.52R,50.77R,50.93R,51.5R,51.75R,51.91R,50.46R,50.71R,50.87R,50.59R,50.84R,51R,50.68R,50.93R,51.09R,50.49R,50.74R,50.9R,50.56R,50.81R,50.97R}
    End Function
    Public Shared Function Counts() As Double()
        Return New Double() {2R,1R,3R,2R,2R,1R,4R,2R,3R,1R,2R,3R,2R,1R,3R,2R,1R,3R,2R,2R,1R,4R,2R,3R,1R,2R,3R,2R,1R,3R,4R,5R,4R,6R,5R,4R,6R,5R,4R,5R,5R,12R,6R,4R,5R,4R,5R,4R,6R,5R,4R,6R,5R,4R,5R,5R,12R,6R,4R,5R}
    End Function
    Public Shared Function SampleSizes() As Double()
        Return New Double() {100R,92R,108R,84R,116R,96R,104R,88R,112R,100R,90R,110R,100R,92R,108R,84R,116R,96R,104R,88R,112R,100R,90R,110R,100R,92R,108R,84R,116R,96R,104R,88R,112R,100R,90R,110R,100R,92R,108R,84R,116R,96R,104R,88R,112R,100R,90R,110R,100R,92R,108R,84R,116R,96R,104R,88R,112R,100R,90R,110R}
    End Function
    Public Shared Function Exposures() As Double()
        Return New Double() {100R,80R,120R,90R,110R,100R,85R,115R,95R,105R,100R,80R,120R,90R,110R,100R,85R,115R,95R,105R,100R,80R,120R,90R,110R,100R,85R,115R,95R,105R,100R,80R,120R,90R,110R,100R,85R,115R,95R,105R,100R,80R,120R,90R,110R,100R,85R,115R,95R,105R,100R,80R,120R,90R,110R,100R,85R,115R,95R,105R}
    End Function
    Public Shared Function Ids(n As Integer) As String()
        Return Enumerable.Range(0, n).Select(Function(i) "G" & (i \ 3).ToString(CultureInfo.InvariantCulture)).ToArray()
    End Function
    Public Shared Function Input(chart As SpcChartType, edge As Integer) As SpcInputData
        Dim n = 60, sources = Enumerable.Range(2, n).ToArray(), labels = Enumerable.Range(0,n).Select(Function(i) If(i Mod 3 = 0, "", "same Ω")).ToArray()
        Dim seq = Enumerable.Range(1,n).Select(Function(i) CDbl(i)).ToArray()
        If edge = 12 Then sources = sources.Select(Function(v) 101 + v * 3).Reverse().ToArray()
        If Spc21LegacyPreparation.IsAttributeChart(chart) Then
            Dim c = Counts(), sizes = If(chart = SpcChartType.NpChart, Enumerable.Repeat(100.0R,n).ToArray(), SampleSizes()), exp = Exposures()
            Select Case edge
                Case 0 : c(5) = Double.NaN
                Case 1 : c(5) = Double.PositiveInfinity
                Case 2 : c(5) = -1
                Case 3 : c(5) = 0.25R
                Case 4 : c(5) = 1001
                Case 5 : sizes(5) = 0 : exp(5) = 0
                Case 6 : sizes = Nothing : exp = Nothing
                Case 7 : c = New Double() {1} : sizes = New Double() {10} : exp = New Double() {2} : labels = Nothing : seq = Nothing : sources = Nothing
            End Select
            Return SpcInputData.FromAggregatedCounts(c, If(chart = SpcChartType.PChart OrElse chart = SpcChartType.NpChart,sizes,Nothing), If(chart = SpcChartType.UChart,exp,Nothing), labels,seq,sources)
        ElseIf Spc21LegacyPreparation.IsSubgroupChart(chart) Then
            If edge = 13 OrElse edge = 14 OrElse edge = 15 Then
                Dim ids = Spc21Fixture.Ids(n), values = Stacked()
                If edge = 14 Then ids = Enumerable.Range(0,n).Select(Function(i) If(i Mod 2 = 0, " A ", "a")).ToArray()
                If edge = 15 Then ids(2) = " "
                Return SpcInputData.FromStackedObservations(values,ids,labels,seq,sources,"μ")
            End If
            Dim x = Wide()
            Select Case edge
                Case 0 : x(5,2)=Double.NaN
                Case 1 : x(5,2)=Double.NegativeInfinity
                Case 2 : For i=0 To n-1 : For j=0 To 4 : x(i,j)=50 : Next : Next
                Case 3 : For i=0 To n-1 : For j=0 To 4 : x(i,j)=Double.NaN : Next : Next
                Case 4 : x=New Double(,) {{1.0R,2.0R}} : labels=Nothing : seq=Nothing : sources=Nothing
                Case 5 : x=New Double(,) {{1.0R},{2.0R},{3.0R}} : labels=Nothing : seq=Nothing : sources=Nothing
                Case 6 : x=New Double(,) {} : labels=Nothing : seq=Nothing : sources=Nothing
                Case 7 : labels=New String() {"wrong length"}
            End Select
            Return SpcInputData.FromWideSubgroups(x,labels,seq,sources,New String() {"x","x","μ"," x ","V"})
        Else
            Dim values=Individuals()
            Select Case edge
                Case 0 : values(5)=Double.NaN
                Case 1 : values(5)=Double.PositiveInfinity
                Case 2 : values=Enumerable.Repeat(50.0R,n).ToArray()
                Case 3 : values=Enumerable.Repeat(Double.NaN,n).ToArray()
                Case 4 : values=New Double() {50} : labels=Nothing : seq=Nothing : sources=Nothing
                Case 5 : values=New Double() {} : labels=Nothing : seq=Nothing : sources=Nothing
                Case 6 : seq(2)=Double.NaN
                Case 7 : seq=New Double() {1}
            End Select
            Return SpcInputData.FromIndividualSequence(values,labels,seq,sources,"μ")
        End If
    End Function
    Public Shared Function CatchAction(action As Action) As Exception
        Try
            action() : Return Nothing
        Catch ex As Exception
            If TypeOf ex Is AssertFailedException Then Throw
            Return ex
        End Try
    End Function
    Public Shared Sub SameException(a As Exception,b As Exception)
        Assert.IsNotNull(a) : Assert.IsNotNull(b)
        Assert.AreEqual(a.GetType(),b.GetType()) : Assert.AreEqual(a.Message,b.Message)
    End Sub
    Public Shared Function Text(value As Object) As String
        If value Is Nothing Then Return "<Nothing>"
        If TypeOf value Is Double Then Return CDbl(value).ToString("R",CultureInfo.InvariantCulture)
        Return Convert.ToString(value,CultureInfo.InvariantCulture)
    End Function
    Public Shared Function Hash(value As Object) As String
        Dim sb As New StringBuilder
        Encode(sb,value)
        Using h=SHA256.Create()
            Return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "").ToLowerInvariant()
        End Using
    End Function
    Private Shared Sub Encode(sb As StringBuilder,value As Object)
        If value Is Nothing Then
            sb.Append("null;") : Return
        End If
        Dim t=value.GetType() : sb.Append(t.FullName).Append(":"c)
        If TypeOf value Is String OrElse t.IsPrimitive OrElse t.IsEnum OrElse TypeOf value Is Decimal OrElse TypeOf value Is DateTime Then
            Dim str=Text(value) : sb.Append(str.Length).Append(":"c).Append(str).Append(";"c) : Return
        End If
        If TypeOf value Is Array Then
            Dim a=DirectCast(value,Array)
            For i=0 To a.Rank-1 : sb.Append(a.GetLength(i)).Append(","c) : Next
        End If
        If TypeOf value Is IEnumerable Then
            For Each item As Object In DirectCast(value,IEnumerable) : Encode(sb,item) : Next
        Else
            For Each p In t.GetProperties().Where(Function(v) v.CanRead AndAlso v.GetIndexParameters().Length=0).OrderBy(Function(v) v.Name,StringComparer.Ordinal)
                sb.Append(p.Name) : Encode(sb,p.GetValue(value,Nothing))
            Next
        End If
        sb.Append(";"c)
    End Sub
    Public Shared Function Tables(result As SpcFitResult) As List(Of ResultTable)
        Dim r=SpcResultTables.BuildSummaryTables(result)
        r.AddRange(SpcResultTables.BuildChartDataTables(result,False)) : r.AddRange(SpcResultTables.BuildSignalTables(result)) : r.AddRange(SpcResultTables.BuildAuditTables(result))
        Return r
    End Function
    Public Shared Function TableHash(tables As IEnumerable(Of ResultTable)) As String
        Dim models As New List(Of ResultTableOutputModel)
        For Each table In tables
            Dim m=table.ToOutputModel(), a=DirectCast(m.Values.Clone(),Object(,))
            For row=0 To a.GetLength(0)-1
                If a.GetLength(1)>1 AndAlso TypeOf a(row,0) Is String AndAlso {"Execution time (ms)","Execution started (UTC)","Execution completed (UTC)"}.Contains(CStr(a(row,0))) Then
                    a(row,1)="VOLATILE:" & If(a(row,1) Is Nothing,"null",a(row,1).GetType().FullName)
                End If
            Next
            models.Add(New ResultTableOutputModel(a,m.HeaderTopRows,m.HeaderLeftColumns,m.FooterRows,m.PvalueColumns,m.TitleRows,m.IsResultTable,m.PvalueCells))
        Next
        Return Hash(models)
    End Function
    Public Shared Sub Emit(root As XElement)
        Console.WriteLine("04A21_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(root.ToString(SaveOptions.DisableFormatting))))
    End Sub
End Class
