Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui11PCA

    Private pWorksheet As Worksheet
    Private pWorkbook As Workbook
    Private VariableColumnsInfo As Dictionary(Of String, VarColumnInfo) 'information of variable/column names inported into the input listbox
    Private AllColumnsInfo As Dictionary(Of String, VarColumnInfo) 'all available columns, including non-numeric, for optional row labels

    Sub New(analysis As String, ws As Worksheet)
        ' This call is required by the designer.
        InitializeComponent()
        pWorksheet = ws
        pWorkbook = ws.Parent
        Me.tbEps.Text = FormatUiDouble(0.000001)

        ' Add any initialization after the InitializeComponent() call.
        Me.Text = analysis
        Me.TabPageOptionsPCA.Parent = Nothing
        Me.TabPageOptionsSPM.Parent = Nothing
        Me.TabPageOptionsKmeans.Parent = Nothing
        Me.TabPageOptionsHierarchicalClustering.Parent = Nothing
        Me.TabPageOptionsFA.Parent = Nothing
        Me.TabPageOptionsDA.Parent = Nothing

        If Me.Text = "Scatter Plot MatrixType" Then
            Me.TabPageOptionsSPM.Parent = Me.TabControl1

        ElseIf Me.Text = "Principal Component Analysis" Then
            Me.TabPageOptionsPCA.Parent = Me.TabControl1

        ElseIf Me.Text = "Multiple Correspondence Analysis" Then
            Me.ckFirstRow.Visible = True

        ElseIf Me.Text = "K-Means Clustering" Then
            Me.TabPageOptionsKmeans.Parent = Me.TabControl1
            Me.cbKmeansInitialization.Items.AddRange(New Object() {"K-Means++", "Forgy", "Random Partition", "User-Specified Centers"})
            Me.cbKmeansDistance.Items.AddRange(New Object() {"Squared Euclidean", "Euclidean"})
            Me.cbKmeansStandardization.Items.AddRange(New Object() {"None", "Z-scores", "Range 0 to 1"})
            Me.cbKmeansMissingPolicy.Items.AddRange(New Object() {"Error on missing", "Listwise deletion"})
            Me.cbKmeansEmptyCluster.Items.AddRange(New Object() {"Farthest observation", "Random observation", "Keep previous center"})
            Me.tbKmeansTolerance.Text = FormatUiDouble(0.000001)
            Me.tbKmeansSeed.Text = AppGlobals.GetDefaultRandomSeedText()
            Me.lblKmeansRowLabel.Visible = True
            Me.cbKmeansRowLabel.Visible = True

            Me.cbKmeansInitialization.SelectedIndex = 0
            Me.cbKmeansDistance.SelectedIndex = 0
            Me.cbKmeansStandardization.SelectedIndex = 0
            Me.cbKmeansMissingPolicy.SelectedIndex = 0
            Me.cbKmeansEmptyCluster.SelectedIndex = 0
            Me.refKmeansCenters.ExcelConnector = AppGlobals.app
            AddHandler Me.cbKmeansInitialization.SelectedIndexChanged, AddressOf Me.KMeansInitializationChanged
            Me.KMeansInitializationChanged(Me.cbKmeansInitialization, System.EventArgs.Empty)

        ElseIf Me.Text = "Hierarchical Clustering" Then
            Me.TabPageOptionsHierarchicalClustering.Parent = Me.TabControl1
            Me.lblKmeansRowLabel.Visible = True
            Me.cbKmeansRowLabel.Visible = True
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

        ElseIf Me.Text = "Factor Analysis" Then
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

        ElseIf Me.Text = "Discriminant Analysis" Then
            Me.TabPageOptionsDA.Parent = Me.TabControl1
            Me.lblGruppingVar.Visible = True
            Me.cbGruppingVar.Visible = True
            Me.lblKmeansRowLabel.Visible = True
            Me.cbKmeansRowLabel.Visible = True
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

            If Me.Text = "Discriminant Analysis" Then
                MyData = GetDiscriminantData()
            Else
                MyData = GetData()
            End If

            If MyData.bZeroValid Then 'check for zero valid data
                MsgBox("No valid observations")
                Exit Sub
            End If

            If Me.Text = "Scatter Plot MatrixType" Then
                Me.RunSPM(MyData)
            ElseIf Me.Text = "Principal Component Analysis" Then
                Me.RunPCA(MyData)
            ElseIf Me.Text = "Multiple Correspondence Analysis" Then
                Me.RunMCA(MyData)
            ElseIf Me.Text = "K-Means Clustering" Then
                Me.RunKmeans(MyData)
            ElseIf Me.Text = "Hierarchical Clustering" Then
                Me.RunHierarchicalClustering(MyData)
            ElseIf Me.Text = "Factor Analysis" Then
                Me.RunFA(MyData)
            ElseIf Me.Text = "Discriminant Analysis" Then
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

        Dim fitDA As New Multivariate.DiscriminantAnalysis
        fitDA.dataInputs(predictorData, groupLabels, rowLabels, predictorNames)

        Dim priorMode As Multivariate.DiscriminantPriorMode = Me.GetSelectedDiscriminantPriorMode()
        fitDA.settingsInputs(method:=Me.GetSelectedDiscriminantMethod(),
                         standardization:=Me.GetSelectedDiscriminantStandardization(),
                         missingPolicy:=Me.GetSelectedDiscriminantMissingPolicy(),
                         priorMode:=priorMode,
                         covarianceRegularization:=Me.GetSelectedDiscriminantRegularization())

        If priorMode = Multivariate.DiscriminantPriorMode.UserSpecified Then
            Dim priorLabels() As Object = Nothing
            Dim priorValues() As Double = Nothing
            Me.GetDiscriminantUserPriors(priorLabels, priorValues)
            fitDA.priorInputs(priorLabels, priorValues)
        End If

        fitDA.validationInputs(mode:=Me.GetSelectedDiscriminantValidationMode(),
                           numberOfFolds:=Me.nudDAFolds.Value,
                           holdoutFraction:=Me.tbDAHoldoutFraction.Value,
                           randomSeed:=Me.GetSelectedDiscriminantRandomSeed(),
                           stratified:=Me.ckDAStratified.Checked)
        fitDA.Fit()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter With {.wb = wb, .ws = wsData}
        wrData.write(Me.BuildDiscriminantInputDataTable(MyData, groupLabels, rowLabels))

        Dim wsResults As Worksheet = CType(wb.Worksheets.Add(After:=wsData), Worksheet)
        wsResults.Name = "Discriminant results"
        Dim wrSummary As New ExcelDnaResultWriter With {.wb = wb, .ws = wsResults}
        Dim rr = New ProcessListofResultTables(fitDA.wrapResults())
        rr.writeToSheet(wrSummary, True)
    End Sub

    Private Sub RunFA(MyData As DataObj)
        Dim fitFA As New Multivariate.FactorAnalysis
        fitFA.dataInputs(MyData.DataDbl, MyData.RowIds, MyData.varNames)
        fitFA.settingsInputs(
            maximumIteration:=Me.nudFAMaxIter.Value,
            dEps:=ParseUiDouble(Me.tbFAEps.Text, "Convergence tolerance"),
            analyzedMatrixType:=Me.GetSelectedFAMatrixType(),
            extractionMethod:=Me.GetSelectedFAExtractionMethod(),
            retentionMethod:=Me.GetSelectedFARetentionMethod(),
            retentionValue:=Me.GetSelectedFARetentionValue(),
            rotationMethod:=Me.GetSelectedFARotationMethod(),
            scoreMethod:=Me.GetSelectedFAScoreMethod(),
            communalityInitialization:=Me.GetSelectedFACommunalityInitialization(),
            missingValuePolicy:=Me.GetSelectedFAMissingPolicy(),
            useKaiserNormalization:=Me.ckFAKaiserNormalization.Checked,
            promaxPower:=CDbl(Me.nudFAPromaxPower.Value))
        fitFA.Calculate()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter With {.wb = wb, .ws = wsData}
        wrData.write(Me.BuildKMeansInputDataTable(MyData, Nothing))

        Dim wsResults As Worksheet = CType(wb.Worksheets.Add(After:=wsData), Worksheet)
        wsResults.Name = "Factor analysis results"
        Dim wrSummary As New ExcelDnaResultWriter With {.wb = wb, .ws = wsResults}
        Dim rr = New ProcessListofResultTables(fitFA.wrapResults())
        rr.writeToSheet(wrSummary, True)

        graphics.FactorAnalysisPlotExcel.ScreePlot(fitFA)
        graphics.FactorAnalysisPlotExcel.LoadingPlot2D(fitFA)
        graphics.FactorAnalysisPlotExcel.LoadingPlot3D(fitFA)
    End Sub

    Private Sub RunHierarchicalClustering(MyData As DataObj)
        Dim rowLabels() As String = Me.GetSelectedKMeansRowLabels(MyData)

        Dim fitHierarchical As New Multivariate.HierarchicalClustering
        fitHierarchical.dataInputs(MyData.DataDbl, rowLabels, MyData.varNames)
        fitHierarchical.settingsInputs(linkage:=Me.GetSelectedHierarchicalLinkage(),
                                       distanceMetric:=Me.GetSelectedHierarchicalDistanceMetric(),
                                       minkowskiPower:=ParseUiDouble(Me.tbHierarchicalMinkowskiPower.Text, "Minkowski power"),
                                       standardization:=Me.GetSelectedHierarchicalStandardization(),
                                       missingValuePolicy:=Me.GetSelectedHierarchicalMissingPolicy())
        fitHierarchical.reportInputs(cutMode:=Me.GetSelectedHierarchicalMembershipMode(),
                                     membershipClusterCount:=CInt(Me.nudHierarchicalClusters.Value),
                                     membershipCutHeight:=ParseUiDouble(Me.tbHierarchicalCutHeight.Text, "Cut height"))
        fitHierarchical.Fit()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter()
        wrData.wb = wb
        wrData.ws = wsData
        wrData.write(Me.BuildKMeansInputDataTable(MyData, rowLabels))

        Dim res = fitHierarchical.wrapResults()
        Dim wrSummary As New ExcelDnaResultWriter()
        Dim wsSummary As Worksheet = CType(wb.Worksheets.Add(After:=wsData), Worksheet)
        wsSummary.Name = "Hierarchical results"
        wrSummary.wb = wb
        wrSummary.ws = wsSummary
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(wrSummary, True)

        If Me.grpHierarchicalDendrogram IsNot Nothing AndAlso Me.ckHierarchicalCreateDendrogram.Checked Then
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

        Dim fitKmeans As New Multivariate.KMeans
        fitKmeans.dataInputs(MyData.DataDbl, rowLabels, MyData.varNames)

        Dim initialization As Multivariate.KMeansInitializationMethod = Me.GetSelectedKMeansInitialization()
        If initialization = Multivariate.KMeansInitializationMethod.UserSpecifiedCenters Then
            fitKmeans.startingCentersInputs(Me.ImportKMeansStartingCenters(expectedClusters:=CInt(Me.nudKmeansClusters.Value),
                                                                   expectedVariables:=MyData.nCols))
        End If

        Dim seed As Integer = Integer.MinValue
        If Me.tbKmeansSeed IsNot Nothing AndAlso Me.tbKmeansSeed.Text.Trim() <> String.Empty Then
            seed = ParseUiInteger(Me.tbKmeansSeed.Text, "Random seed")
        End If

        fitKmeans.settingsInputs(numberOfClusters:=CInt(Me.nudKmeansClusters.Value),
                             initialization:=initialization,
                             distanceMetric:=Me.GetSelectedKMeansDistanceMetric(),
                             nStarts:=CInt(Me.nudKmeansStarts.Value),
                             maxIterations:=CInt(Me.nudKmeansMaxIterations.Value),
                             convergenceTolerance:=ParseUiDouble(Me.tbKmeansTolerance.Text, "Convergence tolerance"),
                             standardization:=Me.GetSelectedKMeansStandardization(),
                             missingValuePolicy:=Me.GetSelectedKMeansMissingPolicy(),
                             emptyClusterHandling:=Me.GetSelectedKMeansEmptyClusterHandling(),
                             randomSeed:=seed)
        fitKmeans.Fit()

        Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
        Dim wsData As Worksheet = CType(wb.Worksheets(1), Worksheet)
        wsData.Name = "Data"
        Dim wrData As New ExcelDnaResultWriter()
        wrData.wb = wb
        wrData.ws = wsData
        wrData.write(Me.BuildKMeansInputDataTable(MyData, rowLabels))

        Dim res = fitKmeans.wrapResults()
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

        mca.DataMultiple(Matrix.Array2strArray(MyData.FinalData), MyData.varNames)
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
        Dim res = mca.wrapResults()
        WriteRes = New ExcelDnaResultWriter
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "MCA results"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

        'Add figures
        graphics.CorrespondenceAnalysisPlotExcel.CorrespondencePlot(mca)
        graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(mca, 0, False)
        graphics.CorrespondenceAnalysisPlotExcel.ContributionPlot(mca, 1, False)
    End Sub

    Private Sub RunPCA(MyData As DataObj)
        Dim strExtractMethod As String = String.Empty, Extractcoef As Double = 0.0, strExtractMethodLong As String = String.Empty
        Dim strMatrix As String = If(Me.optCorr.Checked, "Correlation", "Covariance")

        If Me.optExtractEigen.Checked Then
            strExtractMethod = "Eigenvalue"
            Extractcoef = CDbl(Me.spinBtnExtractEigen.Value)
            strExtractMethodLong = "Eigenvalue > " & CStr(Me.spinBtnExtractEigen.Value)

        ElseIf Me.optExtractFixed.checked Then
            strExtractMethod = "Fixed"
            Extractcoef = CDbl(Me.spinBtnExtractComp.Value)
            strExtractMethodLong = "Fixed Number of Components = " & CStr(Me.spinBtnExtractComp.Value)

        ElseIf Me.optExtractVariance.checked Then
            strExtractMethod = "Variance"
            Extractcoef = CDbl(Me.spinBtnExtractVariance.Value)
            strExtractMethodLong = "Explained Variance > " & CStr(Me.spinBtnExtractVariance.Value)

        End If

        Dim objPCA As New Multivariate.PCA
        With objPCA
            .dataInputs(MyData.DataDbl, MyData.RowIds, MyData.varNames, strExtractMethodLong)
            .settingsInputs(ParseUiInteger(Me.tbMaxIter.Text, "Maximum iterations"),
                            ParseUiDouble(Me.tbEps.Text, "Convergence epsilon"),
                            strMatrix)
            .Calculate(strExtractMethod, Extractcoef)
        End With

        'Dump results
        Dim WriteRes As ExcelDnaResultWriter = New ExcelDnaResultWriter
        WriteRes.wb = AppGlobals.app.Workbooks.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "Data"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(2)
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
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
        Dim res = objPCA.wrapResults()
        WriteRes = New ExcelDnaResultWriter
        AppGlobals.app.ActiveWorkbook.Worksheets.Add()
        AppGlobals.app.ActiveWorkbook.ActiveSheet.name = "PCA results"
        WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Dim rr = New ProcessListofResultTables(res)
        rr.writeToSheet(WriteRes, True)

        'Figures
        graphics.PcaPlotExcel.ScreePlot(objPCA)
        graphics.PcaPlotExcel.ScorePlot2D(objPCA)
        graphics.PcaPlotExcel.LoadingPlot2D(objPCA)
        graphics.PcaPlotExcel.Biplot(objPCA, 0.0)
        graphics.PcaPlotExcel.Biplot(objPCA, 0.5)
        graphics.PcaPlotExcel.Biplot(objPCA, 1.0)
        graphics.PcaPlotExcel.ScorePlot3D(objPCA)
        graphics.PcaPlotExcel.LoadingPlot3D(objPCA)

    End Sub

    Private Sub RunSPM(MyData As DataObj)
        Dim spm = New graphics.ScatterPlotMatrix(MyData.DataDbl, MyData.varNames, pWorkbook)
        spm.settingInputs(Me.ckDisplayCorrelCoef.Checked, Me.ckShowRegressionLines.Checked)
        spm.compute()
    End Sub

    Private Sub btAddX_Click(sender As Object, e As System.EventArgs) Handles btAddX.Click
        If Me.Text = "Discriminant Analysis" Then
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
            If pWorksheet.name <> Me.cbSheetsList.SelectedItem.ToString() Then 'new sheet selected clear all listboxes
                Me.lbXs.Items.Clear()
            End If
            newSheet = pWorkbook.worksheets(Me.cbSheetsList.SelectedItem.ToString())
            Me.Populate(newSheet)
        Else
            Me.Populate(pWorksheet)
        End If
    End Sub

    Private Sub cbGruppingVar_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cbGruppingVar.SelectedIndexChanged
        If Me.Text <> "Discriminant Analysis" Then Exit Sub

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

        If Me.Text = "K-Means Clustering" OrElse Me.Text = "Hierarchical Clustering" OrElse Me.Text = "Factor Analysis" Then
            Dim rowLabelKey As String = Me.GetSelectedKMeansRowLabelKey()
            If rowLabelKey <> String.Empty AndAlso keys.Contains(rowLabelKey) Then
                Throw New ArgumentException("The optional row label variable must not also be selected as an analysis variable.")
            End If

            'Keep missing values in the imported matrix so the clustering engine can apply
            'its own missing-value policy (error vs. listwise deletion).
            MyData.bAllowMissing = True
        End If

        Dim ref As String = BuildExcelRefList(pWorksheet, keys, Me.VariableColumnsInfo)
        If String.IsNullOrWhiteSpace(ref) Then
            Throw New ArgumentException("No valid worksheet reference could be constructed from the selected analysis variables.")
        End If

        If Me.Text = "Multiple Correspondence Analysis" Then
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

        If predictorKeys.Contains(groupingKey) Then
            Throw New ArgumentException("The grouping variable must not also be selected as an analysis variable.")
        End If

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

        If Me.Text = "Multiple Correspondence Analysis" Then
            'we accept character columns
            Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns, False)
        Else
            Me.VariableColumnsInfo = VarNamesToLBox(VarRng, MaxRows, Me.lbAllColumns)
        End If

        Me.AllColumnsInfo = Nothing
        If Me.Text = "K-Means Clustering" OrElse Me.Text = "Hierarchical Clustering" OrElse Me.Text = "Discriminant Analysis" Then
            Dim sink As New System.Windows.Forms.ListBox()
            Me.AllColumnsInfo = VarNamesToLBox(VarRng, MaxRows, sink, False)

            If Me.Text = "K-Means Clustering" OrElse Me.Text = "Hierarchical Clustering" Then
                Me.PopulateKMeansRowLabelItems()
            End If

            If Me.Text = "Discriminant Analysis" Then
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

    Private Sub PopulateKMeansRowLabelItems()
        If Me.cbKmeansRowLabel Is Nothing Then Exit Sub

        Dim previous As String = String.Empty
        If Me.cbKmeansRowLabel.SelectedItem IsNot Nothing Then previous = CStr(Me.cbKmeansRowLabel.SelectedItem)

        Me.cbKmeansRowLabel.Items.Clear()
        Me.cbKmeansRowLabel.Items.Add("(none)")

        If Me.AllColumnsInfo IsNot Nothing Then
            Dim infos As New List(Of VarColumnInfo)
            For Each kvp In Me.AllColumnsInfo
                infos.Add(kvp.Value)
            Next
            infos.Sort(Function(a As VarColumnInfo, b As VarColumnInfo) a.ColumnNumber.CompareTo(b.ColumnNumber))

            For Each info As VarColumnInfo In infos
                Me.cbKmeansRowLabel.Items.Add(info.DisplayText)
            Next
        End If

        If previous <> String.Empty AndAlso Me.cbKmeansRowLabel.Items.Contains(previous) Then
            Me.cbKmeansRowLabel.SelectedItem = previous
        Else
            Me.cbKmeansRowLabel.SelectedIndex = 0
        End If
    End Sub

    Private Function GetSelectedKMeansRowLabelKey() As String
        If Me.cbKmeansRowLabel Is Nothing OrElse Me.cbKmeansRowLabel.SelectedIndex <= 0 OrElse Me.cbKmeansRowLabel.SelectedItem Is Nothing Then
            Return String.Empty
        End If
        Return CStr(Me.cbKmeansRowLabel.SelectedItem)
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

            If String.IsNullOrWhiteSpace(labelText) Then labelText = $"Row {MyData.RowIds(i)}"
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
        Select Case Me.cbKmeansInitialization.SelectedIndex
            Case 1
                Return Multivariate.KMeansInitializationMethod.Forgy
            Case 2
                Return Multivariate.KMeansInitializationMethod.RandomPartition
            Case 3
                Return Multivariate.KMeansInitializationMethod.UserSpecifiedCenters
            Case Else
                Return Multivariate.KMeansInitializationMethod.KMeansPlusPlus
        End Select
    End Function

    Private Function GetSelectedKMeansDistanceMetric() As Multivariate.KMeansDistanceMetric
        If Me.cbKmeansDistance.SelectedIndex = 1 Then
            Return Multivariate.KMeansDistanceMetric.Euclidean
        End If
        Return Multivariate.KMeansDistanceMetric.SquaredEuclidean
    End Function

    Private Function GetSelectedKMeansStandardization() As Multivariate.ClusterStandardizationMode
        Select Case Me.cbKmeansStandardization.SelectedIndex
            Case 1
                Return Multivariate.ClusterStandardizationMode.ZScores
            Case 2
                Return Multivariate.ClusterStandardizationMode.RangeZeroToOne
            Case Else
                Return Multivariate.ClusterStandardizationMode.None
        End Select
    End Function

    Private Function GetSelectedKMeansMissingPolicy() As Multivariate.ClusterMissingValuePolicy
        If Me.cbKmeansMissingPolicy.SelectedIndex = 1 Then
            Return Multivariate.ClusterMissingValuePolicy.ListwiseDeletion
        End If
        Return Multivariate.ClusterMissingValuePolicy.ErrorOnMissing
    End Function

    Private Function GetSelectedKMeansEmptyClusterHandling() As Multivariate.EmptyClusterHandlingStrategy
        Select Case Me.cbKmeansEmptyCluster.SelectedIndex
            Case 1
                Return Multivariate.EmptyClusterHandlingStrategy.RandomObservation
            Case 2
                Return Multivariate.EmptyClusterHandlingStrategy.KeepPreviousCenter
            Case Else
                Return Multivariate.EmptyClusterHandlingStrategy.FarthestObservation
        End Select
    End Function

    Private Function BuildKMeansInputDataTable(MyData As DataObj, rowLabels() As String) As Object(,)
        Dim includeRowLabels As Boolean = (rowLabels IsNot Nothing AndAlso rowLabels.Length = MyData.nRows)
        Dim extraCols As Integer = If(includeRowLabels, 2, 1)
        Dim out(MyData.nRows, MyData.nCols + extraCols - 1) As Object

        out(0, 0) = "OriginalRow"
        If includeRowLabels Then out(0, 1) = "RowLabel"

        For j As Integer = 0 To MyData.nCols - 1
            out(0, j + extraCols) = MyData.varNames(j)
        Next

        For i As Integer = 0 To MyData.nRows - 1
            out(i + 1, 0) = MyData.RowIds(i)
            If includeRowLabels Then out(i + 1, 1) = rowLabels(i)

            For j As Integer = 0 To MyData.nCols - 1
                out(i + 1, j + extraCols) = MyData.FinalData(i, j)
            Next
        Next

        Return out
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
        Select Case Me.cbHierarchicalLinkage.SelectedIndex
            Case 1
                Return Multivariate.HierarchicalLinkageMethod.Complete
            Case 2
                Return Multivariate.HierarchicalLinkageMethod.Average
            Case 3
                Return Multivariate.HierarchicalLinkageMethod.WeightedAverage
            Case 4
                Return Multivariate.HierarchicalLinkageMethod.SingleLinkage
            Case 5
                Return Multivariate.HierarchicalLinkageMethod.Centroid
            Case 6
                Return Multivariate.HierarchicalLinkageMethod.Median
            Case Else
                Return Multivariate.HierarchicalLinkageMethod.Ward
        End Select
    End Function

    Private Function GetSelectedHierarchicalDistanceMetric() As Multivariate.HierarchicalDistanceMetric
        Select Case Me.cbHierarchicalDistance.SelectedIndex
            Case 1
                Return Multivariate.HierarchicalDistanceMetric.Euclidean
            Case 2
                Return Multivariate.HierarchicalDistanceMetric.Manhattan
            Case 3
                Return Multivariate.HierarchicalDistanceMetric.Chebyshev
            Case 4
                Return Multivariate.HierarchicalDistanceMetric.Minkowski
            Case 5
                Return Multivariate.HierarchicalDistanceMetric.Cosine
            Case 6
                Return Multivariate.HierarchicalDistanceMetric.Correlation
            Case Else
                Return Multivariate.HierarchicalDistanceMetric.SquaredEuclidean
        End Select
    End Function

    Private Function GetSelectedHierarchicalStandardization() As Multivariate.ClusterStandardizationMode
        Select Case Me.cbHierarchicalStandardization.SelectedIndex
            Case 1
                Return Multivariate.ClusterStandardizationMode.ZScores
            Case 2
                Return Multivariate.ClusterStandardizationMode.RangeZeroToOne
            Case Else
                Return Multivariate.ClusterStandardizationMode.None
        End Select
    End Function

    Private Function GetSelectedHierarchicalMissingPolicy() As Multivariate.ClusterMissingValuePolicy
        If Me.cbHierarchicalMissingPolicy.SelectedIndex = 1 Then
            Return Multivariate.ClusterMissingValuePolicy.ListwiseDeletion
        End If
        Return Multivariate.ClusterMissingValuePolicy.ErrorOnMissing
    End Function

    Private Function GetSelectedHierarchicalMembershipMode() As Multivariate.HierarchicalMembershipDisplayMode
        If Me.optHierarchicalCutByHeight IsNot Nothing AndAlso Me.optHierarchicalCutByHeight.Checked Then
            Return Multivariate.HierarchicalMembershipDisplayMode.ByHeight
        End If
        Return Multivariate.HierarchicalMembershipDisplayMode.ByClusterCount
    End Function

    Private Function GetSelectedHierarchicalDendrogramHeightMode() As Multivariate.DendrogramHeightMode
        If Me.cbHierarchicalHeightMode.SelectedIndex = 1 Then
            Return Multivariate.DendrogramHeightMode.StepLevels
        End If
        Return Multivariate.DendrogramHeightMode.MergeDistance
    End Function

    Private Function GetSelectedHierarchicalDendrogramOrientation() As Multivariate.DendrogramOrientation
        Select Case Me.cbHierarchicalOrientation.SelectedIndex
            Case 1
                Return Multivariate.DendrogramOrientation.Bottom
            Case 2
                Return Multivariate.DendrogramOrientation.Left
            Case 3
                Return Multivariate.DendrogramOrientation.Right
            Case Else
                Return Multivariate.DendrogramOrientation.Top
        End Select
    End Function

    Private Function GetSelectedHierarchicalDendrogramLabelMode() As Multivariate.DendrogramLabelMode
        Select Case Me.cbHierarchicalLabelMode.SelectedIndex
            Case 1
                Return Multivariate.DendrogramLabelMode.AxisTitle
            Case 2
                Return Multivariate.DendrogramLabelMode.None
            Case Else
                Return Multivariate.DendrogramLabelMode.DataLabels
        End Select
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
        If Me.optFACovariance.Checked Then
            Return Multivariate.FactorAnalysisMatrixType.Covariance
        End If
        Return Multivariate.FactorAnalysisMatrixType.Correlation
    End Function

    Private Function GetSelectedFAExtractionMethod() As Multivariate.FactorAnalysisExtractionMethod
        Select Case Me.cbFAExtraction.SelectedIndex
            Case 1
                Return Multivariate.FactorAnalysisExtractionMethod.PrincipalAxis
            Case 2
                Return Multivariate.FactorAnalysisExtractionMethod.MaximumLikelihood ' MaximumLikelihood after enum extension
            Case 3
                Return Multivariate.FactorAnalysisExtractionMethod.GeneralizedLeastSquares ' GeneralizedLeastSquares after enum extension
            Case 4
                Return Multivariate.FactorAnalysisExtractionMethod.Image ' Image after enum extension
            Case 5
                Return Multivariate.FactorAnalysisExtractionMethod.Alpha ' Alpha after enum extension
            Case Else
                Return Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents
        End Select
    End Function

    Private Function GetSelectedFARetentionMethod() As Multivariate.FactorAnalysisRetentionMethod
        If Me.optFAExtractEigen.Checked Then
            Return Multivariate.FactorAnalysisRetentionMethod.Eigenvalue
        End If
        If Me.optFAExtractVariance.Checked Then
            Return Multivariate.FactorAnalysisRetentionMethod.Variance
        End If
        Return Multivariate.FactorAnalysisRetentionMethod.Fixed
    End Function

    Private Function GetSelectedFARetentionValue() As Double
        If Me.optFAExtractEigen.Checked Then
            Return CDbl(Me.nudFAEigen.Value)
        End If
        If Me.optFAExtractVariance.Checked Then
            Return CDbl(Me.nudFAVariance.Value)
        End If
        Return CDbl(Me.nudFAFactors.Value)
    End Function

    Private Function GetSelectedFARotationMethod() As Multivariate.FactorAnalysisRotationMethod
        Select Case Me.cbFARotation.SelectedIndex
            Case 1
                Return Multivariate.FactorAnalysisRotationMethod.Varimax
            Case 2
                Return Multivariate.FactorAnalysisRotationMethod.Quartimax
            Case 3
                Return Multivariate.FactorAnalysisRotationMethod.Equamax
            Case 4
                Return Multivariate.FactorAnalysisRotationMethod.Promax
            Case Else
                Return Multivariate.FactorAnalysisRotationMethod.None
        End Select
    End Function

    Private Function GetSelectedFAScoreMethod() As Multivariate.FactorAnalysisScoreMethod
        Select Case Me.cbFAScoreMethod.SelectedIndex
            Case 1
                Return Multivariate.FactorAnalysisScoreMethod.Regression
            Case 2
                Return Multivariate.FactorAnalysisScoreMethod.Bartlett
            Case Else
                Return Multivariate.FactorAnalysisScoreMethod.None
        End Select
    End Function

    Private Function GetSelectedFACommunalityInitialization() As Multivariate.FactorAnalysisCommunalityInitialization
        If Me.cbFACommunalityInit.SelectedIndex = 1 Then
            Return Multivariate.FactorAnalysisCommunalityInitialization.One
        End If
        Return Multivariate.FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation
    End Function

    Private Function GetSelectedFAMissingPolicy() As Multivariate.FactorAnalysisMissingValuePolicy
        If Me.cbFAMissingPolicy.SelectedIndex = 1 Then
            Return Multivariate.FactorAnalysisMissingValuePolicy.ListwiseDeletion
        End If
        Return Multivariate.FactorAnalysisMissingValuePolicy.ErrorOnMissing
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
        Dim predictorNames() As String = Me.ExtractDiscriminantPredictorNames(MyData)
        Dim includeRowLabels As Boolean = (rowLabels IsNot Nothing AndAlso rowLabels.Length = MyData.nRows)

        Dim totalCols As Integer = predictorNames.Length + 1
        If includeRowLabels Then totalCols += 1

        Dim out(MyData.nRows, totalCols - 1) As Object
        Dim col As Integer = 0

        If includeRowLabels Then
            out(0, col) = "Row label"
            col += 1
        End If

        out(0, col) = MyData.varNames(0)
        col += 1

        For j As Integer = 0 To predictorNames.Length - 1
            out(0, col + j) = predictorNames(j)
        Next

        For i As Integer = 0 To MyData.nRows - 1
            col = 0

            If includeRowLabels Then
                out(i + 1, col) = rowLabels(i)
                col += 1
            End If

            out(i + 1, col) = groupLabels(i)
            col += 1

            For j As Integer = 1 To MyData.nCols - 1
                out(i + 1, col + j - 1) = MyData.FinalData(i, j)
            Next
        Next

        Return out
    End Function

    Private Function GetSelectedDiscriminantMethod() As Multivariate.DiscriminantAnalysisMethod
        Dim txt As String = Me.cbDAMethod.SelectedItem
        If txt.IndexOf("quadratic", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("qda", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantAnalysisMethod.Quadratic
        End If
        Return Multivariate.DiscriminantAnalysisMethod.Linear
    End Function

    Private Function GetSelectedDiscriminantMissingPolicy() As Multivariate.ClusterMissingValuePolicy
        Dim txt As String = Me.cbDAMissingPolicy.SelectedItem
        If txt.IndexOf("listwise", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.ClusterMissingValuePolicy.ListwiseDeletion
        End If
        Return Multivariate.ClusterMissingValuePolicy.ErrorOnMissing
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

            If String.IsNullOrWhiteSpace(labelText) Then labelText = $"Row {MyData.RowIds(i)}"
            out(i) = labelText
        Next

        Return out
    End Function

    Private Sub GetDiscriminantUserPriors(ByRef categoryLabels() As Object, ByRef priorProbabilities() As Double)
        Dim txt As String = Me.tbDAUserPriors.Text
        If txt.Trim() = String.Empty Then
            Throw New ArgumentException("User-specified priors were selected, but no prior definitions were supplied. Use the format GroupA=0.4; GroupB=0.6 or place each definition on a new line.")
        End If

        Dim normalized As String = txt.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim chunks() As String = normalized.Split(New String() {vbLf, ";"}, StringSplitOptions.RemoveEmptyEntries)

        Dim labelList As New List(Of Object)
        Dim valueList As New List(Of Double)

        For Each rawChunk As String In chunks
            Dim chunk As String = rawChunk.Trim()
            If chunk = String.Empty Then Continue For

            Dim splitPos As Integer = chunk.IndexOf("="c)
            If splitPos < 0 Then splitPos = chunk.IndexOf(":"c)
            If splitPos <= 0 OrElse splitPos >= chunk.Length - 1 Then
                Throw New ArgumentException("Each user prior must be written as label=probability (for example: GroupA=0.4). Separate multiple definitions with semicolons or new lines.")
            End If

            Dim label As String = chunk.Substring(0, splitPos).Trim()
            Dim valueText As String = chunk.Substring(splitPos + 1).Trim()

            If label = String.Empty Then
                Throw New ArgumentException("Each user prior must include a non-empty group label.")
            End If

            Dim p As Double
            If Not Double.TryParse(valueText,
                               System.Globalization.NumberStyles.Float Or System.Globalization.NumberStyles.AllowThousands,
                               System.Globalization.CultureInfo.CurrentCulture,
                               p) Then
                If Not Double.TryParse(valueText,
                                   System.Globalization.NumberStyles.Float Or System.Globalization.NumberStyles.AllowThousands,
                                   System.Globalization.CultureInfo.InvariantCulture,
                                   p) Then
                    Throw New ArgumentException("Could not parse the user-specified prior probability '" & valueText & "'.")
                End If
            End If

            labelList.Add(label)
            valueList.Add(p)
        Next

        If labelList.Count = 0 Then
            Throw New ArgumentException("No valid user-specified priors were found.")
        End If

        categoryLabels = labelList.ToArray()
        priorProbabilities = valueList.ToArray()
    End Sub

    Private Function GetSelectedDiscriminantStandardization() As Multivariate.ClusterStandardizationMode
        Dim txt As String = Me.cbDAStandardization.SelectedItem
        If txt.IndexOf("z", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.ClusterStandardizationMode.ZScores
        End If
        If txt.IndexOf("range", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("0 to 1", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.ClusterStandardizationMode.RangeZeroToOne
        End If
        Return Multivariate.ClusterStandardizationMode.None
    End Function

    Private Sub PopulateOptionalDiscriminantRowLabelItems()
        Dim cb = Me.cbKmeansRowLabel
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
        Dim txt As String = String.Empty
        If Me.cbDAPriors IsNot Nothing AndAlso Me.cbDAPriors.SelectedItem IsNot Nothing Then txt = CStr(Me.cbDAPriors.SelectedItem)

        If txt.IndexOf("user", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantPriorMode.UserSpecified
        End If
        If txt.IndexOf("equal", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantPriorMode.Equal
        End If
        Return Multivariate.DiscriminantPriorMode.ProportionalToGroupSizes
    End Function

    Private Function GetSelectedDiscriminantValidationMode() As Multivariate.DiscriminantValidationMode
        Dim txt As String = String.Empty
        If Me.cbDAValidation IsNot Nothing AndAlso Me.cbDAValidation.SelectedItem IsNot Nothing Then txt = CStr(Me.cbDAValidation.SelectedItem)

        If txt.IndexOf("leave", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantValidationMode.LeaveOneOut
        End If
        If txt.IndexOf("k-fold", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("k fold", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("kfold", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantValidationMode.KFold
        End If
        If txt.IndexOf("holdout", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantValidationMode.Holdout
        End If
        Return Multivariate.DiscriminantValidationMode.None
    End Function

    Private Function GetSelectedDiscriminantRandomSeed() As Integer
        Dim txt As String = Me.tbDASeed.text
        If txt.Trim() = String.Empty Then Return Integer.MinValue
        Return ParseUiInteger(txt, "Random seed")
    End Function

    Private Function GetSelectedDiscriminantRegularization() As Double
        Dim txt As String = Me.tbDARegularization.Text
        If txt.Trim() = String.Empty Then Return 0.00000001
        Return ParseUiDouble(txt, "Covariance regularization")
    End Function

    Private Sub UpdateDiscriminantOptionStates()
        If Me.Text <> "Discriminant Analysis" Then Exit Sub

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
        If Me.cbKmeansRowLabel Is Nothing OrElse Me.cbKmeansRowLabel.SelectedIndex <= 0 OrElse Me.cbKmeansRowLabel.SelectedItem Is Nothing Then
            Return String.Empty
        End If
        Return CStr(Me.cbKmeansRowLabel.SelectedItem)
    End Function

End Class