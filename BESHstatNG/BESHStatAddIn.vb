Option Explicit On

Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices 'needed for the <ComVisible(True)>
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration 'app = ExcelDnaUtil.Application
Imports ExcelDna.Integration.CustomUI 'needed for the Inherits ExcelRibbon
Imports Microsoft.Office.Interop.Excel 'Dim app As Application

Public Class BESHStatAddIn
    Implements IExcelAddIn
    Public Sub AutoOpen() Implements IExcelAddIn.AutoOpen
        ' Store this for access from anywhere in my workflows: 
        ' https://groups.google.com/g/exceldna/c/1rScvDdeVOk/m/euij1L-VihoJ
        ExcelIntegration.RegisterUnhandledExceptionHandler(AddressOf UnhandledExceptionHandler)

        AppGlobals.app = ExcelDnaUtil.Application
        AppGlobals.gXllName = DirectCast(XlCall.Excel(XlCall.xlGetName), String)
        AppGlobals.gXllPath = Path.GetDirectoryName(AppGlobals.gXllName)
        AppGlobals.gLogFile = Path.Combine(AppGlobals.gXllPath, "Logs", "all.log")
        ExcelDnaHost.ConfigureCoreServicesForExcelDna(AppGlobals.gsAPP_TITLE)
        AppGlobals.gLogger = New SimpleFileLogger(AppGlobals.gXllPath, GetType(BESHStatAddIn).FullName, resetTraceLog:=True)

        Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Info($"AutoOpen starting. Version={AppGlobals.gAddinVersion}; Build={AppGlobals.GetBuildDateIso()}; XllPath={AppGlobals.gXllPath}")

        Try
            AppGlobals.gSettingsStore = New AppInfrastructure.BeshStatNgSettingsStore(AppGlobals.gXllPath)
            AppGlobals.ApplySettings(AppGlobals.gSettingsStore.Load())
            Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Info("Settings loaded successfully.")
        Catch ex As Exception
            AppGlobals.ApplySettings(New AppInfrastructure.BeshStatNgSettings())
            Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Error(ex, "Failed to load settings. Defaults were applied for this session.")
        End Try

        Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Info("Trace execution logging enabled = " & AppGlobals.TraceExecutionLoggingEnabled.ToString())

        Try
            BESHStatUpdate.AutoUpdate.Start(4000)
            Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Debug("Background update check scheduled.")
        Catch ex As Exception
            Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Error(ex, "Failed to schedule background update check.")
        End Try

        Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Info("AutoOpen completed.")
    End Sub

    Public Sub AutoClose() Implements IExcelAddIn.AutoClose
        Try
            Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Info("AutoClose starting.")

            If AppGlobals.gLogger IsNot Nothing Then
                AppGlobals.gLogger.Dispose()
                AppGlobals.gLogger = Nothing
            End If
        Catch ex As Exception
            Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Error(ex, "Error while closing the add-in.")
        Finally
            AppGlobals.app = Nothing
        End Try
    End Sub

    Private Function UnhandledExceptionHandler(exception As Object) As Object
        Dim ex As Exception = TryCast(exception, Exception)
        If ex Is Nothing Then
            ex = New ApplicationException("Unhandled non-exception object received from Excel-DNA: " & If(exception, "<null>").ToString())
        End If

        Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Error(ex, "Unhandled Excel-DNA exception")

        Try
            Dim erForm As New Ui10ShowLog(ex)
            erForm.Show()
        Catch uiEx As Exception
            Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Error(uiEx, "Failed to show the log window for an unhandled exception.")
            MsgBox("An unhandled error occurred: " & ex.Message & vbCrLf & "Check the log for more information.",
                   MsgBoxStyle.Exclamation,
                   AppGlobals.gsAPP_TITLE)
        End Try

        Return ExcelError.ExcelErrorValue
    End Function
End Class

<ComVisible(True)>
Public Class Ribbon
    Inherits ExcelRibbon

    Friend Shared XllName As String = Nothing
    Friend Shared XllPath As String = Nothing

    Public Overrides Function GetCustomUI(RibbonID As String) As String
        Return My.Resources.RibbonXML ' The name here is the resource name that the ribbon xml has in the BESHStatResources resource file
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
        Dim sh As Worksheet

        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Scatter Plot MatrixType", HelpTopic.ScatterPlotMatrix, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmPolarPressed(control As IRibbonControl)
        Dim mwForm As New Ui01PolarPlot(HelpTopic.Polarplot)
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
        Dim sh As Worksheet, mwForm As New Ui4Cox(HelpTopic.CoxRegression)

        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
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
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
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
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
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
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui18MMRM("Mixed Models for Repeated Measures (MMRM)", HelpTopic.MixedModelsForRepeatedMeasuresMMRM)
        mwForm.Populate(sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmLMMPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
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
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Principal Component Analysis", HelpTopic.PrincipalComponentAnalysis, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmCAPressed(control As IRibbonControl)
        Dim mwForm As New Ui0OneRefeditMulticol("Correspondence Analysis", HelpTopic.CorrespondenceAnalysis)
        mwForm.Show()
    End Sub

    Public Sub OnbtmMCAPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Multiple Correspondence Analysis", HelpTopic.MultipleCorrespondenceAnalysis, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmKMCPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("K-Means Clustering", HelpTopic.KMeansClustering, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmHCPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Hierarchical Clustering", HelpTopic.HierarchicalClustering, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmFAPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui11PCA("Factor Analysis", HelpTopic.FactorAnalysis, sh)
        mwForm.Show()
    End Sub

    Public Sub OnbtmDiscrPressed(control As IRibbonControl)
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
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
        Dim sh As Worksheet
        If AppGlobals.app.Workbooks.Count > 0 Then
            sh = AppGlobals.app.ActiveSheet
        Else
            AppGlobals.app.Workbooks.Add()
            sh = AppGlobals.app.ActiveSheet
        End If
        Dim mwForm As New Ui20PropensityScoreMatching("Propensity Score Matching", HelpTopic.PropensityScoreMatching)
        mwForm.Populate(sh)
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
        Dim mwForm As New Ui12GlobalSettings(HelpTopic.GlobalSettings)
        mwForm.Show()
    End Sub

    'Chart exporting
    Private Shared _exportChartForm As Ui99ExportChart

    Public Sub OnbtmShowExportChart(control As IRibbonControl)

        ExcelDna.Integration.ExcelAsyncUtil.QueueAsMacro(Sub()

                                                             Dim app = AppGlobals.app
                                                             If Not Ui99ExportChart.WorkbookHasAnyCharts(app) Then
                                                                 MessageBox.Show("The active workbook contains no embedded charts and no chart sheets.",
                                                                                 "Export Chart",
                                                                                 MessageBoxButtons.OK,
                                                                                 MessageBoxIcon.Information)
                                                                 Return
                                                             End If

                                                             Dim hwnd As IntPtr = ExcelDnaUtil.WindowHandle
                                                             Dim f As New Ui99ExportChart(HelpTopic.ExportChart)
                                                             f.Show(New ExcelWindowWrapper(hwnd))

                                                         End Sub)
    End Sub

End Class
