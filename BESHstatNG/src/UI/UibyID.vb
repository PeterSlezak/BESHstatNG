Imports System.Runtime.InteropServices.ComTypes
Imports System.Security.Cryptography
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class UibyID

    Sub New(analysis As String)
        ' This call is required by the designer.
        InitializeComponent()

        Me.RefEdit1.ExcelConnector = AppGlobals.app
        Me.RefEdit2.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.Text = analysis

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

        If Me.Text = "Kruskal-Wallis Test" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.grpHomogeneityVariances.Visible = True

        ElseIf Me.Text = "Box and Whiskers" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True

        ElseIf Me.Text = "One-Way ANOVA" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.grpHomogeneityVariances.Visible = True
            Me.grpANOVA1MCP.Visible = True
            Me.ckWelch.Visible = True

        ElseIf Me.Text = "Mann-Whitney Test" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.ckEstimateOfShift.Visible = True
            Me.spinBtnAlpha.Visible = True
            Me.lblAlpha.Visible = True

        ElseIf Me.Text = "Unpaired T-test" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.spinBtnAlpha.Visible = True
            Me.lblAlpha.Visible = True

        ElseIf Me.Text = "ROC Curve" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.spinBtnAlpha.Visible = True
            Me.lblAlpha.Visible = True

        ElseIf Me.Text = "Descriptive Statistcs" Then
            Me.TabPage_OptionsDescriptive.Parent = Me.TabMultipage
            Me.ckBoxPlot_Descriptive.Visible = True

        ElseIf Me.Text = "Normality" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True

        ElseIf Me.Text = "Histogram" Then
            Me.TabPage_OptionsHistogram.Parent = Me.TabMultipage

        ElseIf Me.Text = "Normal Plot" Then
            Me.TabPage_OptionsNormalPlot.Parent = Me.TabMultipage

        ElseIf Me.Text = "Homogeneity Of Variance" Then
            Me.TabPage_Options.Parent = Me.TabMultipage
            Me.ckDescriptiveStatistics.Visible = True
            Me.ckBoxPlot.Visible = True
            Me.grpHomogeneityVariances.Visible = True

        ElseIf Me.Text = "Symmetry" Then
            Me.TabPage_OptionsSymmetry.Parent = Me.TabMultipage

        ElseIf Me.Text = "Outliers" Then
            Me.TabPage_OptionsOutliers.Parent = Me.TabMultipage

        End If

        Me.RefEdit1.txtAddress.Select()
        Me.WireHelp(Me.btnHelp)
    End Sub

    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If Me.Text = "Mann-Whitney Test" Or Me.Text = "Unpaired T-test" Or Me.Text = "ROC Curve" Or Me.optByID.Checked Then
            If CheckRefEdit(Me.RefEdit1.Address, True) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEdit1)
                bOut = True
            End If

            If CheckRefEdit(Me.RefEdit2.Address, True) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEdit2)
                bOut = True
            End If
        Else
            If CheckRefEdit(Me.RefEdit2.Address) Then
                Me.TabMultipage.SelectedIndex = 0
                RefEditReset(Me.RefEdit2)
                bOut = True
            End If
        End If

        If Me.optOutputRange.Checked Then
            If CheckRefEdit(Me.RefEditOutput.Address) Then
                Me.TabMultipage.SelectedIndex = 0
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
            columData.DataInport(prepareRef2D(Me.RefEdit1.Address), True)
            columData2.DataInport(prepareRef2D(Me.RefEdit2.Address), True)

            out.X1 = Matrix.GetColumnFrom2Darray(columData.DataDbl, 0)
            out.X2 = Matrix.GetColumnFrom2Darray(columData2.DataDbl, 0)
            out.name1 = columData.varNames(0)
            out.name2 = columData2.varNames(0)
        Else
            'Group by identifier. We expect only two groups for Mann-Whitney test
            If WorksheetNameFromRefAdress(Me.RefEdit1.Address, True) <> WorksheetNameFromRefAdress(Me.RefEdit2.Address, True) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Input reference range adresses are from different sheets. Input can be only from one sheet."))
            End If

            refId = prepareRef2D(Me.RefEdit1.Address)
            refData = prepareRef2D(Me.RefEdit2.Address)
            'join reference address into one (remove sheet name from the second and concatenate).
            'Data can be only form one sheet because of the above check
            refFinal = refId & ", " & Replace(refData, WorksheetNameFromRefAdress(refData, True) & "!", String.Empty) 'Remove "Sheet1!" from string
            columData.DataInport(refFinal, True, 0)

            'get unique group IDs

            groupIDs = Matrix.GetColumnFrom2Darray(columData.FinalData, 0).Distinct().ToArray()
            Debug.Print(Matrix.array2str(groupIDs))
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
            Debug.Print(ref)
            ii = 0 'use separate column counter so we can drop completely missing columns

            Dim outData()() As Double = New Double()() {}
            ReDim groupIDs(NoGroups - 1), outData(NoGroups - 1)
            For i = 0 To NoGroups - 1
                Dim ref1 As String = WorksheetNameFromRefAdress(ref, True) & "!" & colList(i)
                'Debug.Print(ref1)
                Dim columData = New DataObj
                columData.DataInport(ref1, True)
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
            byIdData.DataInport(refFinal, True, 0)

            'Debug.Print(array2str(byIdData.FinalData))
            out.X = byIdData.DataByID2ByColumn()
            out.varNames = Matrix.Array2strArray(Matrix.GetColumnFrom2Darray(byIdData.FinalData, 0).Distinct().ToArray())
            Return out
        End If

    End Function

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Try
            Dim errText As String = String.Empty
            Dim MWdata As TwoGroupsData, data As MultiGroupsUnpairedData

            'Validate Inputs
            If Me.checkInputs() Then Exit Sub

            If Me.Text = "Mann-Whitney Test" Or Me.Text = "Unpaired T-test" Or Me.Text = "ROC Curve" Then

                'Get Data
                MWdata = Me.getData2Groups(errText)
                If errText <> String.Empty Then
                    MsgBox(errText, vbExclamation)
                    Exit Sub
                End If
                data = New MultiGroupsUnpairedData
                data.X = {MWdata.X1, MWdata.X2}
                data.varNames = {MWdata.name1, MWdata.name2}

                If Me.Text = "Mann-Whitney Test" Then
                    Me.RunMannWhitney(data)
                ElseIf Me.Text = "Unpaired T-test" Then
                    Me.RunTtest(data)
                ElseIf Me.Text = "ROC Curve" Then
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
                If Me.Text = "Kruskal-Wallis Test" Then
                    Me.RunKruskallWalis(data)
                ElseIf Me.Text = "Box And Whiskers" Then
                    Me.RunBoxAndWhiskers(data)
                ElseIf Me.Text = "One-Way ANOVA" Then
                    Me.RunOneWayANOVA(data)
                ElseIf Me.Text = "Descriptive Statistcs" Then
                    Me.RunDescStat(data)
                ElseIf Me.Text = "Normality" Then
                    Me.RunNormality(data)
                ElseIf Me.Text = "Histogram" Then
                    Me.RunHistogram(data)
                ElseIf Me.Text = "Normal Plot" Then
                    Me.RunNormalPlot(data)
                ElseIf Me.Text = "Homogeneity Of Variance" Then
                    Me.RunHomogeneityVar(data)
                ElseIf Me.Text = "Symmetry" Then
                    Me.RunSymmetry(data)
                ElseIf Me.Text = "Outliers" Then
                    Me.RunOutliers(data)
                End If
            End If
        Catch ex As Exception
            AppGlobals.BSerr.LogAndThrow(ex, False, True)
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
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
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
        If Me.ckDescriptiveStatistics.Checked Then Res.add(Me.ComputeDescriptiveStats(data))

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
            Dim rr = New ProcessListofResultTables(Res)
            Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
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
        Dim res = New List(Of  ResultTable)
        Dim strBiningTyp As String = String.Empty
        Dim histList = New List(Of graphics.Histogram)

        If Me.optSturges.Checked Then
            strBiningTyp = "(Sturges)"
        ElseIf Me.optDoane.Checked Then
            strBiningTyp = "(Doane)"
        ElseIf Me.optScott.checked Then
            strBiningTyp = "(Scott)"
        ElseIf Me.optFreedmanDiaconis.Checked Then
            strBiningTyp = "(Freedman-Diaconis)"
        End If

        For i = 0 To data.X.Length - 1
            Dim hist = New graphics.Histogram(data.X(i))
            Dim d = hist.compute(Me.ckOverlay.Checked, strBiningTyp)
            HisData.Add(d)
            histList.Add(hist)
        Next

        'descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then Res.add(Me.ComputeDescriptiveStats(data))

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        If Me.ckDescriptiveStatistics.Checked Then
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
            'WriteRes.write({data.varNames(i)})
            'WriteRes.write({"Bins MidPoints", "Frequencies"})
            'WriteRes.write(HisData(i))

            Dim strHistogramTitle As String = $"Histogram {strBiningTyp} - {data.varNames(i)}"

            histList(i).addChart(WriteRes.ws, row + 1, col, strHistogramTitle)

            'WriteRes.setRowPointer(row)
            'WriteRes.setColumnPointer(col + 2)
        Next i

        If Me.ckDescriptiveStatistics.Checked Then rr.writeToSheet(WriteRes, True)
    End Sub

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
                AppGlobals.BSlogg.Log("N not in range between 3 and 5000")
                sw_r = {{"NA n<4 or n>5000"}, {"NA n<4 or n>5000"}, {"NA n<4 or n>5000"}}
            End If


            If data.X(i).Length >= 9 Then
                Dim da_res = assumptions.DAgostino(data.X(i), strErr)
                da_r = {{""}, {da_res.TestStatistics1}, {da_res.Pvalue}}
            Else
                AppGlobals.BSlogg.Log("N not >= 9")
                da_r = {{"NA n<9"}, {"NA n<9"}, {"NA n<9"}}
            End If

            If data.X(i).Length > 1 Then
                Dim ad_res = assumptions.AndersonDarlingTEST(data.X(i))
                ad_r = {{""}, {ad_res.TestStatistics1}, {ad_res.Pvalue}}
            Else
                AppGlobals.BSlogg.Log("N not > 1")
                ad_r = {{"NA"}, {"NA"}, {"NA"}}
            End If

            results = Matrix.VerticalStackArrays(results, Matrix.HorizontalStackArrays(Matrix.HorizontalStackArrays(sw_r, da_r), ad_r))
        Next
        rTable.SetBody(results)
        rTable.AddHeaderTopRow(Matrix.ConcatArrays({"Normality Tests"}, data.varNames))
        res.add(rTable)

        'descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then Res.add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.Add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
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

        Res.add(Me.ComputeDescriptiveStats(data, stat))

        'box plot if requested
        If Me.ckBoxPlot_Descriptive.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            Res.add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
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
        If Me.ckDescriptiveStatistics.Checked Then Res.add(Me.ComputeDescriptiveStats(data))

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(res)
        Dim totrows As Integer = RR.TotRows + Res.Count - 1 'one blank row as a separator
        Dim totcols As Integer = RR.TotCols
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
        Dim alphaValue As Double = CDbl(Me.spinBtnAlpha.Value)
        Dim ttest = New parametric.UnpairedTtest(data.X, data.varNames)
        ttest.compute(alphaValue)
        Dim res = ttest.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then Res.add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            Res.add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
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
        descTable.AddTitle("Descriptive Statistics")
        descTable.AddHeaderTopRow(Matrix.ConcatArrays({""}, data.varNames))

        Return descTable
    End Function

    Private Sub RunOneWayANOVA(data As MultiGroupsUnpairedData)
        Dim box As graphics.BoxPlot = Nothing
        Dim anova = New parametric.OneWayANOVA(data.X, data.varNames)
        anova.compute()
        If Me.ckWelch.Checked Then anova.WelshANOVA()
        If Me.ckLSD.Checked Then anova.FisherLSD()
        If Me.ckBonferroni.Checked Then anova.FisherLSD(True)
        If Me.ckTukey.Checked Then anova.TukeyKramer()
        If Me.ckGamesHowell.Checked Then anova.GamesHowell()
        Dim res = anova.wrapResults()

        'homogeneity of variances
        If Me.ckBartlett.Checked Or Me.ckFlignerKilleen.Checked Or
        Me.ckLevene.Checked Or Me.ckSquaredRanks.Checked Then Res.add(Me.ComputeVarianceHomogeneity(data))

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then Res.add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            Res.add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
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

    Private Sub RunBoxAndWhiskers(data As MultiGroupsUnpairedData)
        Dim res = New List(Of ResultTable)
        Dim box = New graphics.BoxPlot(data.X, data.varNames)
        box.Calculate()
        box.CalcForPlotting()
        res.Add(box.wrapResults())

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

        If Me.ckBoxPlot.Checked Then
            box.SetWs = WriteRes.ws
            box.AddBoxPlot()
        End If
    End Sub

    Private Sub RunKruskallWalis(data As MultiGroupsUnpairedData)
        Dim box As graphics.BoxPlot = Nothing

        'Compute test
        Dim KW As New nonparametric.KruskallWalis(data.X, data.varNames)
        KW.compute()
        KW.MCP()
        Dim res = KW.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then Res.add(Me.ComputeDescriptiveStats(data))
        If Me.ckBartlett.Checked Or Me.ckFlignerKilleen.Checked Or
            Me.ckLevene.Checked Or Me.ckSquaredRanks.Checked Then res.Add(Me.ComputeVarianceHomogeneity(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            Res.add(box.wrapResults())
        End If

        'Dump outputs
        Dim WriteRes = GetResultWriter() 'pass just table from the main test output
        Dim rr = New ProcessListofResultTables(Res)
        Dim totrows As Integer = rr.TotRows + Res.Count - 1 'one blank row as a separator
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
        MW.Compute(Me.progressBarExactCalc)
        If Me.ckEstimateOfShift.Checked Then MW.ComputeShift(alphaValue)
        Dim res = MW.wrapResults()

        'Compute descriptive statistics
        If Me.ckDescriptiveStatistics.Checked Then res.add(Me.ComputeDescriptiveStats(data))

        'box plot if requested
        If Me.ckBoxPlot.Checked Then
            box = New graphics.BoxPlot(data.X, data.varNames)
            box.Calculate()
            box.CalcForPlotting()
            res.add(box.wrapResults())
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

    Private Function GetResultWriter() As WriteResults
        Dim WriteRes = New WriteResults, rRange As Range
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
        If Me.Text = "Mann-Whitney Test" Or Me.Text = "Unpaired T-test" Or Me.Text = "ROC Curve" Then
            Me.lblRefedit1.Text = If(Me.Text = "ROC Curve", "Group with characteristic present (patients)", "Data: Group 1")
            Me.lblRefedit2.Text = If(Me.Text = "ROC Curve", "Group with characteristic absent (controls)", "Data: Group 2")
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()

        Else
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.Enabled = False
            Me.lblRefedit1.Enabled = False
            Me.RefEdit2.txtAddress.Select()
        End If
    End Sub

    Private Sub optByID_Click(sender As Object, e As System.EventArgs) Handles optByID.Click
        If Me.Text = "Mann-Whitney Test" Or Me.Text = "Unpaired T-test" Or Me.Text = "ROC Curve" Then
            Me.RefEdit1.txtAddress.Text = String.Empty
            Me.RefEdit1.txtAddress.Select()
            Me.lblRefedit1.Text = "Group ID:"
            Me.lblRefedit2.Text = "Data:"
        Else
            Me.RefEdit1.Enabled = True
            Me.RefEdit1.Enabled = True
        End If
        Me.RefEdit1.txtAddress.Select()
    End Sub
End Class
