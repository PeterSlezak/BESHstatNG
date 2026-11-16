Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports ExcelDna.Integration.CustomUI
Namespace Harness45
Friend Module FakeTrace45
 Friend ReadOnly Events As New List(Of String)
 Friend RibbonXml As String
 Friend FailAt As String
 Friend Sub Record(value As String)
  Events.Add(value)
  If FailAt = value Then Throw New InvalidOperationException("synthetic fault")
 End Sub
 Friend Sub Reset()
  Events.Clear() : FailAt = Nothing : FakeGlobals45.app = New FakeApp45()
  FakeDna45.WindowHandle = New IntPtr(45) : FakeForm45.Charts = True
 End Sub
End Module
Friend Class FakeRibbonBase45
 Public Overridable Function GetCustomUI(id As String) As String
  Return Nothing
 End Function
End Class
Friend Class FakeSheet45
End Class
Friend Class FakeBooks45
 Friend Count As Integer = 1
 Friend Sub Add()
  FakeTrace45.Record("Workbooks.Add") : Count += 1
 End Sub
End Class
Friend Class FakeApp45
 Friend Workbooks As New FakeBooks45()
 Friend ActiveSheet As New FakeSheet45()
End Class
Friend Module FakeGlobals45
 Friend app As New FakeApp45()
End Module
Friend Module FakeDna45
 Friend WindowHandle As IntPtr
End Module
Friend Module FakeAsync45
 Friend Sub QueueAsMacro(action As Action)
  FakeTrace45.Record("Queue") : action()
 End Sub
End Module
Friend Module FakeHelp45
 Friend Const BaseUrl As String = "synthetic-help"
 Friend Sub OpenUrl(value As String)
  FakeTrace45.Record("Help:" & value)
 End Sub
End Module
Friend Module FakeMessage45
 Friend Sub Show(ParamArray args() As Object)
  FakeTrace45.Record("Message")
 End Sub
End Module
Friend Class FakeWindow45
 Friend Sub New(value As IntPtr)
  FakeTrace45.Record("Window")
 End Sub
End Class
Friend Class FakeForm45
 Friend Shared Charts As Boolean = True
 Friend Sub New(ParamArray args() As Object)
  FakeTrace45.Record("Construct:" & Me.GetType().Name)
 End Sub
 Friend Sub Show(ParamArray args() As Object)
  FakeTrace45.Record("Show:" & Me.GetType().Name)
 End Sub
 Friend Sub Populate(sheet As FakeSheet45)
  FakeTrace45.Record("Populate")
 End Sub
 Friend Shared Function WorkbookHasAnyCharts(app As FakeApp45) As Boolean
  FakeTrace45.Record("Charts") : Return Charts
 End Function
End Class
Friend Class Ui01BulletChart
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui01ConvexHullPlot
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui01DumbbellPlot
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui01LadderPlot
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui01PolarPlot
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui01SankeyPlot
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui0OneRefeditMulticol
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui10ShowLog
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui11AboutAddin
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui11PCA
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui12GlobalSettings
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui12SampleSizeTtestSingleProp
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui13GEE
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui18MMRM
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui19LMM
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui20PropensityScoreMatching
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui21ControlCharts
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui22MultivariateControlCharts
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui3XYZplot
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui4Cox
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui4KMandLogRank
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui82x2
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui8Proportions
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui99ExportChart
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class Ui9ANOVA2nested
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class UiGLM
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class UiTwoInputRefedits
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class UibyID
 Inherits FakeForm45
 Friend Sub New(ParamArray args() As Object)
  MyBase.New(args)
 End Sub
