Imports System.Runtime.InteropServices.ComTypes
Imports System.Security.Cryptography
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class UibyID
    Private Const BoxPlotLegacyPaletteIndex As Integer = 4

    Sub New(analysis As String, tagn As Integer)
        ' This call is required by the designer.
        InitializeComponent()

        Me.RefEdit1.ExcelConnector = AppGlobals.app
        Me.RefEdit2.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.Text = analysis
        Me.Tag = tagn
        Me.spinBtnAlpha.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha.Minimum, Me.spinBtnAlpha.Maximum)
        Me.spinBtnAlphaOutliers.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlphaOutliers.Minimum, Me.spinBtnAlphaOutliers.Maximum)

        'set all "Options" tab to invisible and then later show just that required based on analysis.
        'options for other outputs https://stackoverflow.com/questions/12740073/programmatically-hide-remove-tabpages-in-vb-net
        Me.TabPage_Options.Parent = Nothing
        Me.TabPage_OptionsDescriptive.Parent = Nothing
        Me.TabPage_OptionsHistogram.Parent = Nothing
        Me.TabPage_OptionsNormalPlot.Parent = Nothing
        Me.TabPage_OptionsSymmetry.Parent = Nothing
        Me.TabPage_OptionsOutliers.Parent = Nothing
        Me.TabPage_OptionsUTT.Parent = Nothing
        Me.TabPage_OptionsCategoricalHistogram.Parent = Nothing
        Me.TabPage_OptionsViolin.Parent = Nothing
        Me.TabPage_OptionsBoxPlot.Parent = Nothing
        Me.TabPage_CDFplot.Parent = Nothing
        Me.TabPage_CDFplotAppearance.Parent = Nothing

        If Me.Tag = HelpTopic.KruskalWallisTest Then
            Me.TabPage_Options.Parent = Me.TabControl1
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.grpHomogeneityVariances.Visible = True

        ElseIf Me.Tag = HelpTopic.BoxAndWhiskers Then
            Me.TabPage_OptionsBoxPlot.Parent = Me.TabControl1
            Me.cmbBoxPalette.Items.AddRange(New Object() {"Tableau 10", "Okabe-Ito", "ColorBrewer Set1", "Grayscale", "Legacy BESHStat box plot"})
            Me.cmbBoxPalette.SelectedIndex = 0
            Me.UpdateBoxPlotOptionState()

        ElseIf Me.Tag = HelpTopic.OneWayANOVA Then
            Me.TabPage_Options.Parent = Me.TabControl1
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.grpHomogeneityVariances.Visible = True
            Me.grpANOVA1MCP.Visible = True
            Me.ckWelch.Visible = True

        ElseIf Me.Tag = HelpTopic.MannWhitneyTest Then
            Me.TabPage_Options.Parent = Me.TabControl1
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.ckEstimateOfShift.Visible = True
            Me.spinBtnAlpha.Visible = True
            Me.lblAlpha.Visible = True

        ElseIf Me.Tag = HelpTopic.UnpairedTTest Then
            Me.TabPage_OptionsUTT.Parent = Me.TabControl1
            Me.spinBtnAlpha_UTT.Value = AppGlobals.GetDefaultAlphaDecimal(Me.spinBtnAlpha_UTT.Minimum, Me.spinBtnAlpha_UTT.Maximum)
            Me.UpdateUnpairedTtestOptionVisibility()
            Me.ApplyUnpairedTtestInputLabels()

        ElseIf Me.Tag = HelpTopic.ROCCurve Then
            Me.TabPage_Options.Parent = Me.TabControl1
            Me.ckDescriptiveStatistics.Visible = True
            Me.spinBtnAlpha.Visible = True
            Me.lblAlpha.Visible = True

        ElseIf Me.Tag = HelpTopic.DescriptiveStatistics Then
            Me.TabPage_OptionsDescriptive.Parent = Me.TabControl1
            Me.ckBoxPlot_Descriptive.Visible = True

        ElseIf Me.Tag = HelpTopic.NormalityTests Then
            Me.TabPage_Options.Parent = Me.TabControl1
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True

        ElseIf Me.Tag = HelpTopic.Histogram Then
            Me.TabPage_OptionsHistogram.Parent = Me.TabControl1

        ElseIf Me.Tag = HelpTopic.NormalPlot Then
            Me.TabPage_OptionsNormalPlot.Parent = Me.TabControl1

        ElseIf Me.Tag = HelpTopic.HomogeneityOfVariance Then
            Me.TabPage_Options.Parent = Me.TabControl1
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.grpHomogeneityVariances.Visible = True

        ElseIf Me.Tag = HelpTopic.Symmetry Then
            Me.TabPage_OptionsSymmetry.Parent = Me.TabControl1

        ElseIf Me.Tag = HelpTopic.UnivariateOutliers Then
            Me.TabPage_OptionsOutliers.Parent = Me.TabControl1

        ElseIf Me.Tag = HelpTopic.CategoricalHistogram Then
            Me.TabPage_OptionsCategoricalHistogram.Parent = Me.TabControl1
            Me.cmbCatHistPalette.Items.AddRange(New Object() {"Tableau 10", "Okabe-Ito", "ColorBrewer Set1", "Grayscale"})
            Me.cmbCatHistPalette.SelectedIndex = 0
            'Categorical histograms support both long/by-ID and wide/by-column layouts.
            Me.optByID.Checked = True
            Me.optByID.Enabled = True
            Me.optByColumn.Enabled = True
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
            Me.UpdateCategoricalHistogramOptionState()

        ElseIf Me.Tag = HelpTopic.ViolinPlot Then
            Me.TabPage_OptionsViolin.Parent = Me.TabControl1
            Me.cmbViolinBandwidth.Items.AddRange(New Object() {"Silverman (automatic)", "Scott", "Manual"})
            Me.cmbViolinBandwidth.SelectedIndex = 0
            Me.cmbViolinPalette.Items.AddRange(New Object() {"Tableau 10", "Okabe-Ito", "ColorBrewer Set1", "Grayscale"})
            Me.cmbViolinPalette.SelectedIndex = 0
            Me.cmdViolinScaling.Items.AddRange(New Object() {"Equal maximum width", "Equal area", "Width proportional to N"})
            Me.cmdViolinScaling.SelectedIndex = 0
            'Violin plots support both long/by-ID and wide/by-column layouts.
            Me.optByID.Enabled = True
            Me.optByColumn.Enabled = True
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"

        ElseIf Me.Tag = HelpTopic.CDFplot Then
            Me.TabPage_CDFplot.Parent = Me.TabControl1
            Me.TabPage_CDFplotAppearance.Parent = Me.TabControl1
            Me.cmbCDFPlotType.Items.AddRange(New Object() {"Empirical CDF", "Empirical CDF + fitted distribution", "Fitted distribution only"})
            Me.cmbCDFPlotType.SelectedIndex = 1

            Me.cmbCDFDistribution.Items.AddRange(New Object() {
                "Normal",
                "Lognormal",
                "3-parameter Lognormal",
                "Gamma",
                "3-parameter Gamma",
                "Exponential",
                "2-parameter Exponential",
                "Smallest Extreme Value",
                "Weibull",
                "3-parameter Weibull",
                "Largest Extreme Value",
                "Logistic",
                "Loglogistic",
                "3-parameter Loglogistic"})
            Me.cmbCDFDistribution.SelectedIndex = 0

            Me.cmbCDFYScale.Items.AddRange(New Object() {"Probability", "Percent"})
            Me.cmbCDFYScale.SelectedIndex = 1

            Me.cmbCDFEmpiricalMethod.Items.AddRange(New Object() {"Exact ECDF", "Minitab median rank"})
            Me.cmbCDFEmpiricalMethod.SelectedIndex = 0

            Me.cmbCDFLegendMode.Items.AddRange(New Object() {"Groups only", "All curves"})
            Me.cmbCDFLegendMode.SelectedIndex = 0

            Me.cmbCDFPalette.Items.AddRange(New Object() {"Tableau 10", "Okabe-Ito", "ColorBrewer Set1", "Grayscale"})
            Me.cmbCDFPalette.SelectedIndex = 0

            'CDF/ECDF plots support both long/by-ID and wide/by-column layouts.
            Me.optByID.Checked = True
            Me.optByID.Enabled = True
            Me.optByColumn.Enabled = True
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"

            Me.UpdateCDFOptionState()

        End If

        Me.RefEdit1.txtAddress.Select()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Or Me.optByID.Checked Then
            If CheckRefEdit(Me.RefEdit1.Address, True) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEdit1)
                bOut = True
            End If

            If CheckRefEdit(Me.RefEdit2.Address, True) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEdit2)
                bOut = True
            End If
        Else
            If CheckRefEdit(Me.RefEdit2.Address) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEdit2)
                bOut = True
            End If
        End If

        If Me.optOutputRange.Checked Then
            If CheckRefEdit(Me.RefEditOutput.Address) Then
                Me.TabControl1.SelectedIndex = 0
                RefEditReset(Me.RefEditOutput)
                bOut = True
            End If
        End If
        Return bOut
    End Function

    Private Function getData2Groups(ByRef strErr As String) As TwoGroupsData
        Dim out = New TwoGroupsData
        Dim columData = New DataObj, columData2 = New DataObj
        'for by ID declarations
        Dim refId As String, refData As String, refFinal As String
        Dim groupIDs() As Object

        If Me.optByColumn.Checked Then
            'Group by Column
            ExcelDnaDataImporter.ImportInto(columData, prepareRef2D(Me.RefEdit1.Address), True)
            ExcelDnaDataImporter.ImportInto(columData2, prepareRef2D(Me.RefEdit2.Address), True)

            out.X1 = Matrix.GetColumnFrom2Darray(columData.DataDbl, 0)
            out.X2 = Matrix.GetColumnFrom2Darray(columData2.DataDbl, 0)
            out.name1 = columData.varNames(0)
            out.name2 = columData2.varNames(0)
        Else
            'Group by identifier. We expect only two groups for Mann-Whitney test
            If WorksheetNameFromRefAdress(Me.RefEdit1.Address, True) <> WorksheetNameFromRefAdress(Me.RefEdit2.Address, True) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Input reference range adresses are from different sheets. Input can be only from one sheet."))
            End If

            refId = prepareRef2D(Me.RefEdit1.Address)
            refData = prepareRef2D(Me.RefEdit2.Address)
            'join reference address into one (remove sheet name from the second and concatenate).
            'Data can be only form one sheet because of the above check
            refFinal = refId & ", " & Replace(refData, WorksheetNameFromRefAdress(refData, True) & "!", String.Empty) 'Remove "Sheet1!" from string
            ExcelDnaDataImporter.ImportInto(columData, refFinal, True, 0)

            'get unique group IDs

            groupIDs = Matrix.GetColumnFrom2Darray(columData.FinalData, 0).Distinct().ToArray()
            If groupIDs.Length <> 2 Then strErr = "Number Of groups must be eq 2"

            Dim coljagged()() As Double = columData.DataByID2ByColumn()
            out.X1 = coljagged(0)
            out.X2 = coljagged(1)
            out.name1 = CStr(groupIDs(0))
            out.name2 = CStr(groupIDs(1))
        End If

        Return out
    End Function

    Private Function getDataMultipleGroups(ByRef strErr As String) As MultiGroupsUnpairedData
        Dim byIdData = New DataObj
        Dim out = New MultiGroupsUnpairedData
        'for by ID declarations
        Dim refId As String, refData As String, refFinal As String
        Dim groupIDs() As String, NoGroups As Integer, ii As Integer

        If Me.optByColumn.Checked Then
            'Group by Column. Loop throug each column separately because data are not paired
            Dim colList() As String = ColumListFromRefAdress(Me.RefEdit2.Address)
            NoGroups = colList.Length
            Dim ref As String = prepareRef2D(Me.RefEdit2.Address)
            ii = 0 'use separate column counter so we can drop completely missing columns

            Dim outData()() As Double = New Double()() {}
            ReDim groupIDs(NoGroups - 1), outData(NoGroups - 1)
            For i = 0 To NoGroups - 1
                Dim ref1 As String = WorksheetNameFromRefAdress(ref, True) & "!" & colList(i)
                'Debug.Print(ref1)
                Dim columData = New DataObj
                ExcelDnaDataImporter.ImportInto(columData, ref1, True)
                If columData.FinalData IsNot Nothing Then 'save data
                    outData(ii) = Matrix.GetColumnFrom2Darray(columData.DataDbl, 0)
                    groupIDs(ii) = columData.varNames(0)
                    ii += 1
                End If
            Next
            If ii = 0 Then 'no valid data column
                strErr = "No valid data"
                Return Nothing
            Else
                ReDim Preserve groupIDs(ii - 1)
                ReDim Preserve outData(ii - 1)
                out.X = outData
                out.varNames = groupIDs
            End If

            Return out
        Else
            'Group by identifier
            If WorksheetNameFromRefAdress(Me.RefEdit1.Address, True) <> WorksheetNameFromRefAdress(Me.RefEdit2.Address, True) Then
                strErr = "Input reference range adresses are from different sheets. Input can be only from one sheet."
            End If

            refId = prepareRef2D(Me.RefEdit1.Address)
            refData = prepareRef2D(Me.RefEdit2.Address)
            'join reference address into one (remove sheet name from the second and concatenate).
            'Data can be only form one sheet because of the above check
            refFinal = refId & ", " & Replace(refData, WorksheetNameFromRefAdress(refData, True) & "!", String.Empty) 'Remove "Sheet1!" from string
            ExcelDnaDataImporter.ImportInto(byIdData, refFinal, True, 0)

            'Debug.Print(array2str(byIdData.FinalData))
            out.X = byIdData.DataByID2ByColumn()
            out.varNames = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(byIdData.FinalData, 0).Distinct().ToArray())
            Return out
        End If
    End Function

    Private NotInheritable Class GroupedContinuousInputData
        Public Values() As Double
        Public Groups() As Object
        Public ContinuousName As String
        Public GroupName As String
    End Class

    Private Function getGroupedContinuousData(ByRef strErr As String) As GroupedContinuousInputData
        If Me.optByColumn.Checked Then
            Return Me.getGroupedContinuousDataByColumn(strErr)
        End If

        Return Me.getGroupedContinuousDataById(strErr)
    End Function

    Private Function getGroupedContinuousDataByColumn(ByRef strErr As String) As GroupedContinuousInputData
        'Wide layout: each selected worksheet column is one group. Missing values
        'are handled independently within each column, so rows do not need to be
        'paired across groups. This path is shared by violin plots and categorical
        'histograms.
        Dim dataWorksheet As Worksheet = WorksheetFromRefAdress(Me.RefEdit2.Address, Me.RefEdit2.ExcelWorkBook)
        Dim ref As String = prepareRef2D(Me.RefEdit2.Address, Me.RefEdit2.ExcelWorkBook)
        Dim colList() As String = ColumListFromRefAdress(Me.RefEdit2.Address, Me.RefEdit2.ExcelWorkBook)

        If colList Is Nothing OrElse colList.Length = 0 Then
            strErr = "Select at least one continuous data column."
            Return Nothing
        End If

        Dim values As New List(Of Double)()
        Dim groups As New List(Of Object)()
        Dim usedGroupNames As New Dictionary(Of String, Integer)(StringComparer.CurrentCultureIgnoreCase)

        For i As Integer = 0 To colList.Length - 1
            Dim columnAddress As String = LTrim$(colList(i))
            Dim columnRef As String = WorksheetNameFromRefAdress(ref, True, Me.RefEdit2.ExcelWorkBook) & "!" & columnAddress
            Dim columnData As New DataObj
            ExcelDnaDataImporter.ImportInto(columnData, columnRef, True)

            'Completely empty/non-numeric columns are simply omitted, matching the
            'existing multiple-groups by-column import behaviour.
            If columnData.bZeroValid OrElse columnData.FinalData Is Nothing OrElse columnData.nRows = 0 Then
                Continue For
            End If
            If columnData.nCols <> 1 Then
                strErr = "Each by-column input must resolve to a single worksheet column."
                Return Nothing
            End If

            Dim sourceRange As Range = dataWorksheet.Range(columnAddress)
            Dim firstRowIsTextLabel As Boolean = False
            Dim groupName As String = ResolveGroupedColumnName(sourceRange, firstRowIsTextLabel)
            groupName = MakeUniqueGroupedColumnName(groupName, usedGroupNames)

            Dim columnValues() As Double = Matrix.GetColumnFrom2Darray(columnData.DataDbl, 0)
            Dim firstValueIndex As Integer = 0

            'A numeric-looking text header (for example "0") is accepted by DataObj
            'as numeric data. If that first worksheet row was explicitly text, remove
            'it from the plotted observations while retaining it as the group label.
            If firstRowIsTextLabel AndAlso columnData.RowIds IsNot Nothing AndAlso
               columnData.RowIds.Length > 0 AndAlso columnData.RowIds(0) = sourceRange.Row Then
                firstValueIndex = 1
            End If

            For valueIndex As Integer = firstValueIndex To columnValues.Length - 1
                values.Add(columnValues(valueIndex))
                groups.Add(groupName)
            Next
        Next

        If values.Count = 0 Then
            strErr = "No valid continuous observations were found in the selected columns."
            Return Nothing
        End If

        Return New GroupedContinuousInputData With {
            .Values = values.ToArray(),
            .Groups = groups.ToArray(),
            .ContinuousName = "Value",
            .GroupName = "Group"
        }
    End Function

    Private Shared Function ResolveGroupedColumnName(sourceRange As Range, ByRef firstRowIsTextLabel As Boolean) As String
        firstRowIsTextLabel = False
        If sourceRange Is Nothing Then Return "Group"

        Dim firstCell As Range = DirectCast(sourceRange.Cells(1, 1), Range)
        Dim firstValue As Object = ExcelDnaDataImporter.NormalizeExcelValue(firstCell.Value2)

        'Any non-blank text in the first selected row is an explicit group label.
        'A genuinely numeric first value remains data and falls back to the Excel
        'column identifier (A, B, C, ...).
        If TypeOf firstValue Is String Then
            Dim textLabel As String = CStr(firstValue).Trim()
            If textLabel <> String.Empty Then
                firstRowIsTextLabel = True
                Return textLabel
            End If
        End If

        Dim entireColumnAddress As String = DirectCast(sourceRange.Cells(1, 1), Range).EntireColumn.Address(False, False)
        Dim columnName As String = entireColumnAddress.Split(":"c)(0).Replace("$", String.Empty)
        Return columnName
    End Function

    Private Shared Function MakeUniqueGroupedColumnName(groupName As String, usedNames As Dictionary(Of String, Integer)) As String
        Dim baseName As String = If(String.IsNullOrWhiteSpace(groupName), "Group", groupName.Trim())
        Dim occurrence As Integer = 0

        If usedNames.TryGetValue(baseName, occurrence) Then
            occurrence += 1
            usedNames(baseName) = occurrence
            Return baseName & " (" & occurrence.ToString() & ")"
        End If

        usedNames.Add(baseName, 1)
        Return baseName
    End Function

    Private Function getGroupedContinuousDataById(ByRef strErr As String) As GroupedContinuousInputData
        'Resolve the ranges using the workbook associated with each RefEdit.
        Dim groupWorksheet As Worksheet = WorksheetFromRefAdress(Me.RefEdit1.Address, Me.RefEdit1.ExcelWorkBook)
        Dim dataWorksheet As Worksheet = WorksheetFromRefAdress(Me.RefEdit2.Address, Me.RefEdit2.ExcelWorkBook)
        Dim groupWorkbook As Workbook = DirectCast(groupWorksheet.Parent, Workbook)
        Dim dataWorkbook As Workbook = DirectCast(dataWorksheet.Parent, Workbook)

        'Both variables must come from the same workbook and worksheet.
        If Not String.Equals(groupWorkbook.FullName, dataWorkbook.FullName,
                         StringComparison.OrdinalIgnoreCase) OrElse
           Not String.Equals(groupWorksheet.Name, dataWorksheet.Name,
                             StringComparison.OrdinalIgnoreCase) Then

            strErr = "Categorical and continuous variables must be on the same worksheet."
            Return Nothing
        End If

        Dim groupRange As Range = groupWorksheet.Range(Me.RefEdit1.Address)
        Dim dataRange As Range = dataWorksheet.Range(Me.RefEdit2.Address)

        'Each input must be a single continuous range.
        If groupRange.Areas.Count <> 1 OrElse dataRange.Areas.Count <> 1 Then
            strErr = "Each grouping/continuous input must be one continuous column range."
            Return Nothing
        End If

        'The categorical and continuous observations are paired row-by-row.
        If groupRange.Row <> dataRange.Row OrElse groupRange.Rows.Count <> dataRange.Rows.Count Then
            strErr = "Categorical and continuous variable ranges must start on the same row and contain the same number of rows."
            Return Nothing
        End If

        Dim imported = New DataObj
        Dim refId As String = prepareRef2D(Me.RefEdit1.Address)
        Dim refData As String = prepareRef2D(Me.RefEdit2.Address)
        Dim refFinal As String = refId & ", " & Replace(refData, WorksheetNameFromRefAdress(refData, True) & "!", String.Empty)

        'The first imported column is categorical and may contain text;
        'the second must be numeric.
        ExcelDnaDataImporter.ImportInto(imported, refFinal, True, 0)

        If imported.bZeroValid OrElse imported.FinalData Is Nothing OrElse imported.nRows = 0 Then
            strErr = "No valid paired categorical/continuous observations were found."
            Return Nothing
        End If

        If imported.nCols <> 2 Then
            strErr = "This plot requires one grouping column and one continuous data column."
            Return Nothing
        End If

        Dim out As New GroupedContinuousInputData With {
            .ContinuousName = imported.varNames(1),
            .GroupName = imported.varNames(0)
        }

        ReDim out.Values(imported.nRows - 1)
        ReDim out.Groups(imported.nRows - 1)

        For i As Integer = 0 To imported.nRows - 1
            out.Groups(i) = imported.FinalData(i, 0)
            out.Values(i) = CDbl(imported.FinalData(i, 1))
        Next

        Return out
    End Function

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim errText As String = String.Empty
            Dim MWdata As TwoGroupsData, data As MultiGroupsUnpairedData

            'Validate Inputs
            If Me.Tag = HelpTopic.UnpairedTTest Then
                If Me.ValidateUnpairedTtestOptionInputs() Then Exit Sub
            Else
                If Me.checkInputs() Then Exit Sub
            End If

            If Me.Tag = HelpTopic.CategoricalHistogram OrElse Me.Tag = HelpTopic.ViolinPlot Then
                Dim groupedData As GroupedContinuousInputData = Me.getGroupedContinuousData(errText)
                If errText <> String.Empty Then
                    MsgBox(errText, vbExclamation)
                    Exit Sub
                End If

                If Me.Tag = HelpTopic.CategoricalHistogram Then
                    Me.RunCategoricalHistogram(groupedData)
                Else
                    Me.RunViolinPlot(groupedData)
                End If

            ElseIf Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Then

                'Get Data
                MWdata = Me.getData2Groups(errText)
                If errText <> String.Empty Then
                    MsgBox(errText, vbExclamation)
                    Exit Sub
                End If
                data = New MultiGroupsUnpairedData
                data.X = {MWdata.X1, MWdata.X2}
                data.varNames = {MWdata.name1, MWdata.name2}

                If Me.Tag = HelpTopic.MannWhitneyTest Then
                    Me.RunMannWhitney(data)
                ElseIf Me.Tag = HelpTopic.UnpairedTTest Then
                    Me.RunTtest(data)
                ElseIf Me.Tag = HelpTopic.ROCCurve Then
                    Me.RunROC(data)
                End If
            Else
                'Get Data
                data = Me.getDataMultipleGroups(errText)
                If errText <> String.Empty Then
                    MsgBox(errText, vbExclamation)
                    Exit Sub
                End If

                'Run analysis
                If Me.Tag = HelpTopic.KruskalWallisTest Then
                    Me.RunKruskallWalis(data)
                ElseIf Me.Tag = HelpTopic.BoxAndWhiskers Then
                    Me.RunBoxAndWhiskers(data)
                ElseIf Me.Tag = HelpTopic.OneWayANOVA Then
                    Me.RunOneWayANOVA(data)
                ElseIf Me.Tag = HelpTopic.DescriptiveStatistics Then
                    Me.RunDescStat(data)
                ElseIf Me.Tag = HelpTopic.NormalityTests Then
                    Me.RunNormality(data)
                ElseIf Me.Tag = HelpTopic.Histogram Then
                    Me.RunHistogram(data)
                ElseIf Me.Tag = HelpTopic.NormalPlot Then
                    Me.RunNormalPlot(data)
                ElseIf Me.Tag = HelpTopic.HomogeneityOfVariance Then
                    Me.RunHomogeneityVar(data)
                ElseIf Me.Tag = HelpTopic.Symmetry Then
                    Me.RunSymmetry(data)
                ElseIf Me.Tag = HelpTopic.UnivariateOutliers Then
                    Me.RunOutliers(data)
                ElseIf Me.Tag = HelpTopic.CDFplot Then
                    Me.RunCDFPlot(data)
                End If
            End If
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex, False, True)
        End Try
    End Sub

    Private Sub RunOutliers(data As MultiGroupsUnpairedData)
        Dim res = New List(Of ResultTable), out(,) As Object = Nothing
        Dim box As graphics.BoxPlot = Nothing
        Dim outlierTable = New ResultTable

        Dim k = data.X.Length
        If Me.optGrubbs.Checked Then
            outlierTable.AddHeaderLeftRow({"Sample", "Alpha", "Critical Test Statistic", "Sample Test Statistic", "Result"})
            ReDim out(3, k - 1)
        ElseIf Me.optRosner.Checked Then
            outlierTable.AddHeaderLeftRow({"Sample", "Alpha", "Number of Outliers"})
            ReDim out(12, k - 1) 'rosner can detect up to 10 outliers + two rows for "number of outliers information" and additional text
        End If

        Dim nMaxOutliers As Integer = 0
        For i = 0 To k - 1
            If Me.optGrubbs.Checked Then
                Dim t = assumptions.Grubbs(data.X(i), Me.spinBtnAlphaOutliers.Value)
                out(0, i) = Me.spinBtnAlphaOutliers.Value
                out(1, i) = t.TestStatistics1
                out(2, i) = t.TestStatistics2
                out(3, i) = t.strSpecialInformation

            ElseIf Me.optRosner.Checked Then

                Dim t = assumptions.Rosner(data.X(i), Me.spinBtnAlphaOutliers.Value)

                out(0, i) = Me.spinBtnAlphaOutliers.Value
                out(2, i) = "List of Outliers:"
                If t Is Nothing Then
                    out(1, i) = 0
                Else
                    out(1, i) = t.Length
                    For j = 0 To t.Length - 1
                        out(j + 3, i) = t(j)
                    Next
                End If
                nMaxOutliers = Math.Max(nMaxOutliers, out(1, i))
            End If
        Next

        outlierTable.SetBody(out)
        outlierTable.AddHeaderTopRow(data.varNames)
        outlierTable.bLeftHeaderAdjustUp = True
        res.Add(outlierTable)

        'descriptive statistics
        If Me.ckDescriptive_Outliers.Checked Then res.Add(Me.ComputeDescriptiveStats(data))


        'box plot if requested
        If Me.ckBoxPlot_Outliers.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot_Outliers.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Sub RunSymmetry(data As MultiGroupsUnpairedData)
        Dim res = New List(Of ResultTable), out(,) As Object
        Dim plots = New List(Of graphics.SymetryPlot)
        Dim asymTable = New ResultTable

        Dim strTest As String = If(Me.optMGG.Checked, "Miao-Gel-Gastwirth", "Cabilio-Masaro")
        Dim k = data.X.Length
        ReDim out(1, k - 1)
        For i = 0 To k - 1
            Dim t = assumptions.SymmetryTest(data.X(i), strTest)
            out(0, i) = t.TestStatistics1
            out(1, i) = t.Pvalue
            If Me.ckSymmetryPlot.Checked Then plots.Add(New graphics.SymetryPlot(data.X(i)))
        Next
        asymTable.SetBody(out)
        For i = 0 To k - 1
            asymTable.AddPvalueCellToFormat(2, i + 1)
        Next
        Dim tmp(UBound(data.varNames)) As String
        tmp(0) = strTest
        asymTable.AddHeaderTopRow(tmp)
        asymTable.AddHeaderTopRow(data.varNames)
        asymTable.AddHeaderLeftRow({"Asymetry test", "Test Statistic", "Two-sided P-value"})
        res.Add(asymTable)

        'descriptive statistics
        If Me.ckDescriptive_Symmetry.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        'dysplay symmetry plot
        If Me.ckSymmetryPlot.Checked Then
            For i = 0 To k - 1
                plots(i).AsymmetryPlot(data.varNames(i), (i + 1) * 280 - 200, 100)
            Next
        End If
    End Sub

    Private Sub RunHomogeneityVar(data As MultiGroupsUnpairedData)
        Dim res = New List(Of ResultTable)
        Dim box As graphics.BoxPlot = Nothing

        If Me.ckBartlett.Checked Or Me.ckFlignerKilleen.Checked Or
            Me.ckLevene.Checked Or Me.ckSquaredRanks.Checked Then res.Add(ComputeVarianceHomogeneity(data))

        'descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Sub RunNormalPlot(data As MultiGroupsUnpairedData)
        Dim res = New List(Of ResultTable)
        Dim normList = New List(Of graphics.NormalPlot)
        Dim rankMethod As String = String.Empty, lineMethod As String = String.Empty

        If Me.optBlom.Checked Then
            rankMethod = "Blom"
        ElseIf Me.optRankit.Checked Then
            rankMethod = "Rankit"
        ElseIf Me.optVanDerWaerden.Checked Then
            rankMethod = "Van Der Waerden"
        End If

        If Me.optSPSS.Checked Then
            lineMethod = "SPSS"
        ElseIf Me.optOLS.Checked Then
            lineMethod = "OLS"
        ElseIf Me.optR.Checked Then
            lineMethod = "R"
        End If

        For i = 0 To data.X.Length - 1
            Dim np = New graphics.NormalPlot(data.X(i), data.varNames(i))
            np.compute(rankMethod)
            np.computeRefLIne(lineMethod)
            normList.Add(np)
        Next

        Dim WriteRes = GetResultWriter() 'pass just table from the main test output

        'descriptive statistics
        If Me.ckDescriptive_NormalPlot.Checked Then
            res.Add(Me.ComputeDescriptiveStats(data))

            'Dump outputs
            Dim rr = New ProcessListofResultTables(res)
            Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
            Dim totcols As Integer = rr.TotCols
            If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If
            If Me.ckDescriptive_NormalPlot.Checked Then rr.writeToSheet(WriteRes, True)
        End If

        For i = 0 To normList.Count - 1
            normList(i).addChart(WriteRes.ws)
        Next
    End Sub

    Private Sub RunHistogram(data As MultiGroupsUnpairedData)
        Dim HisData = New List(Of Object(,))
        Dim res = New List(Of ResultTable)
        Dim strBiningTyp As String = String.Empty
        Dim histList = New List(Of graphics.Histogram)

        If Me.optSturges.Checked Then
            strBiningTyp = "(Sturges)"
        ElseIf Me.optDoane.Checked Then
            strBiningTyp = "(Doane)"
        ElseIf Me.optScott.Checked Then
            strBiningTyp = "(Scott)"
        ElseIf Me.optFreedmanDiaconis.Checked Then
            strBiningTyp = "(Freedman-Diaconis)"
        End If

        For i = 0 To data.X.Length - 1
            Dim hist = New graphics.Histogram(data.X(i))
            Dim d = hist.compute(Me.ckOverlay.Checked, strBiningTyp, Me.ckBoxPlot_Histogram.Checked)
            HisData.Add(d)
            histList.Add(hist)
        Next

        'descriptive statistics
        If Me.ckDescriptive_Histogram.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        If Me.ckDescriptive_Histogram.Checked Then
            Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
            Dim totcols As Integer = rr.TotCols
            If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
                If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                    Exit Sub
                End If
            End If
        End If

        For i = 0 To histList.Count - 1
            Dim row As Integer = WriteRes.RowID
            Dim col As Integer = WriteRes.ColID

            histList(i).SetWs = WriteRes.ws
            Dim strHistogramTitle As String = $"Histogram {strBiningTyp} - {data.varNames(i)}"

            'Place the chart to the right of the descriptive table (when requested).
            'For multiple variables, stack charts vertically instead of overlapping them.
            Dim chartCol As Integer = col
            If Me.ckDescriptive_Histogram.Checked Then chartCol += rr.TotCols + 1
            Dim chartRow As Integer = row + 1 + i * If(Me.ckBoxPlot_Histogram.Checked, 24, 21)

            histList(i).addChart(WriteRes.ws, chartRow, chartCol, strHistogramTitle, Me.ckBoxPlot_Histogram.Checked)
        Next i

        If Me.ckDescriptive_Histogram.Checked Then rr.writeToSheet(WriteRes, True)
    End Sub

    Private Sub RunCategoricalHistogram(data As GroupedContinuousInputData)
        If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))

        Dim options As New CategoricalHistogramOptions

        If Me.optCatHistStackedBar.Checked Then
            options.PlotType = CategoricalHistogramPlotType.StackedBar
        ElseIf Me.optCatHistDifferentSampleSizes.Checked Then
            options.PlotType = CategoricalHistogramPlotType.DifferentSampleSizes
        ElseIf Me.optCatHistComparativeHorizontal.Checked Then
            options.PlotType = CategoricalHistogramPlotType.Comparative
            options.ComparativeOrientation = CategoricalHistogramComparativeOrientation.Horizontal
        ElseIf Me.optCatHistComparativeVertical.Checked Then
            options.PlotType = CategoricalHistogramPlotType.Comparative
            options.ComparativeOrientation = CategoricalHistogramComparativeOrientation.Vertical
        Else
            options.PlotType = CategoricalHistogramPlotType.BarsWithLegend
        End If

        If Me.optCatHistDoan.Checked Then
            options.BinningRule = CategoricalHistogramBinningRule.Doane
        ElseIf Me.optCatHistScott.Checked Then
            options.BinningRule = CategoricalHistogramBinningRule.Scott
        ElseIf Me.optCatHistFreedmanDiaconis.Checked Then
            options.BinningRule = CategoricalHistogramBinningRule.FreedmanDiaconis
        Else
            options.BinningRule = CategoricalHistogramBinningRule.Sturges
        End If

        Dim result As CategoricalHistogramResult = CategoricalHistogram.Compute(data.Values, data.Groups, options)

        Dim isComparative As Boolean = (options.PlotType = CategoricalHistogramPlotType.Comparative)
        Dim appearance As New CategoricalHistogramAppearance With {
            .ChartTitle = If(isComparative, "Comparative histogram - ", "Categorical histogram - ") & data.ContinuousName & " by " & data.GroupName,
            .XAxisTitle = data.ContinuousName,
            .ShowLegend = True,
            .GapWidth = CInt(Me.nudCatHistGapWidth.Value),
            .SeriesOverlap = If(isComparative, 100, CInt(Me.nudCatHistSeriesOverlap.Value)),
            .SeriesColors = Me.GetGroupedPlotPalette(Me.cmbCatHistPalette.SelectedIndex)
        }

        Dim WriteRes = GetResultWriter()
        Dim chartAnchor As Range = DirectCast(WriteRes.ws.Cells(WriteRes.RowID, WriteRes.ColID), Range)

        CategoricalHistogramExcel.AddChart(WriteRes.ws,
                                           result,
                                           appearance,
                                           CDbl(chartAnchor.Left),
                                           CDbl(chartAnchor.Top))
    End Sub

    Private Sub RunViolinPlot(data As GroupedContinuousInputData)
        If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))

        Dim options As New ViolinPlotOptions With {
            .GridPoints = CInt(Me.nudViolinDensityPoints.Value),
            .Trim = Me.cmdViolinTrimDensity.Checked
        }

        Select Case Me.cmbViolinBandwidth.SelectedIndex
            Case 1
                options.BandwidthRule = ViolinBandwidthRule.Scott
                options.BandwidthAdjustment = CDbl(Me.nudViolinBandwidthAdjustment.Value)
            Case 2
                options.BandwidthRule = ViolinBandwidthRule.Manual
                options.ManualBandwidth = CDbl(Me.nudViolinBandwidthAdjustment.Value)
            Case Else
                options.BandwidthRule = ViolinBandwidthRule.Silverman
                options.BandwidthAdjustment = CDbl(Me.nudViolinBandwidthAdjustment.Value)
        End Select

        Select Case Me.cmdViolinScaling.SelectedIndex
            Case 1
                options.ScaleMode = ViolinScaleMode.EqualArea
            Case 2
                options.ScaleMode = ViolinScaleMode.Count
            Case Else
                options.ScaleMode = ViolinScaleMode.EqualMaximumWidth
        End Select

        Dim result As ViolinPlotResult = ViolinPlot.Compute(data.Values, data.Groups, options)

        Dim appearance As New ViolinPlotAppearance With {
            .ChartTitle = "Violin plot - " & data.ContinuousName & " by " & data.GroupName,
            .XAxisTitle = data.GroupName,
            .YAxisTitle = data.ContinuousName,
            .ShowHorizontalGridlines = Me.cbViolinHorizontalGridlines.Checked,
            .SeriesColors = Me.GetGroupedPlotPalette(Me.cmbViolinPalette.SelectedIndex),
            .FillTransparency = CSng(CDbl(Me.nudViolinFillTransparency.Value) / 100.0R),
            .ShowOutline = Me.cbViolinOutline.Checked,
            .ShowInnerBox = Me.cbViolinInnerBoxPlot.Checked,
            .ShowMedian = Me.cmdViolinMedian.Checked,
            .ShowMean = Me.cmdViolinMean.Checked,
            .ShowIndividualObservations = Me.cmdViolinIndividualObs.Checked
        }

        Dim WriteRes = GetResultWriter()
        Dim chartAnchor As Range = DirectCast(WriteRes.ws.Cells(WriteRes.RowID, WriteRes.ColID), Range)

        ViolinPlotExcel.AddChart(WriteRes.ws,
                                 result,
                                 appearance,
                                 CDbl(chartAnchor.Left),
                                 CDbl(chartAnchor.Top),
                                 CDbl(Me.nudViolinChartWidth.Value),
                                 CDbl(Me.nudViolinChartHeight.Value))
    End Sub

    Private Sub RunCDFPlot(data As MultiGroupsUnpairedData)
        If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
        If data.X Is Nothing OrElse data.X.Length = 0 Then
            Throw New ArgumentException("No data are available for the CDF/ECDF plot.", NameOf(data))
        End If

        Dim options As New CumulativeDistributionPlotOptions

        Select Case Me.cmbCDFPlotType.SelectedIndex
            Case 0
                options.Mode = CumulativeDistributionPlotMode.EmpiricalOnly
            Case 2
                options.Mode = CumulativeDistributionPlotMode.FittedDistributionOnly
            Case Else
                options.Mode = CumulativeDistributionPlotMode.EmpiricalWithFittedDistribution
        End Select

        If Me.cmbCDFDistribution.SelectedIndex < 0 OrElse Me.cmbCDFDistribution.SelectedIndex > 13 Then
            Throw New ArgumentException("Select a fitted distribution.")
        End If
        options.Distribution = CType(Me.cmbCDFDistribution.SelectedIndex, CumulativeDistributionKind)

        options.YScale = If(Me.cmbCDFYScale.SelectedIndex = 0,
                            CumulativeDistributionYScale.Probability,
                            CumulativeDistributionYScale.Percent)

        options.EmpiricalMethod = If(Me.cmbCDFEmpiricalMethod.SelectedIndex = 1,
                                     CumulativeDistributionEmpiricalMethod.MinitabMedianRank,
                                     CumulativeDistributionEmpiricalMethod.ExactEcdf)

        If Me.ckCDFPercentileLines.Checked Then
            options.Percentiles = Me.ParseCDFPercentiles()
        Else
            options.Percentiles = Array.Empty(Of Double)()
        End If

        Dim results(data.X.Length - 1) As CumulativeDistributionPlotResult
        For i As Integer = 0 To data.X.Length - 1
            If data.X(i) Is Nothing OrElse data.X(i).Length = 0 Then
                Throw New ArgumentException("One of the selected samples contains no valid numeric observations.")
            End If

            Dim seriesName As String = "Sample " & (i + 1).ToString()
            If data.varNames IsNot Nothing AndAlso i < data.varNames.Length AndAlso
               Not String.IsNullOrWhiteSpace(data.varNames(i)) Then
                seriesName = data.varNames(i)
            End If

            results(i) = CumulativeDistributionPlot.Compute(data.X(i), seriesName, options)
        Next

        Dim resultSet As New CumulativeDistributionPlotSetResult(results)
        Dim palette() As Integer = Me.GetGroupedPlotPalette(Me.cmbCDFPalette.SelectedIndex)
        Dim appearance As New CumulativeDistributionPlotAppearance With {
            .ShowLegend = Me.ckCDFShowLegend.Checked,
            .LegendMode = If(Me.cmbCDFLegendMode.SelectedIndex = 1,
                             CumulativeDistributionLegendMode.AllCurves,
                             CumulativeDistributionLegendMode.GroupsOnly),
            .PaletteColors = palette,
            .SingleSeriesColor = palette(0),
            .ShowHorizontalGridlines = Me.ckCDFHorizontalGridlines.Checked,
            .ShowVerticalGridlines = Me.ckCDFVerticalGridlines.Checked,
            .ExtendEmpiricalToAxisBounds = Me.ckCDFExtendEmpirical.Checked,
            .ShowPercentileReferenceLines = Me.ckCDFPercentileLines.Checked,
            .ShowPercentileLabels = Me.ckCDFPercentileLines.Checked AndAlso Me.ckCDFPercentileLabels.Checked
        }

        Dim WriteRes = GetResultWriter()
        Dim chartAnchor As Range = DirectCast(WriteRes.ws.Cells(WriteRes.RowID, WriteRes.ColID), Range)
        Dim chartLeft As Double = CDbl(chartAnchor.Left)
        Dim chartTop As Double = CDbl(chartAnchor.Top)
        Dim chartWidth As Double = CDbl(Me.nudCDFChartWidth.Value)
        Dim chartHeight As Double = CDbl(Me.nudCDFChartHeight.Value)

        If Me.optCDFSeparate.Checked Then
            CumulativeDistributionPlotExcel.AddSeparateCharts(WriteRes.ws,
                                                              resultSet,
                                                              appearance,
                                                              chartLeft,
                                                              chartTop,
                                                              chartWidth,
                                                              chartHeight)
        Else
            CumulativeDistributionPlotExcel.AddOverlayChart(WriteRes.ws,
                                                             resultSet,
                                                             appearance,
                                                             chartLeft,
                                                             chartTop,
                                                             chartWidth,
                                                             chartHeight)
        End If
    End Sub

    Private Function ParseCDFPercentiles() As Double()
        Dim textValue As String = If(Me.txtCDFPercentiles.Text, String.Empty).Trim()
        If textValue = String.Empty Then
            Throw New ArgumentException("Enter at least one percentile between 0 and 100.")
        End If

        textValue = textValue.Replace(ControlChars.Cr, ";").Replace(ControlChars.Lf, ";").Replace(ControlChars.Tab, ";")

        Dim decimalSeparator As String = Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
        If decimalSeparator <> "," Then textValue = textValue.Replace(",", ";")

        Dim tokens() As String = textValue.Split(New Char() {";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim values As New List(Of Double)()

        For Each token As String In tokens
            Dim trimmed As String = token.Trim()
            If trimmed = String.Empty Then Continue For

            Dim percentile As Double
            Dim parsed As Boolean = Double.TryParse(trimmed,
                                                    Globalization.NumberStyles.Float,
                                                    Globalization.CultureInfo.CurrentCulture,
                                                    percentile)
            If Not parsed Then
                parsed = Double.TryParse(trimmed,
                                         Globalization.NumberStyles.Float,
                                         Globalization.CultureInfo.InvariantCulture,
                                         percentile)
            End If

            If Not parsed Then
                Throw New ArgumentException("Invalid percentile value '" & trimmed & "'. Use semicolons to separate values, for example 25; 50; 75.")
            End If
            If Double.IsNaN(percentile) OrElse Double.IsInfinity(percentile) OrElse percentile <= 0.0R OrElse percentile >= 100.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(txtCDFPercentiles),
                                                      "Percentiles must be greater than 0 and less than 100.")
            End If

            If Not values.Any(Function(value As Double) Math.Abs(value - percentile) <= 0.000000000001R) Then
                values.Add(percentile)
            End If
        Next

        If values.Count = 0 Then
            Throw New ArgumentException("Enter at least one percentile between 0 and 100.")
        End If

        values.Sort()
        Return values.ToArray()
    End Function

    Private Function GetGroupedPlotPalette(selectedIndex As Integer) As Integer()
        Select Case selectedIndex
            Case 1 'Okabe-Ito
                Return {&H9FE6, &HE9B456, &H739E00, &H42E4F0, &HB27200, &H5ED5, &HA779CC, &H0}
            Case 2 'ColorBrewer Set1
                Return {&H1C1AE4, &HB87E37, &H4AAF4D, &HA34E98, &H7FFF, &H33FFFF, &H2856A6, &HBF81F7, &H999999}
            Case 3 'Grayscale
                Return {&H404040, &H606060, &H808080, &HA0A0A0, &HC0C0C0, &HE0E0E0}
            Case Else 'Tableau 10
                Return {&HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794, &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17}
        End Select
    End Function

    Private Sub RunNormality(data As MultiGroupsUnpairedData)
        Dim results(,) As Object, sw_r(,) As Object, da_r(,) As Object, ad_r(,) As Object
        Dim box As graphics.BoxPlot = Nothing
        Dim res = New List(Of ResultTable)
        Dim strErr As String = Nothing
        Dim rTable = New ResultTable

        results = {{"Shapiro-Wilk test"}, {"Test statistics"}, {"Two-sided P-value"},
                   {"D'Agostino-Pearson K2 test"}, {"Test statistics"}, {"Two-sided P-value"},
                   {"Anderson Darling test"}, {"Test statistics"}, {"Two-sided P-value"}}
        For i = 0 To data.X.Length - 1
            If data.X(i).Length > 3 And data.X(i).Length < 5000 Then
                Dim sw_res = assumptions.ShapiroWilk(data.X(i), strErr)
                sw_r = {{""}, {sw_res.TestStatistics1}, {sw_res.Pvalue}}
            Else
                AppInfrastructure.CoreServices.Log("N not in range between 3 and 5000")
                sw_r = {{"NA n<4 or n>5000"}, {"NA n<4 or n>5000"}, {"NA n<4 or n>5000"}}
            End If


            If data.X(i).Length >= 9 Then
                Dim da_res = assumptions.DAgostino(data.X(i), strErr)
                da_r = {{""}, {da_res.TestStatistics1}, {da_res.Pvalue}}
            Else
                AppInfrastructure.CoreServices.Log("N not >= 9")
                da_r = {{"NA n<9"}, {"NA n<9"}, {"NA n<9"}}
            End If

            If data.X(i).Length > 1 Then
                Dim ad_res = assumptions.AndersonDarlingTEST(data.X(i))
                ad_r = {{""}, {ad_res.TestStatistics1}, {ad_res.Pvalue}}
            Else
                AppInfrastructure.CoreServices.Log("N not > 1")
                ad_r = {{"NA"}, {"NA"}, {"NA"}}
            End If

            results = Matrix.VerticalStackArrays(results, Matrix.HorizontalStackArrays(Matrix.HorizontalStackArrays(sw_r, da_r), ad_r))
        Next
        rTable.SetBody(results)
        For i = 0 To data.X.Length - 1
            rTable.AddPvalueCellToFormat(3, i + 2)
            rTable.AddPvalueCellToFormat(6, i + 2)
            rTable.AddPvalueCellToFormat(9, i + 2)
        Next
        rTable.AddHeaderTopRow(Matrix.ConcatArrays({"Normality Tests"}, data.varNames))
        res.Add(rTable)

        'descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Sub RunDescStat(data As MultiGroupsUnpairedData)
        Dim box As graphics.BoxPlot = Nothing
        Dim res = New List(Of ResultTable)
        Dim stat = New List(Of String)

        If Me.ckN.Checked Then stat.Add("n")
        If Me.ckMean.Checked Then stat.Add("mean")
        If Me.ckMedian.Checked Then stat.Add("median")
        If Me.ckCV.Checked Then stat.Add("cv")
        If Me.ckVariance.Checked Then stat.Add("variance")

        If Me.ckSD.Checked Then stat.Add("sd")
        If Me.ckSEM.Checked Then stat.Add("sem")
        If Me.ckSkewness.Checked Then stat.Add("skewness")
        If Me.ckKurtosis.Checked Then stat.Add("kurtosis")
        If Me.ckQ1.Checked Then stat.Add("q1")

        If Me.ckQ3.Checked Then stat.Add("q3")
        If Me.ckIQR.Checked Then stat.Add("iqr")
        If Me.ckMin.Checked Then stat.Add("minimum")
        If Me.ckMax.Checked Then stat.Add("maximum")
        If Me.ckRange.Checked Then stat.Add("range")
        If Me.ckShapiroWilk.Checked Then
            stat.Add("swstat")
            stat.Add("swpvalue")
        End If

        res.Add(Me.ComputeDescriptiveStats(data, stat))

        'box plot if requested
        If Me.ckBoxPlot_Descriptive.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot_Descriptive.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Sub RunROC(data As MultiGroupsUnpairedData)
        Dim rroc = New graphics.ROC(data.X, data.varNames)
        rroc.compute(Me.spinBtnAlpha.Value)
        Dim res = rroc.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)
        rroc.addROCplot(WriteRes.ws)
    End Sub

    Private Sub RunTtest(data As MultiGroupsUnpairedData)
        Dim box As graphics.BoxPlot = Nothing
        Dim res As New List(Of ResultTable)

        If Me.optHypothesisSuperiority_UTT.Checked Then
            Dim alphaValue As Double = CDbl(Me.spinBtnAlpha_UTT.Value)
            Dim ttest = New parametric.UnpairedTtest(data.X, data.varNames)
            ttest.compute(alphaValue)
            Dim core As List(Of ResultTable) = ttest.wrapResults()

            If Me.optVarianceEqual_UTT.Checked Then
                res.Add(core(0))
            Else
                res.Add(core(1))
            End If

        ElseIf Me.optHypothesisNonInferiority_UTT.Checked Then
            Dim ni = equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansNonInferiority(
            controlSample:=data.X(0),
            experimentalSample:=data.X(1),
            nonInferiorityMargin:=Me.GetUnpairedTtestMargin(),
            alphaOneSided:=CDbl(Me.spinBtnAlpha_UTT.Value),
            assumeEqualVariances:=Me.optVarianceEqual_UTT.Checked)

            res.Add(Me.BuildUnpairedTtestNonInferiorityTable(ni, data.varNames))

        ElseIf Me.optHypothesisEquivalence_UTT.Checked Then
            Dim margin As Double = Me.GetUnpairedTtestMargin()
            Dim eqv = equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalence(
            controlSample:=data.X(0),
            experimentalSample:=data.X(1),
            lowerMargin:=-margin,
            upperMargin:=margin,
            alphaOneSided:=CDbl(Me.spinBtnAlpha_UTT.Value),
            assumeEqualVariances:=Me.optVarianceEqual_UTT.Checked)

            res.Add(Me.BuildUnpairedTtestEquivalenceTable(eqv, data.varNames))
        End If

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics_UTT.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'Box plot if requested
        If Me.ckBoxPlot_UTT.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter()
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot_UTT.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Function ComputeVarianceHomogeneity(data As MultiGroupsUnpairedData) As ResultTable
        Dim varResult As TestResult, o(,) As Object = Nothing
        Dim t = New ResultTable

        If Me.ckBartlett.Checked Or Me.ckFlignerKilleen.Checked Or Me.ckLevene.Checked Or Me.ckSquaredRanks.Checked Then
            If Me.ckFlignerKilleen.Checked Then
                varResult = assumptions.FlignerKilleenTEST(data.X)
                o = {{"Fligner-Killeen", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
            End If
            If Me.ckLevene.Checked Then
                varResult = assumptions.LeveneTEST(data.X, True)
                If o Is Nothing Then
                    o = {{"Levene", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
                Else
                    o = Matrix.HorizontalStackArrays(o, {{"Levene", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}})
                End If
            End If
            If Me.ckSquaredRanks.Checked Then
                varResult = assumptions.SquaredRanksTestVARIANCE(data.X)
                If o Is Nothing Then
                    o = {{"Squared Ranks", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
                Else
                    o = Matrix.HorizontalStackArrays(o, {{"Squared Ranks", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}})
                End If
            End If

            If Me.ckBartlett.Checked Then
                varResult = assumptions.BartlettTEST(data.X)
                If o Is Nothing Then
                    o = {{"Bartlett", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
                Else
                    o = Matrix.HorizontalStackArrays(o, {{"Bartlett", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}})
                End If
            End If
            t.AddHeaderTopRow({"Homogeneity of Variances", ""})
            t.SetBody(o)
            For rowIndex = 0 To o.GetLength(0) - 1
                If String.Equals(CStr(o(rowIndex, 0)), "P-value", StringComparison.OrdinalIgnoreCase) Then
                    t.AddPvalueCellToFormat(rowIndex + 1, 2)
                End If
            Next
        End If

                Return t
    End Function

    Private Function ComputeDescriptiveStats(data As MultiGroupsUnpairedData, Optional statsToReturn As List(Of String) = Nothing) As ResultTable
        Dim descTable = New ResultTable
        Dim ds1 = New DescriptiveStat(data.X(0))
        ds1.compute()
        Dim tableBody = ds1.wrapSelf(True, statsToReturn)
        For i = 1 To data.X.Length - 1
            Dim ds2 = New DescriptiveStat(data.X(i))
            ds2.compute()
            tableBody = Matrix.VerticalStackArrays(tableBody, ds2.wrapSelf(False, statsToReturn))
        Next
        descTable.SetBody(tableBody)
        For rowIndex = 0 To tableBody.GetLength(0) - 1
            If String.Equals(CStr(tableBody(rowIndex, 0)), "Two-sided p-value", StringComparison.OrdinalIgnoreCase) Then
                For columnIndex = 1 To tableBody.GetLength(1) - 1
                    descTable.AddPvalueCellToFormat(rowIndex + 1, columnIndex + 1)
                Next
            End If
        Next
        descTable.AddTitle("Descriptive Statistics")
        descTable.AddHeaderTopRow(Matrix.ConcatArrays({""}, data.varNames))

        Return descTable
    End Function

    Private Sub RunOneWayANOVA(data As MultiGroupsUnpairedData)
        'Stage 3.02: the normal one-way ANOVA path now crosses the same
        'BESHStat.Excel.Common application boundary used by the Office.js host.
        'Welch output still uses the legacy combined-table path for this prototype
        'because Core currently embeds Welch columns into the primary ANOVA table.
        If Me.ckWelch.Checked Then
            RunOneWayANOVALegacy(data)
            Exit Sub
        End If

        RunOneWayANOVAThroughSharedLayer(data)
    End Sub

    Private Sub RunOneWayANOVAThroughSharedLayer(data As MultiGroupsUnpairedData)
        Dim alpha As Double = AppGlobals.DefaultAlpha
        Dim box As graphics.BoxPlot = Nothing

        Dim grouped As Global.BESHStatNG.ExcelCommon.GroupedNumericData =
            ExcelDnaSharedApplicationAdapter.ToGroupedNumericData(data)

        Dim sharedResult As Global.BESHStatNG.ExcelCommon.OneWayAnovaApplicationResult =
            Global.BESHStatNG.ExcelCommon.OneWayAnovaApplicationService.Run(grouped)

        'Multiple-comparison procedures still need their Core model instance because
        'the Windows host also consumes the plot payloads. Their additional tables are
        'appended after the shared-layer primary ANOVA table.
        Dim anova As parametric.OneWayANOVA = Nothing
        Dim res As New List(Of ResultTable)

        If Me.ckLSD.Checked OrElse Me.ckBonferroni.Checked OrElse
           Me.ckTukey.Checked OrElse Me.ckGamesHowell.Checked Then

            anova = New parametric.OneWayANOVA(data.X, data.varNames)
            anova.compute()

            If Me.ckLSD.Checked Then anova.FisherLSD(False, alpha)
            If Me.ckBonferroni.Checked Then anova.FisherLSD(True, alpha)
            If Me.ckTukey.Checked Then anova.TukeyKramer(alpha)
            If Me.ckGamesHowell.Checked Then anova.GamesHowell(alpha)

            Dim mcpResults As List(Of ResultTable) = anova.wrapResults()
            For i As Integer = 1 To mcpResults.Count - 1
                res.Add(mcpResults(i))
            Next
        End If

        'Homogeneity of variances.
        If Me.ckBartlett.Checked Or Me.ckFlignerKilleen.Checked Or
           Me.ckLevene.Checked Or Me.ckSquaredRanks.Checked Then
            res.Add(Me.ComputeVarianceHomogeneity(data))
        End If

        'Descriptive statistics.
        If Me.ckDescriptiveStatistics.Checked Then
            res.Add(Me.ComputeDescriptiveStats(data))
        End If

        'Box plot result table if requested.
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Keep the established Windows output-selection behavior. The shared request
        'records the same destination/pointer, while ExcelDnaResultWriter performs the
        'actual COM write using its already configured workbook and worksheet.
        Dim WriteRes As ExcelDnaResultWriter = GetResultWriter()

        Dim destination As Global.BESHStatNG.ExcelCommon.SpreadsheetOutputDestination =
            Global.BESHStatNG.ExcelCommon.SpreadsheetOutputDestination.CurrentWorksheet
        If Me.optWorkbook.Checked Then
            destination = Global.BESHStatNG.ExcelCommon.SpreadsheetOutputDestination.NewWorkbook
        ElseIf Me.optWorksheet.Checked Then
            destination = Global.BESHStatNG.ExcelCommon.SpreadsheetOutputDestination.NewWorksheet
        End If

        Dim outputTarget As New Global.BESHStatNG.ExcelCommon.SpreadsheetOutputTarget With {
            .destination = destination,
            .WorksheetName = CStr(WriteRes.ws.Name),
            .StartRow = WriteRes.RowID,
            .StartColumn = WriteRes.ColID
        }

        Dim sharedRequest As Global.BESHStatNG.ExcelCommon.ResultTableWriteRequest =
            Global.BESHStatNG.ExcelCommon.ResultTableWriteRequestFactory.Create(
                sharedResult.Table,
                AppGlobals.PValuePresentation,
                alpha,
                outputTarget)

        'Preserve historical Windows behavior: existing GUI output does not auto-fit.
        sharedRequest.AutoFit = False

        Dim mainRows As Integer = sharedRequest.Values.Count
        Dim mainCols As Integer = If(mainRows = 0, 0, sharedRequest.Values(0).Count)

        Dim rr As ProcessListofResultTables = Nothing
        Dim extraRows As Integer = 0
        Dim extraCols As Integer = 0
        If res.Count > 0 Then
            rr = New ProcessListofResultTables(res)
            extraRows = rr.TotRows + res.Count - 1
            extraCols = rr.TotCols
        End If

        Dim totrows As Integer = mainRows
        If res.Count > 0 Then totrows += 1 + extraRows
        Dim totcols As Integer = Math.Max(mainCols, extraCols)

        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        WriteRes.WriteSharedRequest(sharedRequest)

        If rr IsNot Nothing Then
            WriteRes.shiftRowPointer()
            rr.writeToSheet(WriteRes, True)
        End If

        Global.BESHStatNG.AppInfrastructure.CoreServices.Logger.Info(
            "Windows one-way ANOVA primary table executed through BESHStat.Excel.Common.")

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If

        Dim plotData As New List(Of parametric.MultipleComparisonPlotData)
        If anova IsNot Nothing Then
            If Me.ckLSD.Checked AndAlso anova.FisherLSDPlotData IsNot Nothing Then plotData.Add(anova.FisherLSDPlotData)
            If Me.ckBonferroni.Checked AndAlso anova.BonferroniPlotData IsNot Nothing Then plotData.Add(anova.BonferroniPlotData)
            If Me.ckTukey.Checked AndAlso anova.TukeyKramerPlotData IsNot Nothing Then plotData.Add(anova.TukeyKramerPlotData)
            If Me.ckGamesHowell.Checked AndAlso anova.GamesHowellPlotData IsNot Nothing Then plotData.Add(anova.GamesHowellPlotData)
        End If

        If plotData.Count > 0 Then
            Dim chartLeft As Double = CDbl(WriteRes.ws.Cells(WriteRes.RowID, WriteRes.ColID + totcols + 2).Left)
            Dim chartTop As Double = CDbl(WriteRes.ws.Cells(WriteRes.RowID, WriteRes.ColID).Top)
            For Each pd As parametric.MultipleComparisonPlotData In plotData
                graphics.MultipleComparisonPlotExcel.AddPlot(WriteRes.ws, pd, chartLeft, chartTop)
                chartTop += graphics.MultipleComparisonPlotExcel.SuggestedHeight(pd) + 15.0
            Next
        End If
    End Sub

    Private Sub RunOneWayANOVALegacy(data As MultiGroupsUnpairedData)
        Dim box As graphics.BoxPlot = Nothing
        Dim alpha As Double = AppGlobals.DefaultAlpha
        Dim anova = New parametric.OneWayANOVA(data.X, data.varNames)
        anova.compute()
        If Me.ckWelch.Checked Then anova.WelshANOVA()
        If Me.ckLSD.Checked Then anova.FisherLSD(False, alpha)
        If Me.ckBonferroni.Checked Then anova.FisherLSD(True, alpha)
        If Me.ckTukey.Checked Then anova.TukeyKramer(alpha)
        If Me.ckGamesHowell.Checked Then anova.GamesHowell(alpha)
        Dim res = anova.wrapResults()

        'homogeneity of variances
        If Me.ckBartlett.Checked Or Me.ckFlignerKilleen.Checked Or
        Me.ckLevene.Checked Or Me.ckSquaredRanks.Checked Then res.Add(Me.ComputeVarianceHomogeneity(data))

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If

        'For GUI one-way ANOVA output, add one confidence-interval chart for each
        'selected multiple-comparison procedure. Charts use the same numeric limits
        'and alpha as the result tables and are placed to the right of the output.
        Dim plotData As New List(Of parametric.MultipleComparisonPlotData)
        If Me.ckLSD.Checked AndAlso anova.FisherLSDPlotData IsNot Nothing Then plotData.Add(anova.FisherLSDPlotData)
        If Me.ckBonferroni.Checked AndAlso anova.BonferroniPlotData IsNot Nothing Then plotData.Add(anova.BonferroniPlotData)
        If Me.ckTukey.Checked AndAlso anova.TukeyKramerPlotData IsNot Nothing Then plotData.Add(anova.TukeyKramerPlotData)
        If Me.ckGamesHowell.Checked AndAlso anova.GamesHowellPlotData IsNot Nothing Then plotData.Add(anova.GamesHowellPlotData)

        If plotData.Count > 0 Then
            Dim chartLeft As Double = CDbl(WriteRes.ws.Cells(WriteRes.RowID, WriteRes.ColID + totcols + 2).Left)
            Dim chartTop As Double = CDbl(WriteRes.ws.Cells(WriteRes.RowID, WriteRes.ColID).Top)
            For Each pd As parametric.MultipleComparisonPlotData In plotData
                graphics.MultipleComparisonPlotExcel.AddPlot(WriteRes.ws, pd, chartLeft, chartTop)
                chartTop += graphics.MultipleComparisonPlotExcel.SuggestedHeight(pd) + 15.0
            Next
        End If
    End Sub

    Private Sub RunBoxAndWhiskers(data As MultiGroupsUnpairedData)
        Dim res = New List(Of ResultTable)
        Dim box = New graphics.BoxPlot(data.X, data.varNames)
        box.Calculate()
        box.CalcForPlotting()
        res.Add(box.wrapResults())

        'Compute descriptive statistics. The standalone box-plot page has its own
        'checkbox; the generic option remains reserved for analyses that optionally
        'add a box plot to another statistical procedure.
        If Me.ckDescriptiveStatistics_Box.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        box.SetWs = WriteRes.ws
        Dim appearance As graphics.BoxPlotAppearance = Me.GetStandaloneBoxPlotAppearance()
        If appearance Is Nothing Then
            'The special legacy preset intentionally uses the original AddBoxPlot()
            'path without custom dimensions or display settings, so its output is
            'identical to the historical BESHStat box plot.
            box.AddBoxPlot()
        Else
            box.AddBoxPlot(appearance,
                           CDbl(Me.nudBoxChartWidth.Value),
                           CDbl(Me.nudBoxChartHeight.Value))
        End If
    End Sub

    Private Function GetStandaloneBoxPlotAppearance() As graphics.BoxPlotAppearance
        If Me.cmbBoxPalette.SelectedIndex = BoxPlotLegacyPaletteIndex Then Return Nothing

        Return New graphics.BoxPlotAppearance With {
            .SeriesColors = Me.GetGroupedPlotPalette(Me.cmbBoxPalette.SelectedIndex),
            .FillTransparency = CSng(CDbl(Me.nudBoxFillTransparency.Value) / 100.0R),
            .ShowOutline = Me.cbBoxOutline.Checked,
            .ShowHorizontalGridlines = Me.cbBoxHorizontalGridlines.Checked,
            .ShowMean = Me.cmdBoxMean.Checked,
            .ConnectMeans = Me.cmdBoxMean.Checked AndAlso Me.ckBoxConnectMeans.Checked,
            .ShowIndividualObservations = Me.cmdBoxIndividualObs.Checked
        }
    End Function

    Private Sub RunKruskallWalis(data As MultiGroupsUnpairedData)
        Dim box As graphics.BoxPlot = Nothing

        'Compute test
        Dim KW As New nonparametric.KruskallWalis(data.X, data.varNames)
        KW.compute()
        KW.MCP(AppGlobals.DefaultAlpha)
        Dim res = KW.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStats(data))
        If Me.ckBartlett.Checked Or Me.ckFlignerKilleen.Checked Or
            Me.ckLevene.Checked Or Me.ckSquaredRanks.Checked Then res.Add(Me.ComputeVarianceHomogeneity(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Sub RunMannWhitney(data As MultiGroupsUnpairedData)
        Dim box As graphics.BoxPlot = Nothing
        Dim alphaValue As Double = CDbl(Me.spinBtnAlpha.Value)

        'Compute test
        Dim MW As New nonparametric.MannWhitney(data.X, data.varNames(0), data.varNames(1))
        MW.Compute(New AppInfrastructure.WinFormsProgressReporter(Me.progressBarExactCalc))
        If Me.ckEstimateOfShift.Checked Then MW.ComputeShift(alphaValue)
        Dim res = MW.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.Add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = rr.TotRows + res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = rr.TotCols
        If AreaCheck(WriteRes.RowID, WriteRes.ColID, totrows, totcols, WriteRes.ws) Then
            If MsgBox("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then
                Exit Sub
            End If
        End If

        rr.writeToSheet(WriteRes, True)

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Function GetResultWriter() As ExcelDnaResultWriter
        Dim WriteRes = New ExcelDnaResultWriter, rRange As Range
        If Me.optWorkbook.Checked Then
            WriteRes.wb = AppGlobals.app.Workbooks.Add()
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        ElseIf Me.optWorksheet.Checked Then
            WriteRes.wb = AppGlobals.app.ActiveWorkbook
            WriteRes.wb.Worksheets.Add()
            WriteRes.ws = AppGlobals.app.ActiveWorkbook.ActiveSheet
        Else
            WriteRes.wb = AppGlobals.app.ActiveWorkbook
            WriteRes.ws = WorksheetFromRefAdress(Me.RefEditOutput.Address)
            rRange = WriteRes.ws.Range(Me.RefEditOutput.Address)
            WriteRes.setRowPointer(rRange.Row)
            WriteRes.setColumnPointer(rRange.Column)
        End If

        Return WriteRes
    End Function

    Private Sub optOutputRange_Click(sender As Object, e As System.EventArgs) Handles optOutputRange.Click
        Me.RefEditOutput.Enabled = True
        Me.RefEditOutput.txtAddress.Select()
    End Sub

    Private Sub optWorksheet_Click(sender As Object, e As System.EventArgs) Handles optWorksheet.Click
        Me.RefEditOutput.Enabled = False
    End Sub

    Private Sub optWorkbook_Click(sender As Object, e As System.EventArgs) Handles optWorkbook.Click
        Me.RefEditOutput.Enabled = False
    End Sub

    Private Sub optByColumn_Click(sender As Object, e As System.EventArgs) Handles optByColumn.Click
        If Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Then
            If Me.Tag = HelpTopic.ROCCurve Then
                Me.lblRefedit1.Text = "Group with characteristic present (patients)"
                Me.lblRefedit2.Text = "Group with characteristic absent (controls)"
            ElseIf Me.Tag = HelpTopic.UnpairedTTest Then
                Me.ApplyUnpairedTtestInputLabels()
            Else
                Me.lblRefedit1.Text = "Data: Group 1"
                Me.lblRefedit2.Text = "Data: Group 2"
            End If

            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()
        Else
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.Enabled = False
            Me.lblRefedit1.Enabled = False
            If Me.Tag = HelpTopic.ViolinPlot OrElse Me.Tag = HelpTopic.CategoricalHistogram OrElse Me.Tag = HelpTopic.CDFplot Then
                Me.lblRefedit2.Text = "Data columns:"
            End If
            Me.RefEdit2.txtAddress.Select()
        End If
    End Sub

    Private Sub optByID_Click(sender As Object, e As System.EventArgs) Handles optByID.Click
        If Me.Tag = HelpTopic.MannWhitneyTest Or Me.Tag = HelpTopic.UnpairedTTest Or Me.Tag = HelpTopic.ROCCurve Then
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
        Else
            Me.RefEdit1.Enabled = True
            Me.lblRefedit1.Enabled = True
            If Me.Tag = HelpTopic.ViolinPlot OrElse Me.Tag = HelpTopic.CategoricalHistogram OrElse Me.Tag = HelpTopic.CDFplot Then
                Me.lblRefedit1.Text = "Group ID:"
                Me.lblRefedit2.Text = "Data:"
            End If
        End If
        Me.RefEdit1.txtAddress.Select()
    End Sub

    Private Sub cmbCDFPlotType_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbCDFPlotType.SelectedIndexChanged
        If Me.Tag = HelpTopic.CDFplot Then Me.UpdateCDFOptionState()
    End Sub

    Private Sub ckCDFPercentileLines_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckCDFPercentileLines.CheckedChanged
        If Me.Tag = HelpTopic.CDFplot Then Me.UpdateCDFOptionState()
    End Sub

    Private Sub ckCDFShowLegend_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckCDFShowLegend.CheckedChanged
        If Me.Tag = HelpTopic.CDFplot Then Me.UpdateCDFOptionState()
    End Sub

    Private Sub UpdateCDFOptionState()
        Dim empiricalOnly As Boolean = (Me.cmbCDFPlotType.SelectedIndex = 0)
        Dim fittedOnly As Boolean = (Me.cmbCDFPlotType.SelectedIndex = 2)

        Me.cmbCDFDistribution.Enabled = Not empiricalOnly
        Me.lblCDFDistribution.Enabled = Not empiricalOnly

        Me.cmbCDFEmpiricalMethod.Enabled = Not fittedOnly
        Me.lblCDFEmpiricalMethod.Enabled = Not fittedOnly
        Me.ckCDFExtendEmpirical.Enabled = Not fittedOnly

        Dim percentileOptionsEnabled As Boolean = Me.ckCDFPercentileLines.Checked
        Me.lblCDFPercentiles.Enabled = percentileOptionsEnabled
        Me.txtCDFPercentiles.Enabled = percentileOptionsEnabled
        Me.ckCDFPercentileLabels.Enabled = percentileOptionsEnabled

        Me.cmbCDFLegendMode.Enabled = Me.ckCDFShowLegend.Checked
        Me.lblCDFLegendMode.Enabled = Me.ckCDFShowLegend.Checked
    End Sub

    Private Sub cmdBoxMean_CheckedChanged(sender As Object, e As System.EventArgs) Handles cmdBoxMean.CheckedChanged
        If Me.Tag = HelpTopic.BoxAndWhiskers Then Me.UpdateBoxPlotOptionState()
    End Sub

    Private Sub cmbBoxPalette_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbBoxPalette.SelectedIndexChanged
        If Me.Tag = HelpTopic.BoxAndWhiskers Then Me.UpdateBoxPlotOptionState()
    End Sub

    Private Sub UpdateBoxPlotOptionState()
        Dim useLegacyAppearance As Boolean = (Me.cmbBoxPalette.SelectedIndex = BoxPlotLegacyPaletteIndex)
        Dim customAppearanceEnabled As Boolean = Not useLegacyAppearance

        'The legacy palette is a preset for the complete historical renderer, not
        'only a grey colour palette. Disable controls that are intentionally ignored
        'while preserving their values so they are restored when another palette is
        'selected.
        Me.cmdBoxMean.Enabled = customAppearanceEnabled
        Me.cmdBoxIndividualObs.Enabled = customAppearanceEnabled
        Me.ckBoxConnectMeans.Enabled = customAppearanceEnabled AndAlso Me.cmdBoxMean.Checked

        Me.nudBoxFillTransparency.Enabled = customAppearanceEnabled
        Me.lblBoxFillTransparency.Enabled = customAppearanceEnabled
        Me.cbBoxHorizontalGridlines.Enabled = customAppearanceEnabled
        Me.cbBoxOutline.Enabled = customAppearanceEnabled
        Me.nudBoxChartWidth.Enabled = customAppearanceEnabled
        Me.lblBoxChartWidth.Enabled = customAppearanceEnabled
        Me.nudBoxChartHeight.Enabled = customAppearanceEnabled
        Me.lblBoxChartHeight.Enabled = customAppearanceEnabled
    End Sub

    Private Sub ViolinBandwidth_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbViolinBandwidth.SelectedIndexChanged
        If Me.Tag = HelpTopic.ViolinPlot Then Me.UpdateViolinBandwidthOptionState()
    End Sub

    Private Sub UpdateViolinBandwidthOptionState()
        Dim manualBandwidth As Boolean = (Me.cmbViolinBandwidth.SelectedIndex = 2)

        If manualBandwidth Then
            Me.lblViolinBandwidthAdjustment.Text = "Manual bandwidth:"
            Me.nudViolinBandwidthAdjustment.DecimalPlaces = 6
            Me.nudViolinBandwidthAdjustment.Increment = 0.01D
            Me.nudViolinBandwidthAdjustment.Minimum = 0.000001D
            Me.nudViolinBandwidthAdjustment.Maximum = 1000000000D
        Else
            Me.lblViolinBandwidthAdjustment.Text = "Bandwidth Adjustment:"
            Me.nudViolinBandwidthAdjustment.DecimalPlaces = 2
            Me.nudViolinBandwidthAdjustment.Increment = 0.1D
            Me.nudViolinBandwidthAdjustment.Minimum = 0.01D
            Me.nudViolinBandwidthAdjustment.Maximum = 100D
        End If
    End Sub

    Private Sub CategoricalHistogramPlotType_CheckedChanged(sender As Object, e As System.EventArgs) Handles optCatHistBarsWithLegend.CheckedChanged,
                                                                                                             optCatHistStackedBar.CheckedChanged,
                                                                                                             optCatHistDifferentSampleSizes.CheckedChanged,
                                                                                                             optCatHistComparativeHorizontal.CheckedChanged,
                                                                                                             optCatHistComparativeVertical.CheckedChanged
        If Me.Tag = HelpTopic.CategoricalHistogram Then Me.UpdateCategoricalHistogramOptionState()
    End Sub

    Private Sub UpdateCategoricalHistogramOptionState()
        Dim isComparative As Boolean = Me.optCatHistComparativeHorizontal.Checked OrElse Me.optCatHistComparativeVertical.Checked
        Dim overlapEnabled As Boolean = Not Me.optCatHistStackedBar.Checked AndAlso Not isComparative
        Me.lblCatHistSeriesOverlap.Enabled = overlapEnabled
        Me.nudCatHistSeriesOverlap.Enabled = overlapEnabled
    End Sub

    Private Sub UnpairedTtestHypothesis_CheckedChanged(sender As Object, e As System.EventArgs) Handles optHypothesisSuperiority_UTT.CheckedChanged,
                                                                                                        optHypothesisNonInferiority_UTT.CheckedChanged,
                                                                                                        optHypothesisEquivalence_UTT.CheckedChanged
        If Me.Tag = HelpTopic.UnpairedTTest Then
            Me.UpdateUnpairedTtestOptionVisibility()
            Me.ApplyUnpairedTtestInputLabels()
        End If
    End Sub

    ' ====================================================================================
    ' Helperw
    ' ====================================================================================
    Private Sub ApplyUnpairedTtestInputLabels()
        If Me.optByColumn.Checked Then
            Me.lblRefedit1.Text = "Control / Reference group:"
            Me.lblRefedit2.Text = "Experimental / Test group:"
        Else
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
        End If
    End Sub

    Private Sub UpdateUnpairedTtestOptionVisibility()
        If Me.optHypothesisSuperiority_UTT.Checked Then
            Me.lblAlpha_UTT.Text = "Two-sided alpha:"
            Me.lblMargin_UTT.Visible = False
            Me.tbMargin_UTT.Visible = False
            Me.lblMarginHint_UTT.Visible = False
        ElseIf Me.optHypothesisNonInferiority_UTT.Checked Then
            Me.lblAlpha_UTT.Text = "One-sided alpha:"
            Me.lblMargin_UTT.Text = "NI margin:"
            Me.lblMargin_UTT.Visible = True
            Me.tbMargin_UTT.Visible = True
            Me.lblMarginHint_UTT.Visible = True
        ElseIf Me.optHypothesisEquivalence_UTT.Checked Then
            Me.lblAlpha_UTT.Text = "One-sided alpha:"
            Me.lblMargin_UTT.Text = "Equivalence margin (±):"
            Me.lblMargin_UTT.Visible = True
            Me.tbMargin_UTT.Visible = True
            Me.lblMarginHint_UTT.Visible = True
        End If
    End Sub

    Private Function ValidateUnpairedTtestOptionInputs() As Boolean
        If Me.Tag <> HelpTopic.UnpairedTTest Then Return False

        If (Me.optHypothesisNonInferiority_UTT.Checked OrElse Me.optHypothesisEquivalence_UTT.Checked) AndAlso Me.optByID.Checked Then
            MsgBox("For noninferiority or equivalence, use 'By Column' input so the direction is explicit: control/reference in Input 1 and experimental/test in Input 2.", vbExclamation)
            Me.TabControl1.SelectedIndex = 0
            Return True
        End If

        If Me.optHypothesisNonInferiority_UTT.Checked OrElse Me.optHypothesisEquivalence_UTT.Checked Then
            Dim marginValue As Double
            If Not Double.TryParse(Me.tbMargin_UTT.Text, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, marginValue) Then
                MsgBox("Enter a numeric positive margin using '.' as the decimal separator.", vbExclamation)
                Return True
            End If
            If marginValue <= 0 Then
                MsgBox("Margin must be greater than zero.", vbExclamation)
                Return True
            End If
        End If

        Return False
    End Function

    Private Function GetUnpairedTtestMargin() As Double
        Dim marginValue As Double
        If Not Double.TryParse(Me.tbMargin_UTT.Text, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, marginValue) Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("A numeric positive margin is required for noninferiority / equivalence."))
        End If
        If marginValue <= 0 Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Margin must be greater than zero."))
        End If
        Return marginValue
    End Function

    Private Function BuildUnpairedTtestNonInferiorityTable(resNi As equivalencetests.MeanNonInferiorityResult, groupNames() As String) As ResultTable
        Dim t = New ResultTable
        Dim varianceLabel As String = If(resNi.AssumeEqualVariances, "Equal variances (pooled)", "Welch unequal variances")

        t.AddHeaderTopRow({"Unpaired T-test", ""})
        t.AddHeaderTopRow({"Noninferiority", ""})
        t.SetBody({
                {"Control / Reference group", groupNames(0)},
                {"Experimental / Test group", groupNames(1)},
                {"Variance assumption", varianceLabel},
                {"Mean difference (Experimental - Control)", resNi.DifferenceExperimentalMinusControl},
                {"SE", resNi.StandardError},
                {"df", resNi.DegreesOfFreedom},
                {"One-sided alpha", resNi.AlphaOneSided},
                {"NI margin", resNi.NonInferiorityMargin},
                {"NI limit", resNi.NonInferiorityLimit},
                {"t", resNi.TestStatistic},
                {"One-sided p-value", resNi.PValue},
                {"Lower one-sided confidence limit", resNi.LowerOneSidedConfidenceLimit},
                {"Two-sided confidence interval", resNi.TwoSidedEquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Conclusion", resNi.Conclusion}
            })
        t.AddPvalueCellToFormat(11, 2)
        Return t
    End Function

    Private Function BuildUnpairedTtestEquivalenceTable(resEq As equivalencetests.MeanEquivalenceResult, groupNames() As String) As ResultTable
        Dim t = New ResultTable
        Dim varianceLabel As String = If(resEq.AssumeEqualVariances, "Equal variances (pooled)", "Welch unequal variances")

        t.AddHeaderTopRow({"Unpaired T-test", ""})
        t.AddHeaderTopRow({"Equivalence (TOST)", ""})
        t.SetBody({
                {"Control / Reference group", groupNames(0)},
                {"Experimental / Test group", groupNames(1)},
                {"Variance assumption", varianceLabel},
                {"Mean difference (Experimental - Control)", resEq.DifferenceExperimentalMinusControl},
                {"SE", resEq.StandardError},
                {"df", resEq.DegreesOfFreedom},
                {"One-sided alpha", resEq.AlphaOneSided},
                {"Lower margin", resEq.LowerMargin},
                {"Upper margin", resEq.UpperMargin},
                {"Lower-component t", resEq.LowerComponentStatistic},
                {"Lower-component p-value", resEq.LowerComponentPValue},
                {"Upper-component t", resEq.UpperComponentStatistic},
                {"Upper-component p-value", resEq.UpperComponentPValue},
                {"TOST p-value", resEq.TostPValue},
                {"Equivalent confidence interval", resEq.EquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Conclusion", resEq.Conclusion}
            })
        t.AddPvalueCellToFormat(11, 2)
        t.AddPvalueCellToFormat(13, 2)
        t.AddPvalueCellToFormat(14, 2)
        Return t
    End Function

End Class
