Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ProportionsPolicyTests
    <DataTestMethod> <DataRow(0)> <DataRow(45)> <DataRow(50)> <DataRow(100)>
    Public Sub Single_ExactLegacySnapshotAndCap(responders As Integer)
        Dim request = ProportionsInputPolicy.Validate(ProportionsMode.SingleSample, IndependentProportionsHypothesis.Superiority, 100, responders, Double.NaN, Double.NaN, Double.NaN, 0.05, Double.NaN).Data
        Dim result = ProportionsReportPolicy.Analyze(request)
        ContingencyPolicyTests.SameTables(ContingencyLegacyCharacterization.Proportions(request), result)
        Assert.AreEqual(contingencytable.SingleProportion(responders,100).Estimate, CDbl(result.Tables(0).Body(2,1)))
        Assert.IsTrue(CDbl(result.Tables(0).Body(4,1)) <= 1)
        If responders = 50 Then Assert.AreEqual(1.0, CDbl(result.Tables(0).Body(4,1)))
    End Sub

    <DataTestMethod> <DataRow(0, "")> <DataRow(1, "")> <DataRow(2, "")> <DataRow(0, "sk-SK")> <DataRow(1, "sk-SK")> <DataRow(2, "sk-SK")>
    Public Sub Independent_AllHypothesesLegacyTablesSignsCIAndMarkers(hypothesis As Integer, culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each alpha In {0.05,0.1}
                Dim request = ProportionsInputPolicy.Validate(ProportionsMode.Independent, CType(hypothesis,IndependentProportionsHypothesis), 100,45,80,52,0,alpha,0.3).Data
                Dim result = ProportionsReportPolicy.Analyze(request)
                ContingencyPolicyTests.SameTables(ContingencyLegacyCharacterization.Proportions(request), result)
                CollectionAssert.AreEqual(New Integer() {45,52,55,28}, request.IndependentFisherTable().Cast(Of Integer)().ToArray())
                If hypothesis = 0 Then
                    Assert.AreEqual(-0.2, CDbl(result.Tables(0).Body(6,1)),0.000000000001)
                    Assert.AreEqual(contingencytable.TwoIndependentProportions(45,100,52,80,alpha).LowerLimit, result.Metrics.Single(Function(m) m.Name="independent.lower").Value)
                Else
                    Assert.AreEqual(0.2, CDbl(result.Tables(0).Body(6,1)),0.000000000001)
                    Assert.AreEqual($"{100*(1-2*alpha):0.##}% CI for proportion difference", CStr(result.Tables(0).Body(9,0)))
                    Dim swapped = ProportionsInputPolicy.Validate(ProportionsMode.Independent, CType(hypothesis,IndependentProportionsHypothesis),80,52,100,45,0,alpha,0.3).Data
                    Assert.AreEqual(-0.2, CDbl(ProportionsReportPolicy.Analyze(swapped).Tables(0).Body(6,1)),0.000000000001)
                End If
            Next
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)> <DataRow(6)> <DataRow(7)> <DataRow(8)> <DataRow(9)> <DataRow(10)>
    Public Sub InvalidModeCountsAlphaAndMarginsProduceNoRequest(kind As Integer)
        Dim mode = ProportionsMode.Independent, hypothesis = IndependentProportionsHypothesis.Noninferiority
        Dim n1 As Double = 100, r1 As Double = 45, n2 As Double = 80, r2 As Double = 52, alpha As Double = 0.05, margin As Double = 0.1
        Select Case kind
            Case 0 : n1=0
            Case 1 : r1=101
            Case 2 : r1=-1
            Case 3 : r2=1.5
            Case 4 : margin=0
            Case 5 : margin=-1
            Case 6 : alpha=0.5
            Case 7 : alpha=Double.NaN
            Case 8 : n2=CDbl(Integer.MaxValue)+1
            Case 9 : mode=CType(99,ProportionsMode)
            Case 10 : hypothesis=CType(99,IndependentProportionsHypothesis)
        End Select
        Dim input = ProportionsInputPolicy.Validate(mode,hypothesis,n1,r1,n2,r2,0,alpha,margin)
        Assert.IsFalse(input.Success)
        Assert.IsNull(input.Data)
        Assert.IsTrue(input.Validation.Issues.All(Function(i) i.FieldPath.Length>0))
    End Sub

    <TestMethod>
    Public Sub IndependentZeroRespondersAndDisabledMarginPolicy()
        Dim request = ProportionsInputPolicy.Validate(ProportionsMode.Independent,IndependentProportionsHypothesis.Superiority,20,0,30,12,0,0.05,Double.NaN)
        Assert.IsTrue(request.Success)
        Assert.IsTrue(ProportionsReportPolicy.Analyze(request.Data).Success)
        Assert.IsTrue(ProportionsInputPolicy.ValidateMargin(IndependentProportionsHypothesis.Superiority, Double.NaN).IsValid)
        Assert.IsFalse(ProportionsInputPolicy.ValidateMargin(IndependentProportionsHypothesis.Equivalence,Double.PositiveInfinity).IsValid)
        Dim degenerate = ProportionsInputPolicy.Validate(ProportionsMode.Independent,IndependentProportionsHypothesis.Noninferiority,20,0,30,0,0,0.05,0.1)
        Dim result = ProportionsReportPolicy.Analyze(degenerate.Data)
        Assert.IsFalse(result.Success) ' Existing Core requires nonzero SE. No invented CI repair.
        Assert.AreEqual(0,result.Tables.Count)
    End Sub

    <DataTestMethod> <DataRow(20,15,40)> <DataRow(0,0,0)> <DataRow(20,30,50)>
    Public Sub PairedDisjointCountContractAndPureSemanticProjection(firstOnly As Integer,secondOnly As Integer,both As Integer)
        Dim input = ProportionsInputPolicy.Validate(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,100,firstOnly,0,secondOnly,both,0.05,0)
        Assert.IsTrue(input.Success)
        CollectionAssert.AreEqual(New Integer() {both,firstOnly,secondOnly,100-firstOnly-secondOnly-both},input.Data.PairedOutcomeTable().Cast(Of Integer)().ToArray())
        Assert.AreEqual(100,input.Data.PairedOutcomeTable().Cast(Of Integer)().Sum())
        Assert.IsFalse(ProportionsReportPolicy.Analyze(input.Data).Success, "No silent choice between conflicting scientific mappings")
        Dim interval=contingencytable.PairedProportions(100,firstOnly,secondOnly,both)
        Dim test As New BESHStatNG.TestResult With {.Pvalue=0.123}
        Dim projected=ProportionsReportPolicy.ProjectPaired(input.Data,interval,test)
        Assert.IsTrue(projected.Success)
        Assert.AreEqual(0.123,CDbl(projected.Tables(0).Body(8,1)))
        Assert.AreEqual(interval.Estimate,CDbl(projected.Tables(0).Body(6,1)))
        Assert.AreEqual(9,projected.Tables(0).ToCoreTable().ToOutputModel().PvalueCells(0).BodyRow)
    End Sub

    <TestMethod>
    Public Sub PairedScientificCandidate_MinimalDiscordanceIsNotLegacyBothNeither()
        ' SC-PROPORTIONS-PAIRED-01: executable discrepancy, NOT a new approved p-value golden.
        Dim request=ProportionsInputPolicy.Validate(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,100,20,0,15,40,0.05,0).Data
        Dim expectedExclusive=request.PairedOutcomeTable()
        Dim legacy As Integer(,)={{20-40,40},{100-20-15+40,15-40}}
        Assert.AreEqual(-20,legacy(0,0))
        Assert.AreEqual(-25,legacy(1,1))
        Dim legacyP=contingencytable.Liddell_McNemar(legacy).Item1.Pvalue
        Dim discordantP=contingencytable.Liddell_McNemar(expectedExclusive).Item1.Pvalue
        Assert.IsTrue(Math.Abs(legacyP-discordantP)>0.01,"Candidate needs owner correction decision, not parity approval")
        Assert.IsFalse(ProportionsInputPolicy.Validate(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,100,60,0,50,0,0.05,0).Success)
        Assert.IsFalse(ProportionsReportPolicy.ProjectPaired(request,Nothing,Nothing).Success)
    End Sub

    <TestMethod>
    Public Sub ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted()
        Dim outside = ProportionsInputPolicy.Validate(ProportionsMode.Independent, IndependentProportionsHypothesis.Equivalence,100,45,80,52,0,0.05,0.15)
        Assert.IsTrue(outside.Success)
        Dim result=ProportionsReportPolicy.Analyze(outside.Data)
        Assert.IsFalse(result.Success)
        Assert.AreEqual(0,result.Tables.Count)
        Assert.AreEqual("proportions.core",result.Validation.Issues(0).Code)
        ' Existing Core Integer intermediate overflow, not a new golden or corrected formula.
        Assert.ThrowsException(Of OverflowException)(Function() contingencytable.PairedProportions(1000,100,80,300))
    End Sub

    <TestMethod>
    Public Sub ExclusiveModeChoicesAndFreshRequestsHaveNoSharedMutableState()
        Assert.AreEqual(ProportionsMode.SingleSample,ProportionsInputPolicy.ModeFromChoices(True,False,False))
        Assert.AreEqual(ProportionsMode.Independent,ProportionsInputPolicy.ModeFromChoices(False,True,False))
        Assert.AreEqual(ProportionsMode.Paired,ProportionsInputPolicy.ModeFromChoices(False,False,True))
        Assert.AreEqual(-1,CInt(ProportionsInputPolicy.ModeFromChoices(False,False,False)))
        Assert.AreEqual(-1,CInt(ProportionsInputPolicy.ModeFromChoices(True,True,False)))
        Threading.Tasks.Parallel.For(0,16,Sub(index)
            Dim request=ProportionsInputPolicy.Validate(ProportionsMode.SingleSample,IndependentProportionsHypothesis.Superiority,100,index,0,0,0,0.05,0)
            Assert.AreEqual(index,request.Data.Responders1)
            Assert.IsTrue(ProportionsReportPolicy.Analyze(request.Data).Success)
        End Sub)
    End Sub
End Class
