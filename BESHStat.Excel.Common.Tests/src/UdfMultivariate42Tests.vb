Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.Multivariate
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfMultivariate42Tests
    Friend Shared Function Data() As Double(,)
        Dim a(23, 2) As Double
        For i = 0 To 23
            For j = 0 To 2
                a(i, j) = Math.Sin(i * (j + 1) * 0.7) + i * 0.2 + (j + 1) * Math.Cos(i * 0.3 + j)
            Next
        Next
        Return a
    End Function

    Friend Shared Sub EqualArrays(a As Object(,), b As Object(,))
        Assert.AreEqual(a.GetLength(0), b.GetLength(0))
        Assert.AreEqual(a.GetLength(1), b.GetLength(1))
        For i = 0 To a.GetLength(0) - 1
            For j = 0 To a.GetLength(1) - 1
                If TypeOf a(i, j) Is Double AndAlso TypeOf b(i, j) Is Double Then
                    Assert.AreEqual(BitConverter.DoubleToInt64Bits(CDbl(a(i, j))), BitConverter.DoubleToInt64Bits(CDbl(b(i, j))))
                Else
                    Assert.AreEqual(a(i, j), b(i, j))
                End If
            Next
        Next
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub FactorRealEngineKeepsNamesContributionsAndCopyIsolation(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim model As New FactorAnalysis()
            Dim names() As String = {"2.000000000000001", "dup", "dup"}
            model.dataInputs(Data(), Enumerable.Range(90, 24).Reverse().ToArray(), names, "compatibility")
            model.settingsInputs(600, 0.0000001, FactorAnalysisMatrixType.Correlation, FactorAnalysisExtractionMethod.PrincipalComponents, FactorAnalysisRetentionMethod.Fixed, 2, FactorAnalysisRotationMethod.None, FactorAnalysisScoreMethod.None)
            model.Calculate()
            Dim expected = UdfMultivariateReportPreparation38.BuildFactorCommunalities(model, names, model.FactorNames("Factor "), model.CommunalityContributionsByFactor(), 3, 2, True)
            Dim actual = UdfMultivariateReportPreparation38.BuildFactorCommunalities(model, names, Function() True)
            EqualArrays(expected, actual)
            actual(1, 0) = "mutated"
            EqualArrays(expected, UdfMultivariateReportPreparation38.BuildFactorCommunalities(model, names, Function() True))
            Assert.AreEqual("2.000000000000001", names(0))
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <TestMethod>
    Public Sub FactorFailurePrecedesHeaderReader()
        Dim read As Boolean = False
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfMultivariateReportPreparation38.BuildFactorCommunalities(Nothing, Nothing, Function()
                                                                                                                                               read = True
                                                                                                                                               Return True
                                                                                                                                           End Function))
        Assert.IsFalse(read)
    End Sub

    <DataTestMethod, DataRow(True), DataRow(False)>
    Public Sub HierarchicalRealFitKeepsLocalIdsAndOnlyReadsSelectedCut(height As Boolean)
        Dim model As New HierarchicalClustering()
        model.dataInputs(Data(), Enumerable.Range(0, 24).Select(Function(i) If(i Mod 3 = 0, "dup", "row" & (90 - i).ToString(CultureInfo.InvariantCulture))).ToArray(), {"a", "b", "c"})
        model.settingsInputs() : model.Fit()
        Dim countRead As Boolean = False, heightRead As Boolean = False
        Dim actual = UdfMultivariateReportPreparation38.BuildHierarchicalMembership(model.Result, If(height, HierarchicalMembershipDisplayMode.ByHeight, HierarchicalMembershipDisplayMode.ByClusterCount), Function()
                                                                                                                                                                                                            heightRead = True
                                                                                                                                                                                                            Return 0.0
                                                                                                                                                                                                        End Function, Function()
                                                                                                                                                                                                                          countRead = True
                                                                                                                                                                                                                          Return 3
                                                                                                                                                                                                                      End Function)
        EqualArrays(If(height, model.Result.GetMembershipTableByHeight(0), model.Result.GetMembershipTable(3)), actual)
        Assert.AreEqual(height, heightRead) : Assert.AreEqual(Not height, countRead)
    End Sub

    <DataTestMethod, DataRow("TRAINING"), DataRow("APPARENT"), DataRow("CV")>
    Public Sub DiscriminantRealFitRetainsPredictionAliasesAndTableCopy(token As String)
        Dim model As New DiscriminantAnalysis()
        model.dataInputs(Data(), Enumerable.Range(0, 24).Select(Function(i) CType(If(i < 12, "B", "A"), Object)).ToArray(), Nothing, {"a", "b", "c"})
        model.settingsInputs(covarianceRegularization:=0.00000001) : model.Fit()
        Assert.AreSame(If(token = "CV", model.ValidationClassification, model.TrainingClassification), UdfMultivariateReportPreparation38.SelectDiscriminantPrediction(model, token))
        Assert.IsNull(MultivariateAnalysisUDFsReports38.FindWrappedDiscriminantTablePrepared(model.wrapResults(), "not present", False, Function(x) Convert.ToString(x, CultureInfo.InvariantCulture)))
        Assert.ThrowsException(Of ArgumentException)(Sub() UdfMultivariateReportPreparation38.SelectDiscriminantPrediction(model, "invalid"))
    End Sub

    <TestMethod>
    Public Sub NullDiscriminantDoesNotReadOrRejectSource()
        Assert.IsNull(UdfMultivariateReportPreparation38.SelectDiscriminantPrediction(Nothing, "invalid"))
    End Sub
End Class
