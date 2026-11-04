Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui11PCA

    Private pWorksheet As Worksheet
    Private pWorkbook As Workbook
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox
    Private AllColumnsInfo As Dictionary(Of String, VarColumnInfo) 'all available columns, including non-numeric, for optional row labels

    Sub New(analysis As String, tagn As Integer, ws As Worksheet)
        ' This call is required by the designer.
        InitializeComponent()
        pWorksheet = ws
        pWorkbook = ws.Parent
        Me.tbEps.Text = FormatUiDouble(0.000001)

        ' Add any initialization after the InitializeComponent() call.
        Me.Text = analysis
        Me.Tag = tagn
        Me.TabPageOptionsPCA.Parent = Nothing
        Me.TabPageOptionsSPM.Parent = Nothing
        Me.TabPageOptionsKmeans.Parent = Nothing
        Me.TabPageOptionsHierarchicalClustering.Parent = Nothing
        Me.TabPageOptionsFA.Parent = Nothing
        Me.TabPageOptionsDA.Parent = Nothing

        If Me.Tag = HelpTopic.ScatterPlotMatrix Then
            Me.TabPageOptionsSPM.Parent = Me.TabControl1

        ElseIf Me.Tag = HelpTopic.PrincipalComponentAnalysis Then
            Me.TabPageOptionsPCA.Parent = Me.TabControl1
            Me.lblGruppingVar.Visible = True
            Me.cbGruppingVar.Visible = True
            Me.lblRowLabel.Visible = True
            Me.cbRowLabel.Visible = True

        ElseIf Me.Tag = HelpTopic.MultipleCorrespondenceAnalysis Then
            Me.ckFirstRow.Visible = True

        ElseIf Me.Tag = HelpTopic.KMeansClustering Then
            Me.TabPageOptionsKmeans.Parent = Me.TabControl1
            Me.cbKmeansInitialization.Items.AddRange(New Object() {"K-Means++", "Forgy", "Random Partition", "User-Specified Centers"})
            Me.cbKmeansDistance.Items.AddRange(New Object() {"Squared Euclidean", "Euclidean"})
            Me.cbKmeansStandardization.Items.AddRange(New Object() {"None", "Z-scores", "Range 0 to 1"})
            Me.cbKmeansMissingPolicy.Items.AddRange(New Object() {"Error on missing", "Listwise deletion"})
            Me.cbKmeansEmptyCluster.Items.AddRange(New Object() {"Farthest observation", "Random observation", "Keep previous center"})
            Me.tbKmeansTolerance.Text = FormatUiDouble(0.000001)
            Me.tbKmeansSeed.Text = AppGlobals.GetDefaultRandomSeedText()
            Me.lblRowLabel.Visible = True
            Me.cbRowLabel.Visible = True

            Me.cbKmeansInitialization.SelectedIndex = 0
            Me.cbKmeansDistance.SelectedIndex = 0
            Me.cbKmeansStandardization.SelectedIndex = 0
            Me.cbKmeansMissingPolicy.SelectedIndex = 0
            Me.cbKmeansEmptyCluster.SelectedIndex = 0
            Me.refKmeansCenters.ExcelConnector = AppGlobals.app
            AddHandler Me.cbKmeansInitialization.SelectedIndexChanged, AddressOf Me.KMeansInitializationChanged
            Me.KMeansInitializationChanged(Me.cbKmeansInitialization, System.EventArgs.Empty)

        ElseIf Me.Tag = HelpTopic.HierarchicalClustering Then
            Me.TabPageOptionsHierarchicalClustering.Parent = Me.TabControl1
            Me.lblRowLabel.Visible = True
            Me.cbRowLabel.Visible = True
            Me.cbHierarchicalLinkage.Items.AddRange(New Object() {"Ward", "Complete", "Average", "Weighted Average", "Single Linkage", "Centroid", "Median"})
            Me.cbHierarchicalDistance.Items.AddRange(New Object() {"Squared Euclidean", "Euclidean", "Manhattan", "Chebyshev", "Minkowski", "Cosine", "Correlation"})
            Me.cbHierarchicalStandardization.Items.AddRange(New Object() {"None", "Z-scores", "Range 0 to 1"})
            Me.cbHierarchicalMissingPolicy.Items.AddRange(New Object() {"Error on missing", "Listwise deletion"})
            Me.cbHierarchicalHeightMode.Items.AddRange(New Object() {"Merge Distance", "Step Levels"})
            Me.cbHierarchicalOrientation.Items.AddRange(New Object() {"Top", "Bottom", "Left", "Right"})
            Me.cbHierarchicalLabelMode.Items.AddRange(New Object() {"Data Labels", "Axis Title", "None"})
            Me.tbHierarchicalMinkowskiPower.Text = FormatUiDouble(2.0)
            Me.tbHierarchicalCutHeight.Text = FormatUiDouble(0.0)
            Me.cbHierarchicalLinkage.SelectedIndex = 0
            Me.cbHierarchicalDistance.SelectedIndex = 0
            Me.cbHierarchicalStandardization.SelectedIndex = 0
            Me.cbHierarchicalMissingPolicy.SelectedIndex = 0
            Me.cbHierarchicalHeightMode.SelectedIndex = 0
            Me.cbHierarchicalOrientation.SelectedIndex = 0
            Me.cbHierarchicalLabelMode.SelectedIndex = 0

            AddHandler Me.cbHierarchicalDistance.SelectedIndexChanged, AddressOf Me.HierarchicalDistanceChanged
            AddHandler Me.optHierarchicalCutByClusters.CheckedChanged, AddressOf Me.HierarchicalMembershipModeChanged
            AddHandler Me.optHierarchicalCutByHeight.CheckedChanged, AddressOf Me.HierarchicalMembershipModeChanged
            Me.HierarchicalDistanceChanged(Me.cbHierarchicalDistance, System.EventArgs.Empty)
            Me.HierarchicalMembershipModeChanged(Me.optHierarchicalCutByClusters, System.EventArgs.Empty)

        ElseIf Me.Tag = HelpTopic.FactorAnalysis Then
            Me.TabPageOptionsFA.Parent = Me.TabControl1
            Me.cbFAExtraction.Items.AddRange(New Object() {"Principal Components", "Principal Axis", "Maximum Likelihood", "Generalized Least Squares", "Image Factoring", "Alpha Factoring"})
            Me.cbFACommunalityInit.Items.AddRange(New Object() {"Squared multiple correlations", "One / full diagonal"})
            Me.cbFAMissingPolicy.Items.AddRange(New Object() {"Error on missing", "Listwise deletion"})
            Me.cbFARotation.Items.AddRange(New Object() {"None", "Varimax", "Quartimax", "Equamax", "Promax"})
            Me.cbFAScoreMethod.Items.AddRange(New Object() {"None", "Regression", "Bartlett"})
            Me.cbFAExtraction.SelectedIndex = 1                ' Principal Axis
            Me.cbFACommunalityInit.SelectedIndex = 0           ' SMC
            Me.cbFAMissingPolicy.SelectedIndex = 0             ' Error on missing
            Me.cbFARotation.SelectedIndex = 1                  ' Varimax
            Me.cbFAScoreMethod.SelectedIndex = 1               ' Regression
            Me.tbFAEps.Text = FormatUiDouble(0.000001)
            AddHandler Me.cbFAExtraction.SelectedIndexChanged, AddressOf Me.FactorAnalysisExtractionChanged
            AddHandler Me.cbFARotation.SelectedIndexChanged, AddressOf Me.FactorAnalysisRotationChanged
            AddHandler Me.optFAExtractFixed.CheckedChanged, AddressOf Me.FactorAnalysisRetentionChanged
            AddHandler Me.optFAExtractEigen.CheckedChanged, AddressOf Me.FactorAnalysisRetentionChanged
            AddHandler Me.optFAExtractVariance.CheckedChanged, AddressOf Me.FactorAnalysisRetentionChanged
            Me.FactorAnalysisExtractionChanged(Me.cbFAExtraction, System.EventArgs.Empty)
            Me.FactorAnalysisRotationChanged(Me.cbFARotation, System.EventArgs.Empty)
            Me.FactorAnalysisRetentionChanged(Me.optFAExtractFixed, System.EventArgs.Empty)

        ElseIf Me.Tag = HelpTopic.DiscriminantAnalysis Then
            Me.TabPageOptionsDA.Parent = Me.TabControl1
            Me.lblGruppingVar.Visible = True
            Me.cbGruppingVar.Visible = True
            Me.lblRowLabel.Visible = True
            Me.cbRowLabel.Visible = True
            Me.cbDAMethod.Items.AddRange(New Object() {"Linear discriminant analysis", "Quadratic discriminant analysis"})
            Me.cbDAStandardization.Items.AddRange(New Object() {"None", "Z-scores", "Range 0 to 1"})
            Me.cbDAMissingPolicy.Items.AddRange(New Object() {"Error on missing", "Listwise deletion"})
            Me.cbDAPriors.Items.AddRange(New Object() {"Proportional to group sizes", "Equal", "User-specified"})
            Me.cbDAValidation.Items.AddRange(New Object() {"None", "Leave-one-out", "K-fold", "Holdout"})
            Me.cbDAMethod.SelectedIndex = 0
            Me.cbDAStandardization.SelectedIndex = 0
            Me.cbDAMissingPolicy.SelectedIndex = 0
            Me.cbDAPriors.SelectedIndex = 0
            Me.cbDAValidation.SelectedIndex = 0
            Me.tbDARegularization.Text = FormatUiDouble(0.00000001)
            Me.tbDASeed.Text = AppGlobals.GetDefaultRandomSeedText()
            Me.tbDAHoldoutFraction.Value = FormatUiDouble(0.3)
            Me.tbDAUserPriors.Enabled = False
            AddHandler Me.cbDAPriors.SelectedIndexChanged, AddressOf Me.DiscriminantPriorModeChanged
            AddHandler Me.cbDAValidation.SelectedIndexChanged, AddressOf Me.DiscriminantValidationChanged
            Me.UpdateDiscriminantOptionStates()
            Me.ckFirstRow.Visible = True

        End If

        Me.WireHelp(Me.btnHelp)
        Me.Populate()
    End Sub

    Private Sub btCalculate_Click(sender As Object, e As System.EventArgs) Handles btCalculate.Click
        Try
            Dim MyData As DataObj
            'activate workbook we are working on (different may  be open if we re-running the analysis)
            Me.pWorkbook.Activate()

            If Me.Tag = HelpTopic.DiscriminantAnalysis Then
                MyData = GetDiscriminantData()
            Else
                MyData = GetData()
            End If

            If MyData.bZeroValid Then 'check for zero valid data
                MsgBox("No valid observations")
                Exit Sub
            End If

            If Me.Tag = HelpTopic.ScatterPlotMatrix Then
                Me.RunSPM(MyData)
            ElseIf Me.Tag = HelpTopic.PrincipalComponentAnalysis Then
                Me.RunPCA(MyData)
            ElseIf Me.Tag = HelpTopic.MultipleCorrespondenceAnalysis Then
                Me.RunMCA(MyData)
            ElseIf Me.Tag = HelpTopic.KMeansClustering Then
                Me.RunKmeans(MyData)
            ElseIf Me.Tag = HelpTopic.HierarchicalClustering Then
                Me.RunHierarchicalClustering(MyData)
            ElseIf Me.Tag = HelpTopic.FactorAnalysis Then
                Me.RunFA(MyData)
            ElseIf Me.Tag = HelpTopic.DiscriminantAnalysis Then
                Me.RunDiscriminantAnalysis(MyData)
            End If
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunDiscriminantAnalysis(MyData As DataObj)
        Dim predictorData(,) As Double = Me.ExtractDiscriminantPredictorData(MyData)
        Dim predictorNames() As String = Me.ExtractDiscriminantPredictorNames(MyData)
        Dim groupLabels() As Object = Matrix.GetColumnFrom2Darray(MyData.FinalData, 0)
        Dim rowLabels() As String = Me.GetSelectedDiscriminantRowLabels(MyData)

        Dim intent = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.Discriminant, MyData, rowLabels, selectedKeys:=(New String() {Me.GetSelectedDiscriminantGroupingKey()}).Concat(Me.lbXs.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToArray()).ToArray())
        Dim fitDA As New Multivariate.DiscriminantAnalysis
        fitDA.dataInputs(predictorData, groupLabels, rowLabels, predictorNames)

        Dim priorMode As Multivariate.DiscriminantPriorMode = Me.GetSelectedDiscriminantPriorMode()
        Dim options As New MultivariateDiscriminantOptions With {
            .Method = Me.GetSelectedDiscriminantMethod(), .Standardization = Me.GetSelectedDiscriminantStandardization(),
            .Missing = Me.GetSelectedDiscriminantMissingPolicy(), .Priors = priorMode,
            .Regularization = Me.GetSelectedDiscriminantRegularization()}
        options.ConfigureModel(fitDA)

        If priorMode = Multivariate.DiscriminantPriorMode.UserSpecified Then
            Dim priorLabels() As Object = Nothing
            Dim priorValues() As Double = Nothing
            Me.GetDiscriminantUserPriors(priorLabels, priorValues)
            fitDA.priorInputs(priorLabels, priorValues)
        End If

        options.Validation = Me.GetSelectedDiscriminantValidationMode()
        options.Folds = CInt(Me.nudDAFolds.Value)
        options.HoldoutFraction = CDbl(Me.tbDAHoldoutFraction.Value)
        options.Seed = Me.GetSelectedDiscriminantRandomSeed()
        options.Stratified = Me.ckDAStratified.Checked
        options.ConfigureValidation(fitDA)
        fitDA.Fit()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter With {.wb = wb, .ws = wsData}
        wrData.write(Me.BuildDiscriminantInputDataTable(MyData, groupLabels, rowLabels))

        Dim wsResults As Worksheet = CType(wb.Worksheets.Add(After:=wsData), Worksheet)
        wsResults.Name = "Discriminant results"
        Dim wrSummary As New ExcelDnaResultWriter With {.wb = wb, .ws = wsResults}
        Dim report = MultivariateReportPolicy.Project(intent, fitDA.wrapResults())
        Dim rr = New ProcessListofResultTables(report.LegacyTables())
        rr.writeToSheet(wrSummary, True)
    End Sub

    Private Sub RunFA(MyData As DataObj)
        Dim intent = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.Factor, MyData, selectedKeys:=Me.lbXs.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToArray())
        Dim fitFA As New Multivariate.FactorAnalysis
        fitFA.dataInputs(MyData.DataDbl, MyData.RowIds, MyData.varNames)
        Dim options As New MultivariateFactorOptions With {
            .MaximumIterations = CInt(Me.nudFAMaxIter.Value), .Epsilon = ParseUiDouble(Me.tbFAEps.Text, "Convergence tolerance"),
            .MatrixType = Me.GetSelectedFAMatrixType(), .Extraction = Me.GetSelectedFAExtractionMethod(),
            .Retention = Me.GetSelectedFARetentionMethod(), .RetentionValue = Me.GetSelectedFARetentionValue(),
            .Rotation = Me.GetSelectedFARotationMethod(), .Scores = Me.GetSelectedFAScoreMethod(),
            .Communality = Me.GetSelectedFACommunalityInitialization(), .Missing = Me.GetSelectedFAMissingPolicy(),
            .KaiserNormalization = Me.ckFAKaiserNormalization.Checked, .PromaxPower = CDbl(Me.nudFAPromaxPower.Value)}
        options.Configure(fitFA)
        fitFA.Calculate()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter With {.wb = wb, .ws = wsData}
        wrData.write(Me.BuildKMeansInputDataTable(MyData, Nothing))

        Dim wsResults As Worksheet = CType(wb.Worksheets.Add(After:=wsData), Worksheet)
        wsResults.Name = "Factor analysis results"
        Dim wrSummary As New ExcelDnaResultWriter With {.wb = wb, .ws = wsResults}
        Dim report = MultivariateReportPolicy.Project(intent, fitFA.wrapResults())
        Dim rr = New ProcessListofResultTables(report.LegacyTables())
        rr.writeToSheet(wrSummary, True)

        WindowsMultivariateWorkflowAdapter.RenderFactor(report, fitFA)
    End Sub

    Private Sub RunHierarchicalClustering(MyData As DataObj)
        Dim rowLabels() As String = Me.GetSelectedKMeansRowLabels(MyData)

        Dim intent = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.Hierarchical, MyData, rowLabels, selectedKeys:=Me.lbXs.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToArray())
        Dim fitHierarchical As New Multivariate.HierarchicalClustering
        fitHierarchical.dataInputs(MyData.DataDbl, rowLabels, MyData.varNames)
        Dim options As New MultivariateHierarchicalOptions With {
            .Linkage = Me.GetSelectedHierarchicalLinkage(), .Distance = Me.GetSelectedHierarchicalDistanceMetric(),
            .MinkowskiPower = ParseUiDouble(Me.tbHierarchicalMinkowskiPower.Text, "Minkowski power"),
            .Standardization = Me.GetSelectedHierarchicalStandardization(), .Missing = Me.GetSelectedHierarchicalMissingPolicy()}
        options.ConfigureModel(fitHierarchical)
        options.Cut = Me.GetSelectedHierarchicalMembershipMode()
        options.Clusters = CInt(Me.nudHierarchicalClusters.Value)
        options.CutHeight = ParseUiDouble(Me.tbHierarchicalCutHeight.Text, "Cut height")
        options.ConfigureReport(fitHierarchical)
        fitHierarchical.Fit()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter()
        wrData.wb = wb
        wrData.ws = wsData
        wrData.write(Me.BuildKMeansInputDataTable(MyData, rowLabels))

        Dim report = MultivariateReportPolicy.Project(intent, fitHierarchical.wrapResults())
        Dim res = report.LegacyTables()
        Dim wrSummary As New ExcelDnaResultWriter()
        Dim wsSummary As Worksheet = CType(wb.Worksheets.Add(After:=wsData), Worksheet)
        wsSummary.Name = "Hierarchical results"
        wrSummary.wb = wb
        wrSummary.ws = wsSummary
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(wrSummary, True)

        Dim plotPlan = MultivariateReportPolicy.PlotPlan(intent.Kind,
            Me.grpHierarchicalDendrogram IsNot Nothing AndAlso Me.ckHierarchicalCreateDendrogram.Checked)
        If plotPlan.Count > 0 Then
            Dim wsDendrogram As Worksheet = CType(wb.Worksheets.Add(After:=wsSummary), Worksheet)
            wsDendrogram.Name = "Dendrogram"
            graphics.ClusterAnalysisPlotExcel.CreateDendrogramChart(fitHierarchical.Result, ws:=wsDendrogram,
                                             topLeftCellAddress:="A1",
                                             chartWidth:=680.0,
                                             chartHeight:=420.0,
                                             heightMode:=Me.GetSelectedHierarchicalDendrogramHeightMode(),
                                             orientation:=Me.GetSelectedHierarchicalDendrogramOrientation(),
                                             labelMode:=Me.GetSelectedHierarchicalDendrogramLabelMode(),
                                             chartTitle:="Hierarchical Clustering Dendrogram",
                                             cutMode:=Me.GetSelectedHierarchicalMembershipMode(),
                                             membershipClusterCount:=CInt(Me.nudHierarchicalClusters.Value),
                                             membershipCutHeight:=ParseUiDouble(Me.tbHierarchicalCutHeight.Text, "Cut height"))
        End If
    End Sub

    Private Sub RunKmeans(MyData As DataObj)
        Dim rowLabels() As String = Me.GetSelectedKMeansRowLabels(MyData)

        Dim intent = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.KMeans, MyData, rowLabels, selectedKeys:=Me.lbXs.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToArray())
        Dim fitKmeans As New Multivariate.KMeans
        fitKmeans.dataInputs(MyData.DataDbl, rowLabels, MyData.varNames)

        Dim initialization As Multivariate.KMeansInitializationMethod = Me.GetSelectedKMeansInitialization()
        If initialization = Multivariate.KMeansInitializationMethod.UserSpecifiedCenters Then
            fitKmeans.startingCentersInputs(Me.ImportKMeansStartingCenters(expectedClusters:=CInt(Me.nudKmeansClusters.Value),
                                                                   expectedVariables:=MyData.nCols))
        End If

        Dim seed As Integer = MultivariateWorkflowPolicy.Seed(Nothing)
        If Me.tbKmeansSeed IsNot Nothing AndAlso Me.tbKmeansSeed.Text.Trim() <> String.Empty Then
            seed = ParseUiInteger(Me.tbKmeansSeed.Text, "Random seed")
        End If

        Dim options As New MultivariateKMeansOptions With {
            .Clusters = CInt(Me.nudKmeansClusters.Value), .Initialization = initialization,
            .Distance = Me.GetSelectedKMeansDistanceMetric(), .Starts = CInt(Me.nudKmeansStarts.Value),
            .MaximumIterations = CInt(Me.nudKmeansMaxIterations.Value),
            .Tolerance = ParseUiDouble(Me.tbKmeansTolerance.Text, "Convergence tolerance"),
            .Standardization = Me.GetSelectedKMeansStandardization(), .Missing = Me.GetSelectedKMeansMissingPolicy(),
            .EmptyCluster = Me.GetSelectedKMeansEmptyClusterHandling(), .Seed = seed}
        options.Configure(fitKmeans)
        fitKmeans.Fit()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter()
        wrData.wb = wb
        wrData.ws = wsData
        wrData.write(Me.BuildKMeansInputDataTable(MyData, rowLabels))

        Dim report = MultivariateReportPolicy.Project(intent, fitKmeans.wrapResults())
        Dim res = report.LegacyTables()
        Dim wrSummary As New ExcelDnaResultWriter()
        Dim wsSummary As Worksheet = CType(wb.Worksheets.Add(After:=wsData), Worksheet)
        wsSummary.Name = "KMeans results"
        wrSummary.wb = wb
        wrSummary.ws = wsSummary
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(wrSummary, True)
    End Sub

    Private Sub RunMCA(MyData As DataObj)
        Dim mca As New Multivariate.CA

        If Me.ckFirstRow.Checked Then 'remove first data row
            Dim ids As New Dictionary(Of Integer, Integer)
            For i = 1 To MyData.nRows - 1
                ids.Add(i, MyData.RowIds(i))
            Next
            MyData.SubsetByRowIdValues(ids)
        End If

        Dim intent = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.MCA, MyData, selectedKeys:=Me.lbXs.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToArray())
        mca.DataMultiple(Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(MyData.FinalData), intent.VariableNames)
        mca.Calculate()

        'Dump results
        Dim WriteRes As ExcelDnaResultWriter = New ExcelDnaResultWriter
        WriteRes.wb = AppGlobals.app.Workbooks.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(3) 'second row is blank because of Design matrix having two header rows
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        WriteRes.write(MyData.varNames)
        WriteRes.setRowPointer(3) 'second row is blank
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        WriteRes.write({"Design MatrixType:"})
        WriteRes.setRowPointer()
        WriteRes.shiftColumnPointer(1)
        WriteRes.write(mca.BurtVarNames)
        WriteRes.write(mca.rowNames)
        WriteRes.write(mca.DesignMatrix)

        'MCA numerical results
        Dim report = MultivariateReportPolicy.Project(intent, mca.wrapResults())
        Dim res = report.LegacyTables()
        WriteRes = New ExcelDnaResultWriter
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "MCA results"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

        'Add figures
        WindowsMultivariateWorkflowAdapter.RenderMca(report, mca)
    End Sub

    Private Sub RunPCA(MyData As DataObj)
        Dim correlation As Boolean = Me.optCorr.Checked
        Dim rowLabels() As String = Me.GetSelectedPcaRowLabels(MyData)
        Dim groupLabels() As String = Me.GetSelectedPcaGroupLabels(MyData)
        Dim options As New MultivariatePcaOptions(correlation, Me.optExtractEigen.Checked, Me.optExtractFixed.Checked, Me.optExtractVariance.Checked,
            Me.spinBtnExtractEigen.Value, Me.spinBtnExtractComp.Value, Me.spinBtnExtractVariance.Value)
        Dim intent = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.PCA, MyData, rowLabels, groupLabels, selectedKeys:=Me.lbXs.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToArray())

        Dim objPCA As New Multivariate.PCA
        With objPCA
            .dataInputs(MyData.DataDbl, intent.SourceRowIds, intent.VariableNames, options.Label)
            options.Configure(objPCA, ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                            ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"))
            .Calculate(options.CoreRetention, options.Value)
        End With

        'Dump results
        Dim WriteRes As ExcelDnaResultWriter = New ExcelDnaResultWriter
        WriteRes.wb = AppGlobals.app.Workbooks.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Dim metadataColumnCount As Integer = 1
        If rowLabels IsNot Nothing Then
            WriteRes.write({"Record ID"})
            WriteRes.setRowPointer(2)
            WriteRes.write(rowLabels, bTall:=True)
        Else
            WriteRes.write({"Row ID"})
            WriteRes.setRowPointer(2)
            WriteRes.write(MyData.RowIds, bTall:=True)
        End If

        If groupLabels IsNot Nothing Then
            metadataColumnCount += 1
            WriteRes.setRowPointer()
            WriteRes.setColumnPointer(2)
            WriteRes.write({"Group ID"})
            WriteRes.setRowPointer(2)
            WriteRes.write(groupLabels, bTall:=True)
        End If
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(metadataColumnCount + 1)
        WriteRes.write(MyData.varNames)
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        'Standardized values
        WriteRes.write(objPCA.StandardizedVarNames(MyData.varNames))
        WriteRes.write(objPCA.StandardizedData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        'Final Reduced Dataset
        WriteRes.write(objPCA.StandardizedVarNames(objPCA.PCnames("Reduced_Data_PC")))
        WriteRes.write(objPCA.ReducedDataset)

        'PCA numerical results
        Dim report = MultivariateReportPolicy.Project(intent, objPCA.wrapResults())
        Dim res = report.LegacyTables()
        WriteRes = New ExcelDnaResultWriter
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "PCA results"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

        'Figures
        WindowsMultivariateWorkflowAdapter.RenderPca(report, objPCA)

    End Sub

    Private Sub RunSPM(MyData As DataObj)
        Dim spm = New graphics.ScatterPlotMatrix(MyData.DataDbl, MyData.varNames, pWorkbook)
        spm.settingInputs(Me.ckDisplayCorrelCoef.Checked, Me.ckShowRegressionLines.Checked)
        spm.compute()
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        If Me.Tag = HelpTopic.DiscriminantAnalysis Then
            Dim groupingKey As String = Me.GetSelectedDiscriminantGroupingKey()
            If groupingKey <> String.Empty Then
                For Each selectedItem As Object In Me.lbAllColumns.SelectedItems
                    If CStr(selectedItem) = groupingKey Then
                        MsgBox("The grouping variable cannot also be added to the analysis-variable list.", vbExclamation, "Input Error!")
                        Exit Sub
                    End If
                Next
            End If
        End If

        AddItemsToListbox(Me.lbXs, Me.lbAllColumns)
    End Sub

    Private Sub btRemoveX_Click(sender As Object, e As System.EventArgs) Handles btRemoveX.Click
        Remove_Item(Me.lbXs, "selected")
    End Sub

    Private Sub btReload_Click(sender As Object, e As System.EventArgs) Handles btReload.Click
        Dim newSheet As Object
        Me.lbAllColumns.Items.Clear()

        If Me.cbSheetsList.SelectedIndex <> -1 Then
            If pWorksheet.Name <> Me.cbSheetsList.SelectedItem.ToString() Then 'new sheet selected clear all listboxes
                Me.lbXs.Items.Clear()
            End If
            newSheet = pWorkbook.Worksheets(Me.cbSheetsList.SelectedItem.ToString())
            Me.Populate(newSheet)
        Else
            Me.Populate(pWorksheet)
        End If
    End Sub

    Private Sub cbGruppingVar_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbGruppingVar.SelectedIndexChanged
        If Me.Tag = HelpTopic.PrincipalComponentAnalysis Then
            Me.RemovePcaAuxiliaryVariableFromAnalysisList(Me.GetSelectedPcaGroupingKey(), "grouping variable")
            Exit Sub
        End If

        If Me.Tag <> HelpTopic.DiscriminantAnalysis Then Exit Sub

        Dim groupingKey As String = Me.GetSelectedDiscriminantGroupingKey()
        If groupingKey = String.Empty Then Exit Sub

        Dim removed As Boolean = False
        For i As Integer = Me.lbXs.Items.Count - 1 To 0 Step -1
            If CStr(Me.lbXs.Items(i)) = groupingKey Then
                Me.lbXs.Items.RemoveAt(i)
                removed = True
            End If
        Next

        If removed Then
            MsgBox("The selected grouping variable was removed from the analysis-variable list because it cannot be used as both a predictor and a grouping variable.", vbInformation, "Discriminant Analysis")
        End If
    End Sub

    Private Sub cbRowLabel_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbRowLabel.SelectedIndexChanged
        If Me.Tag <> HelpTopic.PrincipalComponentAnalysis Then Exit Sub
        Me.RemovePcaAuxiliaryVariableFromAnalysisList(Me.GetSelectedPcaRowLabelKey(), "row label variable")
    End Sub

    Private Sub DiscriminantPriorModeChanged(sender As Object, e As System.EventArgs)
        Me.UpdateDiscriminantOptionStates()
    End Sub

    Private Sub DiscriminantValidationChanged(sender As Object, e As System.EventArgs)
        Me.UpdateDiscriminantOptionStates()
    End Sub

    Private Function GetData() As DataObj
        Dim MyData As DataObj = New DataObj
        Dim keys As New List(Of String)
        For i = 0 To Me.lbXs.Items.Count - 1 'X vars
            keys.Add(CStr(Me.lbXs.Items(i)))
        Next

        If keys.Count = 0 Then
            Throw New ArgumentException("Please select at least one analysis variable.")
        End If

        If Me.Tag = HelpTopic.PrincipalComponentAnalysis Then
            Dim rowLabelKey As String = Me.GetSelectedPcaRowLabelKey()
            Dim groupingKey As String = Me.GetSelectedPcaGroupingKey()

            MultivariateWorkflowPolicy.ValidateAuxiliaryRoles(keys, rowLabelKey, groupingKey, True)
        End If

        If Me.Tag = HelpTopic.KMeansClustering OrElse Me.Tag = HelpTopic.HierarchicalClustering OrElse Me.Tag = HelpTopic.FactorAnalysis Then
            Dim rowLabelKey As String = Me.GetSelectedKMeansRowLabelKey()
            MultivariateWorkflowPolicy.ValidateAuxiliaryRoles(keys, rowLabelKey, String.Empty, False)

            'Keep missing values in the imported matrix so the clustering engine can apply
            'its own missing-value policy (error vs. listwise deletion).
            MyData.bAllowMissing = True
        End If

        Dim ref As String = BuildExcelRefList(pWorksheet, keys, Me.VariableColumnsInfo)
        If String.IsNullOrWhiteSpace(ref) Then
            Throw New ArgumentException("No valid worksheet reference could be constructed from the selected analysis variables.")
        End If

        If Me.Tag = HelpTopic.MultipleCorrespondenceAnalysis Then
            ExcelDnaDataImporter.ImportInto(MyData, ref, False, 20000)
        Else
            ExcelDnaDataImporter.ImportInto(MyData, ref)
        End If

        Return MyData
    End Function

    Private Function GetDiscriminantData() As DataObj
        If Me.AllColumnsInfo Is Nothing Then
            Throw New InvalidOperationException("The worksheet columns have not been populated.")
        End If

        Dim groupingKey As String = Me.GetSelectedDiscriminantGroupingKey()
        If groupingKey = String.Empty Then
            Throw New ArgumentException("Please select a grouping variable.")
        End If

        Dim predictorKeys As New List(Of String)
        For i As Integer = 0 To Me.lbXs.Items.Count - 1
            predictorKeys.Add(CStr(Me.lbXs.Items(i)))
        Next

        If predictorKeys.Count = 0 Then
            Throw New ArgumentException("Please select at least one analysis variable.")
        End If

        MultivariateWorkflowPolicy.ValidateAuxiliaryRoles(predictorKeys, String.Empty, groupingKey, False)

        Dim importKeys As New List(Of String)
        importKeys.Add(groupingKey)
        For Each key As String In predictorKeys
            importKeys.Add(key)
        Next

        Dim ref As String = BuildExcelRefList(pWorksheet, importKeys, Me.AllColumnsInfo)
        If String.IsNullOrWhiteSpace(ref) Then
            Throw New ArgumentException("No valid worksheet reference could be constructed from the selected discriminant-analysis variables.")
        End If

        Dim MyData As New DataObj
        MyData.bAllowMissing = True
        Dim skipRows As Integer = If(Me.ckFirstRow.Checked, 1, 0)
        ExcelDnaDataImporter.ImportInto(MyData, ref, False, 0, skipRows)

        If MyData.varNames Is Nothing OrElse MyData.varNames.Length <> importKeys.Count Then
            Throw New ArgumentException("One or more selected variables contain no usable data after import. Please review the grouping variable and predictors.")
        End If

        If MyData.nCols < 2 Then
            Throw New ArgumentException("Discriminant analysis requires one grouping variable and at least one predictor.")
        End If

        Return MyData
    End Function

    Sub Populate(Optional ws As Object = Nothing)
        Dim VarRng As Object, ws_temp As Object
        If ws IsNot Nothing Then
            pWorksheet = ws
            pWorkbook = ws.parent
        End If

        Dim FinalCol = LastColumnInSheet(pWorksheet)
        Dim MaxRows = MaxRowsInSheet(pWorksheet)
        VarRng = pWorksheet.Range(pWorksheet.Cells(1, 1), pWorksheet.Cells(1, FinalCol)) 'Create range object to contain variable names

        If Me.Tag = HelpTopic.MultipleCorrespondenceAnalysis Then
            'we accept character columns
            Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns, False)
        Else
            Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns)
        End If

        Me.AllColumnsInfo = Nothing
        If Me.Tag = HelpTopic.PrincipalComponentAnalysis OrElse
            Me.Tag = HelpTopic.KMeansClustering OrElse
            Me.Tag = HelpTopic.HierarchicalClustering OrElse
            Me.Tag = HelpTopic.DiscriminantAnalysis Then

            Dim sink As New System.Windows.Forms.ListBox()
            Me.AllColumnsInfo = VarNamesToLBox(VarRng, MaxRows, sink, False)

            If Me.Tag = HelpTopic.PrincipalComponentAnalysis Then Me.PopulatePcaAuxiliaryItems()
            If Me.Tag = HelpTopic.KMeansClustering OrElse Me.Tag = HelpTopic.HierarchicalClustering Then Me.PopulateKMeansRowLabelItems()

            If Me.Tag = HelpTopic.DiscriminantAnalysis Then
                Me.PopulateDiscriminantGroupingItems()
                Me.PopulateOptionalDiscriminantRowLabelItems()
            End If
        End If

        Me.cbSheetsList.Items.Clear()
        For Each ws_temp In pWorkbook.Worksheets
            Me.cbSheetsList.Items.Add(ws_temp.name)
        Next
        Me.cbSheetsList.SelectedIndex = Me.cbSheetsList.FindStringExact(Me.pWorkbook.ActiveSheet.name)
    End Sub

    '--------------------------------------------------------------------------
    ' PCA score-plot row-label/grouping helpers
    '--------------------------------------------------------------------------
    Private Sub PopulatePcaAuxiliaryItems()
        Me.PopulatePcaAuxiliaryCombo(Me.cbGruppingVar)
        Me.PopulatePcaAuxiliaryCombo(Me.cbRowLabel)
    End Sub

    Private Sub PopulatePcaAuxiliaryCombo(combo As System.Windows.Forms.ComboBox)
        If combo Is Nothing Then Exit Sub

        Dim previous As String = String.Empty
        If combo.SelectedItem IsNot Nothing Then previous = CStr(combo.SelectedItem)

        combo.Items.Clear()
        combo.Items.Add("(none)")

        If Me.AllColumnsInfo IsNot Nothing Then
            Dim infos As New List(Of VarColumnInfo)
            For Each kvp In Me.AllColumnsInfo
                infos.Add(kvp.Value)
            Next
            infos.Sort(Function(a As VarColumnInfo, b As VarColumnInfo) a.ColumnNumber.CompareTo(b.ColumnNumber))

            For Each info As VarColumnInfo In infos
                combo.Items.Add(info.DisplayText)
            Next
        End If

        If previous <> String.Empty AndAlso combo.Items.Contains(previous) Then
            combo.SelectedItem = previous
        ElseIf combo.Items.Count > 0 Then
            combo.SelectedIndex = 0
        End If
    End Sub

    Private Function GetSelectedPcaRowLabelKey() As String
        Return Me.GetSelectedPcaAuxiliaryKey(Me.cbRowLabel)
    End Function

    Private Function GetSelectedPcaGroupingKey() As String
        Return Me.GetSelectedPcaAuxiliaryKey(Me.cbGruppingVar)
    End Function

    Private Function GetSelectedPcaAuxiliaryKey(combo As System.Windows.Forms.ComboBox) As String
        If combo Is Nothing OrElse combo.SelectedIndex <= 0 OrElse combo.SelectedItem Is Nothing Then Return String.Empty
        Return CStr(combo.SelectedItem)
    End Function

    Private Function GetSelectedPcaRowLabels(MyData As DataObj) As String()
        Dim key As String = Me.GetSelectedPcaRowLabelKey()
        If key = String.Empty Then Return Nothing
        Return Me.ReadPcaAuxiliaryColumn(MyData, key, useRowIdForBlank:=True)
    End Function

    Private Function GetSelectedPcaGroupLabels(MyData As DataObj) As String()
        Dim key As String = Me.GetSelectedPcaGroupingKey()
        If key = String.Empty Then Return Nothing
        Return Me.ReadPcaAuxiliaryColumn(MyData, key, useRowIdForBlank:=False)
    End Function

    Private Function ReadPcaAuxiliaryColumn(MyData As DataObj,
                                            key As String,
                                            useRowIdForBlank As Boolean) As String()
        If MyData Is Nothing Then Throw New ArgumentNullException(NameOf(MyData))
        If Me.AllColumnsInfo Is Nothing OrElse Not Me.AllColumnsInfo.ContainsKey(key) Then
            Throw New ArgumentException("The selected PCA auxiliary variable could not be found on the active worksheet.")
        End If

        Dim info As VarColumnInfo = Me.AllColumnsInfo(key)
        Dim out(MyData.nRows - 1) As String

        For i As Integer = 0 To MyData.nRows - 1
            Dim raw As Object = Me.pWorksheet.Cells(MyData.RowIds(i), info.ColumnNumber).Value
            Dim text As String = String.Empty
            Try
                If raw IsNot Nothing Then text = CStr(raw).Trim()
            Catch
            End Try

            text = MultivariateWorkflowPolicy.PcaAuxiliaryLabel(text, MyData.RowIds(i), useRowIdForBlank)
            out(i) = text
        Next

        Return out
    End Function

    Private Sub RemovePcaAuxiliaryVariableFromAnalysisList(key As String, roleName As String)
        If key = String.Empty Then Exit Sub

        Dim removed As Boolean = False
        For i As Integer = Me.lbXs.Items.Count - 1 To 0 Step -1
            If CStr(Me.lbXs.Items(i)) = key Then
                Me.lbXs.Items.RemoveAt(i)
                removed = True
            End If
        Next

        If removed Then
            MsgBox("The selected " & roleName & " was removed from the analysis-variable list because PCA auxiliary variables are used only to identify/style observations and are not included in the PCA calculation.",
                   vbInformation,
                   "Principal Component Analysis")
        End If
    End Sub

    Private Sub PopulateKMeansRowLabelItems()
        If Me.cbRowLabel Is Nothing Then Exit Sub

        Dim previous As String = String.Empty
        If Me.cbRowLabel.SelectedItem IsNot Nothing Then previous = CStr(Me.cbRowLabel.SelectedItem)

        Me.cbRowLabel.Items.Clear()
        Me.cbRowLabel.Items.Add("(none)")

        If Me.AllColumnsInfo IsNot Nothing Then
            Dim infos As New List(Of VarColumnInfo)
            For Each kvp In Me.AllColumnsInfo
                infos.Add(kvp.Value)
            Next
            infos.Sort(Function(a As VarColumnInfo, b As VarColumnInfo) a.ColumnNumber.CompareTo(b.ColumnNumber))

            For Each info As VarColumnInfo In infos
                Me.cbRowLabel.Items.Add(info.DisplayText)
            Next
        End If

        If previous <> String.Empty AndAlso Me.cbRowLabel.Items.Contains(previous) Then
            Me.cbRowLabel.SelectedItem = previous
        Else
            Me.cbRowLabel.SelectedIndex = 0
        End If
    End Sub

    Private Function GetSelectedKMeansRowLabelKey() As String
        If Me.cbRowLabel Is Nothing OrElse Me.cbRowLabel.SelectedIndex <= 0 OrElse Me.cbRowLabel.SelectedItem Is Nothing Then
            Return String.Empty
        End If
        Return CStr(Me.cbRowLabel.SelectedItem)
    End Function

    Private Function GetSelectedKMeansRowLabels(MyData As DataObj) As String()
        Dim key As String = Me.GetSelectedKMeansRowLabelKey()
        If key = String.Empty Then Return Nothing
        If Me.AllColumnsInfo Is Nothing OrElse Not Me.AllColumnsInfo.ContainsKey(key) Then Return Nothing

        Dim info As VarColumnInfo = Me.AllColumnsInfo(key)
        Dim out(MyData.nRows - 1) As String

        For i As Integer = 0 To MyData.nRows - 1
            Dim raw As Object = Me.pWorksheet.Cells(MyData.RowIds(i), info.ColumnNumber).Value
            Dim labelText As String = String.Empty
            Try
                If raw IsNot Nothing Then labelText = CStr(raw)
            Catch
            End Try

            labelText = MultivariateWorkflowPolicy.ObservationLabel(labelText, MyData.RowIds(i))
            out(i) = labelText
        Next

        Return out
    End Function

    Private Sub KMeansInitializationChanged(sender As Object, e As System.EventArgs)
        Dim useUserCenters As Boolean = (Me.GetSelectedKMeansInitialization() = Multivariate.KMeansInitializationMethod.UserSpecifiedCenters)
        If Me.refKmeansCenters IsNot Nothing Then Me.refKmeansCenters.Enabled = useUserCenters
        If Me.nudKmeansStarts IsNot Nothing Then Me.nudKmeansStarts.Enabled = Not useUserCenters
    End Sub

    Private Function GetSelectedKMeansInitialization() As Multivariate.KMeansInitializationMethod
        Return MultivariateLegacyChoices.GetSelectedKMeansInitialization(Me.cbKmeansInitialization.SelectedIndex)
    End Function

    Private Function GetSelectedKMeansDistanceMetric() As Multivariate.KMeansDistanceMetric
        Return MultivariateLegacyChoices.GetSelectedKMeansDistanceMetric(Me.cbKmeansDistance.SelectedIndex)
    End Function

    Private Function GetSelectedKMeansStandardization() As Multivariate.ClusterStandardizationMode
        Return MultivariateLegacyChoices.GetSelectedKMeansStandardization(Me.cbKmeansStandardization.SelectedIndex)
    End Function

    Private Function GetSelectedKMeansMissingPolicy() As Multivariate.ClusterMissingValuePolicy
        Return MultivariateLegacyChoices.GetSelectedKMeansMissingPolicy(Me.cbKmeansMissingPolicy.SelectedIndex)
    End Function

    Private Function GetSelectedKMeansEmptyClusterHandling() As Multivariate.EmptyClusterHandlingStrategy
        Return MultivariateLegacyChoices.GetSelectedKMeansEmptyClusterHandling(Me.cbKmeansEmptyCluster.SelectedIndex)
    End Function

    Private Function BuildKMeansInputDataTable(MyData As DataObj, rowLabels() As String) As Object(,)
        Return MultivariateWorkflowPolicy.ClusterInputTable(MyData.FinalData, MyData.varNames, MyData.RowIds, rowLabels)
    End Function

    Private Function ImportKMeansStartingCenters(expectedClusters As Integer,
                                             expectedVariables As Integer) As Double(,)

        Dim refText As String = String.Empty
        If Me.refKmeansCenters IsNot Nothing AndAlso Me.refKmeansCenters.Address IsNot Nothing Then
            refText = Me.refKmeansCenters.Address.Trim()
        End If

        If refText = String.Empty Then
            Throw New ArgumentException("Please provide a worksheet range for the user-specified starting centers.")
        End If

        'Make sure the source workbook is active because DataObj resolves worksheet references
        'through the active workbook.
        Me.pWorkbook.Activate()

        Dim centersData As New DataObj
        ExcelDnaDataImporter.ImportInto(centersData, refText, bStartRow:=True)

        If centersData.bZeroValid Then
            Throw New ArgumentException("The user-specified starting-centers range does not contain any valid numeric observations.")
        End If

        If centersData.nRows <> expectedClusters Then
            Throw New ArgumentException(
                $"The user-specified starting-centers range must contain exactly {expectedClusters} row(s), one for each requested cluster.")
        End If

        If centersData.nCols <> expectedVariables Then
            Throw New ArgumentException(
                $"The user-specified starting-centers range must contain exactly {expectedVariables} column(s), one for each selected analysis variable.")
        End If

        Return centersData.DataDbl
    End Function

    Private Sub HierarchicalDistanceChanged(sender As Object, e As System.EventArgs)
        Dim useMinkowski As Boolean = (Me.GetSelectedHierarchicalDistanceMetric() = Multivariate.HierarchicalDistanceMetric.Minkowski)
        If Me.lblHierarchicalMinkowskiPower IsNot Nothing Then Me.lblHierarchicalMinkowskiPower.Enabled = useMinkowski
        If Me.tbHierarchicalMinkowskiPower IsNot Nothing Then Me.tbHierarchicalMinkowskiPower.Enabled = useMinkowski
    End Sub

    Private Sub HierarchicalMembershipModeChanged(sender As Object, e As System.EventArgs)
        Dim byClusters As Boolean = (Me.optHierarchicalCutByClusters IsNot Nothing AndAlso Me.optHierarchicalCutByClusters.Checked)
        If Me.nudHierarchicalClusters IsNot Nothing Then Me.nudHierarchicalClusters.Enabled = byClusters
        If Me.tbHierarchicalCutHeight IsNot Nothing Then Me.tbHierarchicalCutHeight.Enabled = Not byClusters
    End Sub

    Private Function GetSelectedHierarchicalLinkage() As Multivariate.HierarchicalLinkageMethod
        Return MultivariateLegacyChoices.GetSelectedHierarchicalLinkage(Me.cbHierarchicalLinkage.SelectedIndex)
    End Function

    Private Function GetSelectedHierarchicalDistanceMetric() As Multivariate.HierarchicalDistanceMetric
        Return MultivariateLegacyChoices.GetSelectedHierarchicalDistanceMetric(Me.cbHierarchicalDistance.SelectedIndex)
    End Function

    Private Function GetSelectedHierarchicalStandardization() As Multivariate.ClusterStandardizationMode
        Return MultivariateLegacyChoices.GetSelectedHierarchicalStandardization(Me.cbHierarchicalStandardization.SelectedIndex)
    End Function

    Private Function GetSelectedHierarchicalMissingPolicy() As Multivariate.ClusterMissingValuePolicy
        Return MultivariateLegacyChoices.GetSelectedHierarchicalMissingPolicy(Me.cbHierarchicalMissingPolicy.SelectedIndex)
    End Function

    Private Function GetSelectedHierarchicalMembershipMode() As Multivariate.HierarchicalMembershipDisplayMode
        Return MultivariateLegacyChoices.GetSelectedHierarchicalMembershipMode(Me.optHierarchicalCutByHeight IsNot Nothing AndAlso Me.optHierarchicalCutByHeight.Checked)
    End Function

    Private Function GetSelectedHierarchicalDendrogramHeightMode() As Multivariate.DendrogramHeightMode
        Return MultivariateLegacyChoices.GetSelectedHierarchicalDendrogramHeightMode(Me.cbHierarchicalHeightMode.SelectedIndex)
    End Function

    Private Function GetSelectedHierarchicalDendrogramOrientation() As Multivariate.DendrogramOrientation
        Return MultivariateLegacyChoices.GetSelectedHierarchicalDendrogramOrientation(Me.cbHierarchicalOrientation.SelectedIndex)
    End Function

    Private Function GetSelectedHierarchicalDendrogramLabelMode() As Multivariate.DendrogramLabelMode
        Return MultivariateLegacyChoices.GetSelectedHierarchicalDendrogramLabelMode(Me.cbHierarchicalLabelMode.SelectedIndex)
    End Function

    '--------------------------------------------------------------------------
    ' Factor Analysis
    '--------------------------------------------------------------------------
    Private Sub FactorAnalysisExtractionChanged(sender As Object, e As System.EventArgs)
        Dim method = Me.GetSelectedFAExtractionMethod()
        Dim needsCommunalityStart As Boolean = (method <> Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents)

        If Me.cbFACommunalityInit IsNot Nothing Then Me.cbFACommunalityInit.Enabled = needsCommunalityStart
    End Sub

    Private Sub FactorAnalysisRotationChanged(sender As Object, e As System.EventArgs)
        Dim rotation = Me.GetSelectedFARotationMethod()
        Dim hasRotation As Boolean = (rotation <> Multivariate.FactorAnalysisRotationMethod.None)
        Dim isPromax As Boolean = (rotation = Multivariate.FactorAnalysisRotationMethod.Promax)

        If Me.ckFAKaiserNormalization IsNot Nothing Then Me.ckFAKaiserNormalization.Enabled = hasRotation
        If Me.lblFAPromaxPower IsNot Nothing Then Me.lblFAPromaxPower.Enabled = isPromax
        If Me.nudFAPromaxPower IsNot Nothing Then Me.nudFAPromaxPower.Enabled = isPromax
    End Sub

    Private Sub FactorAnalysisRetentionChanged(sender As Object, e As System.EventArgs)
        If Me.nudFAFactors IsNot Nothing Then Me.nudFAFactors.Enabled = Me.optFAExtractFixed.Checked
        If Me.nudFAEigen IsNot Nothing Then Me.nudFAEigen.Enabled = Me.optFAExtractEigen.Checked
        If Me.nudFAVariance IsNot Nothing Then Me.nudFAVariance.Enabled = Me.optFAExtractVariance.Checked
    End Sub

    Private Function GetSelectedFAMatrixType() As Multivariate.FactorAnalysisMatrixType
        Return MultivariateLegacyChoices.GetSelectedFAMatrixType(Me.optFACovariance.Checked)
    End Function

    Private Function GetSelectedFAExtractionMethod() As Multivariate.FactorAnalysisExtractionMethod
        Return MultivariateLegacyChoices.GetSelectedFAExtractionMethod(Me.cbFAExtraction.SelectedIndex)
    End Function

    Private Function GetSelectedFARetentionMethod() As Multivariate.FactorAnalysisRetentionMethod
        Return MultivariateLegacyChoices.GetSelectedFARetentionMethod(Me.optFAExtractEigen.Checked, Me.optFAExtractVariance.Checked)
    End Function

    Private Function GetSelectedFARetentionValue() As Double
        Return MultivariateWorkflowPolicy.FactorRetentionValue(Me.optFAExtractEigen.Checked, Me.optFAExtractVariance.Checked,
            CDbl(Me.nudFAEigen.Value), CDbl(Me.nudFAVariance.Value), CDbl(Me.nudFAFactors.Value))
    End Function

    Private Function GetSelectedFARotationMethod() As Multivariate.FactorAnalysisRotationMethod
        Return MultivariateLegacyChoices.GetSelectedFARotationMethod(Me.cbFARotation.SelectedIndex)
    End Function

    Private Function GetSelectedFAScoreMethod() As Multivariate.FactorAnalysisScoreMethod
        Return MultivariateLegacyChoices.GetSelectedFAScoreMethod(Me.cbFAScoreMethod.SelectedIndex)
    End Function

    Private Function GetSelectedFACommunalityInitialization() As Multivariate.FactorAnalysisCommunalityInitialization
        Return MultivariateLegacyChoices.GetSelectedFACommunalityInitialization(Me.cbFACommunalityInit.SelectedIndex)
    End Function

    Private Function GetSelectedFAMissingPolicy() As Multivariate.FactorAnalysisMissingValuePolicy
        Return MultivariateLegacyChoices.GetSelectedFAMissingPolicy(Me.cbFAMissingPolicy.SelectedIndex)
    End Function

    '--------------------------------------------------------------------------
    ' Discriminant analysis helpers
    '--------------------------------------------------------------------------
    Private Sub PopulateDiscriminantGroupingItems()
        Dim previous As String = String.Empty
        If Me.cbGruppingVar.SelectedItem IsNot Nothing Then previous = CStr(Me.cbGruppingVar.SelectedItem)

        Me.cbGruppingVar.Items.Clear()
        Me.cbGruppingVar.Items.Add("(select grouping variable)")

        If Me.AllColumnsInfo IsNot Nothing Then
            Dim infos As New List(Of VarColumnInfo)
            For Each kvp In Me.AllColumnsInfo
                infos.Add(kvp.Value)
            Next
            infos.Sort(Function(a As VarColumnInfo, b As VarColumnInfo) a.ColumnNumber.CompareTo(b.ColumnNumber))

            For Each info As VarColumnInfo In infos
                Me.cbGruppingVar.Items.Add(info.DisplayText)
            Next
        End If

        If previous <> String.Empty AndAlso Me.cbGruppingVar.Items.Contains(previous) Then
            Me.cbGruppingVar.SelectedItem = previous
        ElseIf Me.cbGruppingVar.Items.Count > 0 Then
            Me.cbGruppingVar.SelectedIndex = 0
        End If
    End Sub

    Private Function GetSelectedDiscriminantGroupingKey() As String
        If Me.cbGruppingVar Is Nothing OrElse Me.cbGruppingVar.SelectedIndex <= 0 OrElse Me.cbGruppingVar.SelectedItem Is Nothing Then
            Return String.Empty
        End If
        Return CStr(Me.cbGruppingVar.SelectedItem)
    End Function

    Private Function ExtractDiscriminantPredictorData(MyData As DataObj) As Double(,)
        If MyData.nCols < 2 Then
            Throw New ArgumentException("Discriminant analysis requires at least one predictor variable.")
        End If

        Dim out(MyData.nRows - 1, MyData.nCols - 2) As Double
        For i As Integer = 0 To MyData.nRows - 1
            For j As Integer = 1 To MyData.nCols - 1
                If MyData.FinalData(i, j) Is Nothing Then
                    out(i, j - 1) = Double.NaN
                Else
                    out(i, j - 1) = CDbl(MyData.FinalData(i, j))
                End If
            Next
        Next
        Return out
    End Function

    Private Function ExtractDiscriminantPredictorNames(MyData As DataObj) As String()
        If MyData.nCols < 2 Then
            Throw New ArgumentException("Discriminant analysis requires at least one predictor variable.")
        End If

        Dim out(MyData.nCols - 2) As String
        For j As Integer = 1 To MyData.nCols - 1
            out(j - 1) = MyData.varNames(j)
        Next
        Return out
    End Function

    Private Function BuildDiscriminantInputDataTable(MyData As DataObj,
                                                 groupLabels() As Object,
                                                 rowLabels() As String) As Object(,)
        Return MultivariateWorkflowPolicy.DiscriminantInputTable(MyData.FinalData, MyData.varNames, groupLabels, rowLabels)
    End Function

    Private Function GetSelectedDiscriminantMethod() As Multivariate.DiscriminantAnalysisMethod
        Return MultivariateLegacyChoices.GetSelectedDiscriminantMethod(CStr(Me.cbDAMethod.SelectedItem))
    End Function

    Private Function GetSelectedDiscriminantMissingPolicy() As Multivariate.ClusterMissingValuePolicy
        Return MultivariateLegacyChoices.GetSelectedDiscriminantMissingPolicy(CStr(Me.cbDAMissingPolicy.SelectedItem))
    End Function

    Private Function GetSelectedDiscriminantRowLabels(MyData As DataObj) As String()
        Dim key As String = Me.GetSelectedDiscriminantRowLabelKey()
        If key = String.Empty Then Return Nothing
        If Me.AllColumnsInfo Is Nothing OrElse Not Me.AllColumnsInfo.ContainsKey(key) Then Return Nothing

        Dim info As VarColumnInfo = Me.AllColumnsInfo(key)
        Dim out(MyData.nRows - 1) As String

        For i As Integer = 0 To MyData.nRows - 1
            Dim raw As Object = Me.pWorksheet.Cells(MyData.RowIds(i), info.ColumnNumber).Value
            Dim labelText As String = String.Empty
            Try
                If raw IsNot Nothing Then labelText = CStr(raw)
            Catch
            End Try

            labelText = MultivariateWorkflowPolicy.ObservationLabel(labelText, MyData.RowIds(i))
            out(i) = labelText
        Next

        Return out
    End Function

    Private Sub GetDiscriminantUserPriors(ByRef categoryLabels() As Object, ByRef priorProbabilities() As Double)
        MultivariateLegacyChoices.ParsePriors(Me.tbDAUserPriors.Text, categoryLabels, priorProbabilities)
    End Sub

    Private Function GetSelectedDiscriminantStandardization() As Multivariate.ClusterStandardizationMode
        Return MultivariateLegacyChoices.GetSelectedDiscriminantStandardization(CStr(Me.cbDAStandardization.SelectedItem))
    End Function

    Private Sub PopulateOptionalDiscriminantRowLabelItems()
        Dim cb = Me.cbRowLabel
        If cb Is Nothing Then Exit Sub

        Dim previous As String = String.Empty
        If cb.SelectedItem IsNot Nothing Then previous = CStr(cb.SelectedItem)

        cb.Items.Clear()
        cb.Items.Add("(none)")

        If Me.AllColumnsInfo IsNot Nothing Then
            Dim infos As New List(Of VarColumnInfo)
            For Each kvp In Me.AllColumnsInfo
                infos.Add(kvp.Value)
            Next
            infos.Sort(Function(a As VarColumnInfo, b As VarColumnInfo) a.ColumnNumber.CompareTo(b.ColumnNumber))

            For Each info As VarColumnInfo In infos
                cb.Items.Add(info.DisplayText)
            Next
        End If

        If previous <> String.Empty AndAlso cb.Items.Contains(previous) Then
            cb.SelectedItem = previous
        Else
            cb.SelectedIndex = 0
        End If
    End Sub

    Private Function GetSelectedDiscriminantPriorMode() As Multivariate.DiscriminantPriorMode
        Return MultivariateLegacyChoices.GetSelectedDiscriminantPriorMode(If(Me.cbDAPriors Is Nothing OrElse Me.cbDAPriors.SelectedItem Is Nothing, String.Empty, CStr(Me.cbDAPriors.SelectedItem)))
    End Function

    Private Function GetSelectedDiscriminantValidationMode() As Multivariate.DiscriminantValidationMode
        Return MultivariateLegacyChoices.GetSelectedDiscriminantValidationMode(If(Me.cbDAValidation Is Nothing OrElse Me.cbDAValidation.SelectedItem Is Nothing, String.Empty, CStr(Me.cbDAValidation.SelectedItem)))
    End Function

    Private Function GetSelectedDiscriminantRandomSeed() As Integer
        Dim txt As String = Me.tbDASeed.Text
        If txt.Trim() = String.Empty Then Return Integer.MinValue
        Return ParseUiInteger(txt, "Random seed")
    End Function

    Private Function GetSelectedDiscriminantRegularization() As Double
        Dim txt As String = Me.tbDARegularization.Text
        If txt.Trim() = String.Empty Then Return 0.00000001
        Return ParseUiDouble(txt, "Covariance regularization")
    End Function

    Private Sub UpdateDiscriminantOptionStates()
        If Me.Tag <> HelpTopic.DiscriminantAnalysis Then Exit Sub

        Dim priorMode As Multivariate.DiscriminantPriorMode = Me.GetSelectedDiscriminantPriorMode()
        Dim validationMode As Multivariate.DiscriminantValidationMode = Me.GetSelectedDiscriminantValidationMode()

        Dim enableUserPriors As Boolean = (priorMode = Multivariate.DiscriminantPriorMode.UserSpecified)
        Me.tbDAUserPriors.Enabled = enableUserPriors
        Me.lblDAUserPriors.Enabled = enableUserPriors

        Dim enableFolds As Boolean = False
        Dim enableHoldout As Boolean = False
        Dim enableResamplingOptions As Boolean = False

        Select Case validationMode
            Case Multivariate.DiscriminantValidationMode.KFold
                enableFolds = True
                enableResamplingOptions = True

            Case Multivariate.DiscriminantValidationMode.Holdout
                enableHoldout = True
                enableResamplingOptions = True

            Case Else
                ' None / Leave-one-out: keep all subordinate validation controls disabled.
        End Select

        Me.nudDAFolds.Enabled = enableFolds
        Me.lblDAFolds.Enabled = enableFolds

        Me.tbDAHoldoutFraction.Enabled = enableHoldout
        Me.lblDAHoldoutFraction.Enabled = enableHoldout

        Me.ckDAStratified.Enabled = enableResamplingOptions
        Me.tbDASeed.Enabled = enableResamplingOptions
        Me.lblDASeed.Enabled = enableResamplingOptions
    End Sub

    Private Function GetSelectedDiscriminantRowLabelKey() As String
        If Me.cbRowLabel Is Nothing OrElse Me.cbRowLabel.SelectedIndex <= 0 OrElse Me.cbRowLabel.SelectedItem Is Nothing Then
            Return String.Empty
        End If
        Return CStr(Me.cbRowLabel.SelectedItem)
    End Function

End Class