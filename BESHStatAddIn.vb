Option Explicit On
Imports System.IO
Imports System.Runtime.InteropServices 'needed for the <ComVisible(True)>
Imports System.Windows.Forms
Imports ExcelDna.Integration 'app = ExcelDnaUtil.Application
Imports ExcelDna.Integration.CustomUI 'needed for the Inherits ExcelRibbon
Imports Microsoft.Office.Interop.Excel 'Dim app As Application
Imports NLog
Imports NLog.Targets

Public Class BESHStatAddIn
    Implements IExcelAddIn

    Public Sub AutoOpen() Implements IExcelAddIn.AutoOpen
        ' Store this for access from anywhere in my workflows: 
        ' https://groups.google.com/g/exceldna/c/1rScvDdeVOk/m/euij1L-VihoJ
        ExcelIntegration.RegisterUnhandledExceptionHandler(AddressOf UnhandledExceptionHandler)
        BESHstatGlobals.app = ExcelDnaUtil.Application
        BESHstatGlobals.gXllName = DirectCast(XlCall.Excel(XlCall.xlGetName), String)
        BESHstatGlobals.gXllPath = Path.GetDirectoryName(BESHstatGlobals.gXllName)
        BESHstatGlobals.gLogFile = BESHstatGlobals.gXllPath & "\Logs\all.log"
        'recreate log file (warning/error files content is kept)
        Dim LogFileStream = New FileStream(BESHStatNG.gLogFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)
        LogFileStream.Dispose()
        BESHstatGlobals.gLogger = NLog.LogManager.GetCurrentClassLogger()
        BESHstatGlobals.gLogger.Info("Logger started")
    End Sub

    Public Sub AutoClose() Implements IExcelAddIn.AutoClose
        BESHstatGlobals.app = Nothing
    End Sub

    Private Function UnhandledExceptionHandler(exception As Object) As Object
        Dim erForm As New Ui10ShowLog(DirectCast(exception, Exception))
        erForm.Show()
        Return ExcelError.ExcelErrorValue
    End Function
End Class