End Class
Friend Class RibbonProjection45
    Inherits FakeRibbonBase45

    Friend Shared XllName As String = Nothing
    Friend Shared XllPath As String = Nothing

    Public Overrides Function GetCustomUI(RibbonID As String) As String
        Return FakeTrace45.RibbonXml ' The name here is the resource name that the ribbon xml has in the BESHStatResources resource file
    End Function

    '--------------------------------------------------------------------------
    ' Assumptions
    '--------------------------------------------------------------------------
    Public Sub OnbtmNormalityTestsPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Normality", HelpTopic.NormalityTests)
        mwForm.Show()
    End Sub

    Public Sub OnbtmOutliersPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Outliers", HelpTopic.UnivariateOutliers)
        mwForm.Show()
    End Sub

    Public Sub OnbtmHomogeneityVarPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Homogeneity of Variance", HelpTopic.HomogeneityOfVariance)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSymmetryPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Symmetry", HelpTopic.Symmetry)
        mwForm.Show()
    End Sub

    Public Sub OnbtmDescriptivePressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Descriptive Statistcs", HelpTopic.DescriptiveStatistics)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Graphics
    '--------------------------------------------------------------------------
    Public Sub OnbtmHistogramPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Histogram", HelpTopic.Histogram)
        mwForm.Show()
    End Sub

    Public Sub OnbtmBoxWhiskersPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Box and Whiskers", HelpTopic.BoxAndWhiskers)
        mwForm.Show()
    End Sub

    Public Sub OnbtmROCPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("ROC Curve", HelpTopic.ROCCurve)
        mwForm.Show()
    End Sub

    Public Sub OnbtmKMPressed(control As IRibbonControl)
        Dim mwForm As New Ui4KMandLogRank("Kaplan-Meier Plot", HelpTopic.KaplanMeierPlot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmNormalPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Normal Plot", HelpTopic.NormalPlot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmXYZPressed(control As IRibbonControl)
        Dim mwForm As New Ui3XYZplot(HelpTopic.XYZ3DScatterplot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmScatterPlotMatPressed(control As IRibbonControl)
        Dim sh As FakeSheet45

        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Scatter Plot MatrixType", HelpTopic.ScatterPlotMatrix, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmPolarPressed(control As IRibbonControl)
        Dim mwForm As New Ui01PolarPlot(HelpTopic.Polarplot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCHPPressed(control As IRibbonControl)
        Dim mwForm As New Ui01ConvexHullPlot(HelpTopic.ConvexHull)
        mwForm.Show()
    End Sub

    Public Sub OnbtmKitePressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Kite Chart", HelpTopic.KiteChart)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCatHistogramPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Histogram - Categorical", HelpTopic.CategoricalHistogram)
        mwForm.Show()
    End Sub

    Public Sub OnbtmViolinPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Violin Plot", HelpTopic.ViolinPlot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSankeyPressed(control As IRibbonControl)
        Dim mwForm As New Ui01SankeyPlot(HelpTopic.SankeyPlot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmDumbbellPressed(control As IRibbonControl)
        Dim mwForm As New Ui01DumbbellPlot(HelpTopic.DumbbellPlot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmLadderPressed(control As IRibbonControl)
        Dim mwForm As New Ui01LadderPlot(HelpTopic.LadderPlot)
        mwForm.Show()
    End Sub

    Public Sub OnbtmBulletPressed(control As IRibbonControl)
        Dim mwForm As New Ui01BulletChart(HelpTopic.BulletChart)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCDFPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Cumulative Distribution Function Plot", HelpTopic.CDFplot)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Parametric
    '--------------------------------------------------------------------------
    Public Sub OnbtmPairedTPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Paired T-test", HelpTopic.PairedSingleSampleTTests)
        mwForm.Show()
    End Sub

    Public Sub OnbtmUnpairedTPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Unpaired T-test", HelpTopic.UnpairedTTest)
        mwForm.Show()
    End Sub

    Public Sub Onbtm1WayANOVAPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("One-Way ANOVA", HelpTopic.OneWayANOVA)
        mwForm.Show()
    End Sub
    Public Sub Onbtm1RMANOVAPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("One-Way Repeated-Measures ANOVA", HelpTopic.OneWayRepeatedMeasuresANOVA)
        mwForm.Show()
    End Sub
    Public Sub Onbtm2WayNestedPressed(control As IRibbonControl)
        Dim mwForm As New Ui9ANOVA2nested("Two-Way Nested ANOVA", HelpTopic.TwoWayNestedANOVA)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Non-Parametric
    '--------------------------------------------------------------------------
    Public Sub OnbtmMannWhitneyPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Mann-Whitney Test", HelpTopic.MannWhitneyTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmWilcoxonPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Wilcoxon Signed Rank Test", HelpTopic.WilcoxonSignedRankTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmKruskalWallisPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Kruskal-Wallis Test", HelpTopic.KruskalWallisTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmFriedmanPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Friedman Test", HelpTopic.FriedmanTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCochranPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Cochran's Q Test", HelpTopic.CochranSQTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSkillingsMackPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Skillings-Mack Test", HelpTopic.SkillingsMackTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSpearmanPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Spearman Rank Correlation", HelpTopic.SpearmanRankCorrelation)
        mwForm.Show()
    End Sub

    Public Sub OnbtmKendallPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Kendall's Rank Correlation", HelpTopic.KendallSRankCorrelation)
        mwForm.Show()
    End Sub

    Public Sub OnbtmTheilSenPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Theil-Sen Simple Regression", HelpTopic.TheilSenSimpleRegression)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Contingency Tables
    '--------------------------------------------------------------------------
    Public Sub Onbtm2x2Pressed(control As IRibbonControl)
        Dim mwForm As New Ui82x2(HelpTopic.T2x2Table)
        mwForm.Show()
    End Sub
    Public Sub OnbtmRxCPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("RxC Table", HelpTopic.RxCTable)
        mwForm.Show()
    End Sub

    Public Sub OnbtmMantelPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Mantel-Haenszel Test", HelpTopic.MantelHaenszelTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmProportionsPressed(control As IRibbonControl)
        Dim mwForm As New Ui8Proportions(HelpTopic.Proportions)
        mwForm.Show()
    End Sub


    '--------------------------------------------------------------------------
    ' Survival
    '--------------------------------------------------------------------------
    Public Sub OnbtmLogrankPressed(control As IRibbonControl)
        Dim mwForm As New Ui4KMandLogRank("Logrank Test", HelpTopic.LogrankTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCoxPressed(control As IRibbonControl)
        Dim sh As FakeSheet45, mwForm As New Ui4Cox(HelpTopic.CoxRegression)

        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If

        mwForm.Populate(sh)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Regression
    '--------------------------------------------------------------------------
    Public Sub OnbtmLMPressed(control As IRibbonControl)
        Me.RegStart("Multiple Linear Regression (LM)")
    End Sub

    Public Sub OnbtmGLMPressed(control As IRibbonControl)
        Me.RegStart("Generalized Linear Models")
    End Sub

    Public Sub OnbtmGEEPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui13GEE("Generalized Estimating Equations", HelpTopic.GeneralizedEstimatingEquationsGEE)
        mwForm.Populate(sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmGLMNB2Pressed(control As IRibbonControl)
        Me.RegStart("Negative Binomial Regression (NB2)")
    End Sub

    Public Sub OnbtmZeroPressed(control As IRibbonControl)
        Me.RegStart("Zero-Inflated Poisson Regression")
    End Sub

    Public Sub OnbtmMultiLogitPressed(control As IRibbonControl)
        Me.RegStart("Multinomial Logistic Regression")
    End Sub

    Public Sub OnbtmOrdLogitPressed(control As IRibbonControl)
        Me.RegStart("Ordinal Logistic Regression")
    End Sub

    Private Sub RegStart(strTitle As String)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim tagn As Integer = 0
        If strTitle = "Multiple Linear Regression (LM)" Then
            tagn = HelpTopic.MultipleLinearRegressionLM
        ElseIf strTitle = "Generalized Linear Models" Then
            tagn = HelpTopic.GeneralizedLinearModelsGLM
        ElseIf strTitle = "Negative Binomial Regression (NB2)" Then
            tagn = HelpTopic.NegativeBinomialRegressionNB2
        ElseIf strTitle = "Zero-Inflated Poisson Regression" Then
            tagn = HelpTopic.ZeroInflatedPoissonRegression
        ElseIf strTitle = "Multinomial Logistic Regression" Then
            tagn = HelpTopic.MultinomialLogisticRegression
        ElseIf strTitle = "Ordinal Logistic Regression" Then
            tagn = HelpTopic.OrdinalLogisticRegression
        End If
        Dim mwForm As New UiGLM(strTitle, tagn)
        mwForm.Populate(sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmMMRMPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui18MMRM("Mixed Models for Repeated Measures (MMRM)", HelpTopic.MixedModelsForRepeatedMeasuresMMRM)
        mwForm.Populate(sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmLMMPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui19LMM("Liner Mixed Models (LMM)", HelpTopic.LinearMixedModelsLMM)
        mwForm.Populate(sh)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Multivariate
    '--------------------------------------------------------------------------
    Public Sub OnbtmHottelingPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Hotelling's T-Squared Test", HelpTopic.HotellingSTSquaredTest)
        mwForm.Show()
    End Sub

    Public Sub OnbtmPCAPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Principal Component Analysis", HelpTopic.PrincipalComponentAnalysis, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCAPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Correspondence Analysis", HelpTopic.CorrespondenceAnalysis)
        mwForm.Show()
    End Sub

    Public Sub OnbtmMCAPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Multiple Correspondence Analysis", HelpTopic.MultipleCorrespondenceAnalysis, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmKMCPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("K-Means Clustering", HelpTopic.KMeansClustering, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmHCPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Hierarchical Clustering", HelpTopic.HierarchicalClustering, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmFAPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Factor Analysis", HelpTopic.FactorAnalysis, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmDiscrPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Discriminant Analysis", HelpTopic.DiscriminantAnalysis, sh)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Sample Size
    '--------------------------------------------------------------------------
    Public Sub OnbtmSampleSizePairedTPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Paired T-test", HelpTopic.PairedTTest)
        mwForm.Show()
    End Sub
    Public Sub OnbtmSampleSizeUnPairedTPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Unpaired T-test", HelpTopic.UnpairedTTest)
        mwForm.Show()
    End Sub
    Public Sub OnbtmSinglePropPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Single Proportion", HelpTopic.SingleProportion)
        mwForm.Show()
    End Sub
    Public Sub OnbtmIndPropPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Independent Proportions", HelpTopic.IndependentProportions)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSSlogrankPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Log-rank Test", HelpTopic.SSlogrank)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSScoxPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Cox Regression", HelpTopic.SScox)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSSiccPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Intraclass Correlation (ICC)", HelpTopic.SSicc)
        mwForm.Show()
    End Sub

    Public Sub OnbtmSSblandaltmanPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Agreement (Bland-Altman)", HelpTopic.SSblandaltman)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Agreement
    '--------------------------------------------------------------------------
    Public Sub OnbtmPassingBablok(control As IRibbonControl)
        Dim mwForm As New Ui9ANOVA2nested("Passing-Bablok Regression", HelpTopic.PassingBablokRegression)
        mwForm.Show()
    End Sub

    Public Sub OnbtmDeming(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Deming Regression", HelpTopic.DemingRegression)
        mwForm.Show()
    End Sub

    Public Sub OnbtmIcc(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Intraclass Correlation Coefficients", HelpTopic.IntraclassCorrelationCoefficients)
        mwForm.Show()
    End Sub

    Public Sub OnbtmBlandAltman(control As IRibbonControl)
        Dim mwForm As New Ui9ANOVA2nested("Bland–Altman Analysis", HelpTopic.BlandAltman)
        mwForm.Show()
    End Sub

    Public Sub OnbtmLCCC(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Lin's Concordance Correlation Coefficient", HelpTopic.LinsCCC)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCohenKappa(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Cohen's / Weighted Kappa", HelpTopic.CohensKappa)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Causal Inference
    '--------------------------------------------------------------------------

    Public Sub OnbtmPSMPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui20PropensityScoreMatching("Propensity Score Matching", HelpTopic.PropensityScoreMatching)
        mwForm.Populate(sh)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Statistical process control
    '--------------------------------------------------------------------------

    Public Sub OnbtmControlChartsPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui21ControlCharts("Control Charts", HelpTopic.ControlCharts)
        mwForm.Show()
    End Sub

    Public Sub OnbtmMultControlChartsPressed(control As IRibbonControl)
        Dim sh As FakeSheet45
        If FakeGlobals45.app.Workbooks.Count > 0 Then
            sh = FakeGlobals45.app.ActiveSheet
        Else
            FakeGlobals45.app.Workbooks.Add()
            sh = FakeGlobals45.app.ActiveSheet
        End If
        Dim mwForm As New Ui22MultivariateControlCharts("Multivariate Control Charts", HelpTopic.MultivariateControlCharts)
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Ribbon Buttons
    '--------------------------------------------------------------------------
    Public Sub OnbtmAboutPressed(control As IRibbonControl)
        Dim mwForm As New Ui11AboutAddin()
        mwForm.Show()
    End Sub

    Public Sub OnbtmHelpPressed(control As IRibbonControl)
        FakeHelp45.OpenUrl(FakeHelp45.BaseUrl & "/")
    End Sub

    Public Sub OnbtmShowLogPressed(control As IRibbonControl)
        Dim mwForm As New Ui10ShowLog()
        mwForm.Show()
    End Sub

    Public Sub OnbtmSettingsPressed(control As IRibbonControl)
        Dim mwForm As New Ui12GlobalSettings(HelpTopic.GlobalSettings)
        mwForm.Show()
    End Sub

    'Chart exporting
    Private Shared _exportChartForm As Ui99ExportChart

    Public Sub OnbtmShowExportChart(control As IRibbonControl)

        FakeAsync45.QueueAsMacro(Sub()

                                                             Dim app = FakeGlobals45.app
                                                             If Not Ui99ExportChart.WorkbookHasAnyCharts(app) Then
                                                                 FakeMessage45.Show("The active workbook contains no embedded charts and no chart sheets.",
                                                                                 "Export Chart",
                                                                                 0,
                                                                                 0)
                                                                 Return
                                                             End If

                                                             Dim hwnd As IntPtr = FakeDna45.WindowHandle
                                                             Dim f As New Ui99ExportChart(HelpTopic.ExportChart)
                                                             f.Show(New FakeWindow45(hwnd))

                                                         End Sub)
    End Sub

End Class

End Namespace