<ComVisible(True)>
Public Class Ribbon
    Inherits ExcelRibbon

    Friend Shared XllName As String = Nothing
    Friend Shared XllPath As String = Nothing

    Public Overrides Function GetCustomUI(RibbonID As String) As String
        Return BESHStatNG.My.Resources.RibbonXML ' The name here is the resource name that the ribbon xml has in the BESHStatResources resource file
    End Function

    '--------------------------------------------------------------------------
    ' Assumptions
    '--------------------------------------------------------------------------
    Public Sub OnbtmNormalityTestsPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Normality")
        mwForm.Tag = HelpTopic.NormalityTests
        mwForm.Show()
    End Sub

    Public Sub OnbtmOutliersPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Outliers")
        mwForm.Tag = HelpTopic.UnivariateOutliers
        mwForm.Show()
    End Sub

    Public Sub OnbtmHomogeneityVarPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Homogeneity of Variance")
        mwForm.Tag = HelpTopic.HomogeneityOfVariance
        mwForm.Show()
    End Sub

    Public Sub OnbtmSymmetryPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Symmetry")
        mwForm.Tag = HelpTopic.Symmetry
        mwForm.Show()
    End Sub

    Public Sub OnbtmDescriptivePressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Descriptive Statistcs")
        mwForm.Tag = HelpTopic.DescriptiveStatistics
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Graphics
    '--------------------------------------------------------------------------
    Public Sub OnbtmHistogramPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Histogram")
        mwForm.Tag = HelpTopic.Histogram
        mwForm.Show()
    End Sub

    Public Sub OnbtmBoxWhiskersPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Box and Whiskers")
        mwForm.Tag = HelpTopic.BoxAndWhiskers
        mwForm.Show()
    End Sub

    Public Sub OnbtmROCPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("ROC Curve")
        mwForm.Tag = HelpTopic.ROCCurve
        mwForm.Show()
    End Sub

    Public Sub OnbtmKMPressed(control As IRibbonControl)
        Dim mwForm As New Ui4KMandLogRank("Kaplan-Meier Plot")
        mwForm.Tag = HelpTopic.KaplanMeierPlot
        mwForm.Show()
    End Sub

    Public Sub OnbtmNormalPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Normal Plot")
        mwForm.Tag = HelpTopic.NormalPlot
        mwForm.Show()
    End Sub

    Public Sub OnbtmXYZPressed(control As IRibbonControl)
        Dim mwForm As New Ui3XYZplot()
        mwForm.Tag = HelpTopic.XYZ3DScatterplot
        mwForm.Show()
    End Sub

    Public Sub OnbtmScatterPlotMatPressed(control As IRibbonControl)
        Dim sh As Worksheet

        If BESHstatGlobals.app.Workbooks.Count > 0 Then
            sh = BESHstatGlobals.app.ActiveSheet
        Else
            BESHstatGlobals.app.Workbooks.Add()
            sh = BESHstatGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Scatter Plot Matrix", sh)
        mwForm.Tag = HelpTopic.ScatterPlotMatrix
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Parametric
    '--------------------------------------------------------------------------
    Public Sub OnbtmPairedTPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Paired T-test")
        mwForm.Tag = HelpTopic.PairedSingleSampleTTests
        mwForm.Show()
    End Sub

    Public Sub OnbtmUnpairedTPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Unpaired T-test")
        mwForm.Tag = HelpTopic.UnpairedTTest
        mwForm.Show()
    End Sub

    Public Sub Onbtm1WayANOVAPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("One-Way ANOVA")
        mwForm.Tag = HelpTopic.OneWayANOVA
        mwForm.Show()
    End Sub
    Public Sub Onbtm1RMANOVAPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("One-Way Repeated-Measures ANOVA")
        mwForm.Tag = HelpTopic.OneWayRepeatedMeasuresANOVA
        mwForm.Show()
    End Sub
    Public Sub Onbtm2WayNestedPressed(control As IRibbonControl)
        Dim mwForm As New Ui9ANOVA2nested
        mwForm.Tag = HelpTopic.TwoWayNestedANOVA
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Non-Parametric
    '--------------------------------------------------------------------------
    Public Sub OnbtmMannWhitneyPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Mann-Whitney Test")
        mwForm.Tag = HelpTopic.MannWhitneyTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmWilcoxonPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Wilcoxon Signed Rank Test")
        mwForm.Tag = HelpTopic.WilcoxonSignedRankTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmKruskalWallisPressed(control As IRibbonControl)
        Dim mwForm As New UibyID("Kruskal-Wallis Test")
        mwForm.Tag = HelpTopic.KruskalWallisTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmFriedmanPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Friedman Test")
        mwForm.Tag = HelpTopic.FriedmanTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmCochranPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Cochran's Q Test")
        mwForm.Tag = HelpTopic.CochranSQTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmSkillingsMackPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Skillings-Mack Test")
        mwForm.Tag = HelpTopic.SkillingsMackTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmSpearmanPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Spearman Rank Correlation")
        mwForm.Tag = HelpTopic.SpearmanRankCorrelation
        mwForm.Show()
    End Sub

    Public Sub OnbtmKendallPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Kendall's Rank Correlation")
        mwForm.Tag = HelpTopic.KendallSRankCorrelation
        mwForm.Show()
    End Sub

    Public Sub OnbtmTheilSenPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Theil-Sen Simple Regression")
        mwForm.Tag = HelpTopic.TheilSenSimpleRegression
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Contingency Tables
    '--------------------------------------------------------------------------
    Public Sub Onbtm2x2Pressed(control As IRibbonControl)
        Dim mwForm As New Ui82x2()
        mwForm.Tag = HelpTopic.T2x2Table
        mwForm.Show()
    End Sub
    Public Sub OnbtmRxCPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("RxC Table")
        mwForm.Tag = HelpTopic.RxCTable
        mwForm.Show()
    End Sub

    Public Sub OnbtmMantelPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Mantel-Haenszel Test")
        mwForm.Tag = HelpTopic.MantelHaenszelTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmProportionsPressed(control As IRibbonControl)
        Dim mwForm As New Ui8Proportions()
        mwForm.Tag = HelpTopic.Proportions
        mwForm.Show()
    End Sub


    '--------------------------------------------------------------------------
    ' Survival
    '--------------------------------------------------------------------------
    Public Sub OnbtmLogrankPressed(control As IRibbonControl)
        Dim mwForm As New Ui4KMandLogRank("Logrank Test")
        mwForm.Tag = HelpTopic.LogrankTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmCoxPressed(control As IRibbonControl)
        Dim sh As Worksheet, mwForm As New Ui4Cox()

        If BESHstatGlobals.app.Workbooks.Count > 0 Then
            sh = BESHstatGlobals.app.ActiveSheet
        Else
            BESHstatGlobals.app.Workbooks.Add()
            sh = BESHstatGlobals.app.ActiveSheet
        End If
        mwForm.Tag = HelpTopic.CoxRegression
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
        Dim sh As Worksheet
        If BESHstatGlobals.app.Workbooks.Count > 0 Then
            sh = BESHstatGlobals.app.ActiveSheet
        Else
            BESHstatGlobals.app.Workbooks.Add()
            sh = BESHstatGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui13GEE("Generalized Estimating Equations")
        mwForm.Tag = HelpTopic.GeneralizedEstimatingEquationsGEE
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
        Dim sh As Worksheet
        If BESHstatGlobals.app.Workbooks.Count > 0 Then
            sh = BESHstatGlobals.app.ActiveSheet
        Else
            BESHstatGlobals.app.Workbooks.Add()
            sh = BESHstatGlobals.app.ActiveSheet
        End If
        Dim mwForm As New UiGLM(strTitle)
        If strTitle = "Multiple Linear Regression (LM)" Then
            mwForm.Tag = HelpTopic.MultipleLinearRegressionLM
        ElseIf strTitle = "Generalized Linear Models" Then
            mwForm.Tag = HelpTopic.GeneralizedLinearModelsGLM
        ElseIf strTitle = "Negative Binomial Regression (NB2)" Then
            mwForm.Tag = HelpTopic.NegativeBinomialRegressionNB2
        ElseIf strTitle = "Zero-Inflated Poisson Regression" Then
            mwForm.Tag = HelpTopic.ZeroInflatedPoissonRegression
        ElseIf strTitle = "Multinomial Logistic Regression" Then
            mwForm.Tag = HelpTopic.MultinomialLogisticRegression
        ElseIf strTitle = "Ordinal Logistic Regression" Then
            mwForm.Tag = HelpTopic.OrdinalLogisticRegression
        End If
        mwForm.Populate(sh)
        mwForm.Show()

    End Sub

    '--------------------------------------------------------------------------
    ' Multivariate
    '--------------------------------------------------------------------------
    Public Sub OnbtmHottelingPressed(control As IRibbonControl)
        Dim mwForm As New UiTwoInputRefedits("Hotelling's T-Squared Test")
        mwForm.Tag = HelpTopic.HotellingSTSquaredTest
        mwForm.Show()
    End Sub

    Public Sub OnbtmPCAPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If BESHstatGlobals.app.Workbooks.Count > 0 Then
            sh = BESHstatGlobals.app.ActiveSheet
        Else
            BESHstatGlobals.app.Workbooks.Add()
            sh = BESHstatGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Principal Component Analysis", sh)
        mwForm.Tag = HelpTopic.PrincipalComponentAnalysis
        mwForm.Show()
    End Sub

    Public Sub OnbtmCAPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Correspondence Analysis")
        mwForm.Tag = HelpTopic.CorrespondenceAnalysis
        mwForm.Show()
    End Sub

    Public Sub OnbtmMCAPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If BESHstatGlobals.app.Workbooks.Count > 0 Then
            sh = BESHstatGlobals.app.ActiveSheet
        Else
            BESHstatGlobals.app.Workbooks.Add()
            sh = BESHstatGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Multiple Correspondence Analysis", sh)
        mwForm.Tag = HelpTopic.MultipleCorrespondenceAnalysis
        mwForm.Show()
    End Sub

    '--------------------------------------------------------------------------
    ' Sample Size
    '--------------------------------------------------------------------------
    Public Sub OnbtmSampleSizePairedTPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Paired T-test")
        mwForm.Tag = HelpTopic.PairedTTest
        mwForm.Show()
    End Sub
    Public Sub OnbtmSampleSizeUnPairedTPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Unpaired T-test")
        mwForm.Tag = HelpTopic.UnpairedTTest
        mwForm.Show()
    End Sub
    Public Sub OnbtmSinglePropPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Single Proportion")
        mwForm.Tag = HelpTopic.SingleProportion
        mwForm.Show()
    End Sub
    Public Sub OnbtmIndPropPressed(control As IRibbonControl)
        Dim mwForm As New Ui12SampleSizeTtestSingleProp("Sample Size - Independent Proportions")
        mwForm.Tag = HelpTopic.IndependentProporions
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
        HelpLinks.OpenUrl(HelpLinks.BaseUrl & "/")
    End Sub

    Public Sub OnbtmShowLogPressed(control As IRibbonControl)
        Dim mwForm As New Ui10ShowLog()
        mwForm.Show()
    End Sub

    Public Sub OnbtmSettingsPressed(control As IRibbonControl)
        Dim mwForm As New Ui12GlobalSettings()
        mwForm.Show()
    End Sub

End Class
