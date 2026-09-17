Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Public Class Ui01SankeyPlot

    Private Enum SankeyInputMode
        StagedData
        SourceTarget
    End Enum

    Private pLastInputMode As SankeyInputMode = SankeyInputMode.StagedData

    Public Sub New(tagn As Integer)
        InitializeComponent()
        Me.Tag = tagn

        Me.RefEdit_Stages.ExcelConnector = AppGlobals.app
        Me.RefEdit_StagedWeight.ExcelConnector = AppGlobals.app
        Me.RefEditOutput.ExcelConnector = AppGlobals.app
        Me.RefEdit_Source.ExcelConnector = AppGlobals.app
        Me.RefEdit_Target.ExcelConnector = AppGlobals.app
        Me.RefEdit_LinkWeight.ExcelConnector = AppGlobals.app
        Me.RefEdit_SourceStage.ExcelConnector = AppGlobals.app
        Me.RefEdit_TargetStage.ExcelConnector = AppGlobals.app

        Me.InitializeChoiceControls()
        Me.PrepareSharedOutputGroup()
        Me.UpdateOutputRangeState()
        Me.UpdateLabelControlState()
        Me.WireHelp(Me.btnHelp)
    End Sub

    ''' <summary>
    ''' Populates the DropDownList controls created in the Windows Forms designer.
    ''' The displayed order is intentionally kept in sync with the enum mapping helpers
    ''' below rather than relying on enum names as user-facing text.
    ''' </summary>
    Private Sub InitializeChoiceControls()
        Me.cmbNodeOrder.Items.Clear()
        Me.cmbNodeOrder.Items.AddRange(New Object() {
            "Automatic - minimise crossings",
            "First appearance",
            "Alphabetical",
            "Weight descending"
        })
        Me.cmbNodeOrder.SelectedIndex = 0

        Me.cmbVerticalAlignment.Items.Clear()
        Me.cmbVerticalAlignment.Items.AddRange(New Object() {
            "Center",
            "Top",
            "Bottom"
        })
        Me.cmbVerticalAlignment.SelectedIndex = 0

        Me.cmbNodeLabelMode.Items.Clear()
        Me.cmbNodeLabelMode.Items.AddRange(New Object() {
            "Name only",
            "Name + value",
            "Name + percentage",
            "Name + percentage + value"
        })
        Me.cmbNodeLabelMode.SelectedIndex = 3

        Me.cmbPercentageBasis.Items.Clear()
        Me.cmbPercentageBasis.Items.AddRange(New Object() {
            "Overall / reference total",
            "Stage total"
        })
        Me.cmbPercentageBasis.SelectedIndex = 0

        Me.cmbPalette.Items.Clear()
        Me.cmbPalette.Items.AddRange(New Object() {
            "Tableau 10",
            "Okabe-Ito",
            "ColorBrewer Set1",
            "Grayscale"
        })
        Me.cmbPalette.SelectedIndex = 0

        Me.cmbRibbonColorMode.Items.Clear()
        Me.cmbRibbonColorMode.Items.AddRange(New Object() {
            "Source node",
            "Target node",
            "Neutral grey"
        })
        Me.cmbRibbonColorMode.SelectedIndex = 0
    End Sub

    ''' <summary>
    ''' The form contains one shared Output group.  Keep it visible on whichever input
    ''' tab is active so Source/Target input has the same output choices as staged data.
    ''' </summary>
    Private Sub PrepareSharedOutputGroup()
        'Make enough room for the shared output group on the Source/Target page.
        Me.lblNoteSourceTarget.Location = New System.Drawing.Point(16, 282)
        Me.lblNoteSourceTarget.Size = New System.Drawing.Size(415, 38)
        Me.grpOutput.Location = New System.Drawing.Point(3, 329)
        Me.MoveOutputGroupToActiveInputTab()
    End Sub

    Private Sub MoveOutputGroupToActiveInputTab()
        If Me.TabMultipage.SelectedTab Is Me.TabPage1_StagedData Then
            If Not Me.grpOutput.Parent Is Me.TabPage1_StagedData Then
                Me.grpOutput.Parent = Me.TabPage1_StagedData
            End If
            Me.grpOutput.Location = New System.Drawing.Point(3, 329)
            Me.grpOutput.BringToFront()
        ElseIf Me.TabMultipage.SelectedTab Is Me.TabPage2_SourceTarget Then
            If Not Me.grpOutput.Parent Is Me.TabPage2_SourceTarget Then
                Me.grpOutput.Parent = Me.TabPage2_SourceTarget
            End If
            Me.grpOutput.Location = New System.Drawing.Point(3, 329)
            Me.grpOutput.BringToFront()
        End If
    End Sub

    Private Sub TabMultipage_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles TabMultipage.SelectedIndexChanged
        If Me.TabMultipage.SelectedTab Is Me.TabPage1_StagedData Then
            Me.pLastInputMode = SankeyInputMode.StagedData
        ElseIf Me.TabMultipage.SelectedTab Is Me.TabPage2_SourceTarget Then
            Me.pLastInputMode = SankeyInputMode.SourceTarget
        End If

        Me.MoveOutputGroupToActiveInputTab()
    End Sub

    Private Sub optOutputRange_CheckedChanged(sender As Object, e As System.EventArgs) Handles optOutputRange.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub optWorksheet_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorksheet.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub optWorkbook_CheckedChanged(sender As Object, e As System.EventArgs) Handles optWorkbook.CheckedChanged
        Me.UpdateOutputRangeState()
    End Sub

    Private Sub UpdateOutputRangeState()
        Me.RefEditOutput.Enabled = Me.optOutputRange.Checked
        If Me.optOutputRange.Checked AndAlso Me.RefEditOutput.CanFocus Then
            Me.RefEditOutput.txtAddress.Select()
        End If
    End Sub

    Private Sub ckShowNodeLabels_CheckedChanged(sender As Object, e As System.EventArgs) Handles ckShowNodeLabels.CheckedChanged
        Me.UpdateLabelControlState()
    End Sub

    Private Sub cmbNodeLabelMode_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles cmbNodeLabelMode.SelectedIndexChanged
        Me.UpdateLabelControlState()
    End Sub

    Private Sub UpdateLabelControlState()
        Dim labelsEnabled As Boolean = Me.ckShowNodeLabels.Checked
        Me.lblNodeLabelMode.Enabled = labelsEnabled
        Me.cmbNodeLabelMode.Enabled = labelsEnabled

        Dim usesPercentage As Boolean = labelsEnabled AndAlso
                                        (Me.cmbNodeLabelMode.SelectedIndex = 2 OrElse
                                         Me.cmbNodeLabelMode.SelectedIndex = 3)
        Me.lblPercentageBasis.Enabled = usesPercentage
        Me.cmbPercentageBasis.Enabled = usesPercentage
    End Sub

    ''' <summary>
    ''' Validates only the input mode last visited by the user.  This lets the Appearance
    ''' tab remain selected when Compute is pressed without losing the chosen data mode.
    ''' </summary>
    Private Function CheckInputs() As Boolean
        Dim primaryWorkbook As Workbook = Me.GetPrimaryInputWorkbook()
        If primaryWorkbook IsNot Nothing Then primaryWorkbook.Activate()

        If Me.pLastInputMode = SankeyInputMode.StagedData Then
            If Me.ValidateStagedInputs() Then Return True
        Else
            If Me.ValidateSourceTargetInputs() Then Return True
        End If

        If Me.optOutputRange.Checked Then
            Dim outputRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEditOutput,
                                      "Output range",
                                      requireSingleColumn:=False,
                                      requireSingleArea:=True,
                                      resolvedRange:=outputRange) Then
                Return True
            End If
        End If

        Return False
    End Function

    Private Function ValidateStagedInputs() As Boolean
        Dim stagesRange As Range = Nothing
        If Not Me.TryResolveRange(Me.RefEdit_Stages,
                                  "Stage variables",
                                  requireSingleColumn:=False,
                                  requireSingleArea:=True,
                                  resolvedRange:=stagesRange) Then
            Return True
        End If

        If stagesRange.Columns.Count < 2 Then
            MsgBox("Select at least two stage columns for a Sankey chart.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            RefEditReset(Me.RefEdit_Stages)
            Return True
        End If

        If Me.ckStagedFirstRowLabels.Checked AndAlso
           stagesRange.Rows.Count > 0 AndAlso
           stagesRange.Rows.Count < 2 Then
            MsgBox("The staged-data range must contain at least one data row below the stage labels.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return True
        End If

        If Not String.IsNullOrWhiteSpace(Me.RefEdit_StagedWeight.Address) Then
            Dim weightRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_StagedWeight,
                                      "Weight / frequency",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=weightRange) Then
                Return True
            End If

            If Not Me.AreRangesRowAligned(stagesRange, weightRange) Then
                MsgBox("The staged-data and weight ranges must start on the same row and contain the same number of rows.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If

            If Not Me.AreRangesOnSameWorksheet(stagesRange, weightRange) Then
                MsgBox("The staged-data and weight ranges must be on the same worksheet.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return True
            End If
        End If

        Return False
    End Function

    Private Function ValidateSourceTargetInputs() As Boolean
        Dim sourceRange As Range = Nothing
        If Not Me.TryResolveRange(Me.RefEdit_Source,
                                  "Source",
                                  requireSingleColumn:=True,
                                  requireSingleArea:=True,
                                  resolvedRange:=sourceRange) Then
            Return True
        End If

        Dim targetRange As Range = Nothing
        If Not Me.TryResolveRange(Me.RefEdit_Target,
                                  "Target",
                                  requireSingleColumn:=True,
                                  requireSingleArea:=True,
                                  resolvedRange:=targetRange) Then
            Return True
        End If

        If Not Me.ValidateAlignedInputRange(sourceRange, targetRange, "Target") Then Return True

        If Me.ckLinkFirstRowLabels.Checked AndAlso
           sourceRange.Rows.Count > 0 AndAlso
           sourceRange.Rows.Count < 2 Then
            MsgBox("The Source/Target ranges must contain at least one data row below the labels.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return True
        End If

        If Not String.IsNullOrWhiteSpace(Me.RefEdit_LinkWeight.Address) Then
            Dim weightRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEdit_LinkWeight,
                                      "Weight / frequency",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=weightRange) Then
                Return True
            End If
            If Not Me.ValidateAlignedInputRange(sourceRange, weightRange, "Weight / frequency") Then Return True
        End If

        Dim hasSourceStage As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_SourceStage.Address)
        Dim hasTargetStage As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_TargetStage.Address)
        If hasSourceStage Xor hasTargetStage Then
            MsgBox("Source stage and Target stage must either both be supplied or both be left blank.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return True
        End If

        If hasSourceStage Then
            Dim sourceStageRange As Range = Nothing
            Dim targetStageRange As Range = Nothing

            If Not Me.TryResolveRange(Me.RefEdit_SourceStage,
                                      "Source stage",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=sourceStageRange) Then
                Return True
            End If
            If Not Me.TryResolveRange(Me.RefEdit_TargetStage,
                                      "Target stage",
                                      requireSingleColumn:=True,
                                      requireSingleArea:=True,
                                      resolvedRange:=targetStageRange) Then
                Return True
            End If

            If Not Me.ValidateAlignedInputRange(sourceRange, sourceStageRange, "Source stage") Then Return True
            If Not Me.ValidateAlignedInputRange(sourceRange, targetStageRange, "Target stage") Then Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Resolves a RefEdit against the workbook captured by the RefEdit itself.  This is
    ''' slightly more robust than relying on whichever workbook happens to be active.
    ''' </summary>
    Private Function TryResolveRange(refEdit As Excel2007RefEdit,
                                     displayName As String,
                                     requireSingleColumn As Boolean,
                                     requireSingleArea As Boolean,
                                     ByRef resolvedRange As Range) As Boolean
        resolvedRange = Nothing

        If refEdit Is Nothing OrElse String.IsNullOrWhiteSpace(refEdit.Address) Then
            MsgBox(displayName & " range is empty. Please select a range.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        Try
            Dim wb As Workbook = refEdit.ExcelWorkBook
            If wb Is Nothing Then wb = AppGlobals.app.ActiveWorkbook
            If wb Is Nothing Then Throw New InvalidOperationException("No active Excel workbook is available.")

            Dim ws As Worksheet = WorksheetFromRefAdress(refEdit.Address, wb)
            resolvedRange = ws.Range(refEdit.Address)

            If requireSingleArea AndAlso resolvedRange.Areas.Count <> 1 Then
                MsgBox(displayName & " must be one continuous range.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return False
            End If

            If requireSingleColumn AndAlso resolvedRange.Columns.Count <> 1 Then
                MsgBox(displayName & " must contain exactly one column.",
                       vbExclamation,
                       AppGlobals.gsAPP_TITLE)
                Return False
            End If

            Return True
        Catch ex As Exception
            MsgBox(displayName & " is not a valid Excel range.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End Try
    End Function

    Private Function ValidateAlignedInputRange(referenceRange As Range,
                                               otherRange As Range,
                                               displayName As String) As Boolean
        If Not Me.AreRangesOnSameWorksheet(referenceRange, otherRange) Then
            MsgBox("Source, Target, and optional link variables must all be on the same worksheet. " &
                   displayName & " is on a different worksheet.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        If Not Me.AreRangesRowAligned(referenceRange, otherRange) Then
            MsgBox(displayName & " must start on the same row and contain the same number of rows as Source.",
                   vbExclamation,
                   AppGlobals.gsAPP_TITLE)
            Return False
        End If

        Return True
    End Function

    Private Function AreRangesOnSameWorksheet(firstRange As Range, secondRange As Range) As Boolean
        If firstRange Is Nothing OrElse secondRange Is Nothing Then Return False

        Dim firstSheet As Worksheet = TryCast(firstRange.Parent, Worksheet)
        Dim secondSheet As Worksheet = TryCast(secondRange.Parent, Worksheet)
        If firstSheet Is Nothing OrElse secondSheet Is Nothing Then Return False

        Dim firstWorkbook As Workbook = TryCast(firstSheet.Parent, Workbook)
        Dim secondWorkbook As Workbook = TryCast(secondSheet.Parent, Workbook)
        If firstWorkbook Is Nothing OrElse secondWorkbook Is Nothing Then Return False

        Return String.Equals(firstWorkbook.FullName,
                             secondWorkbook.FullName,
                             StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(firstSheet.Name,
                             secondSheet.Name,
                             StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function AreRangesRowAligned(firstRange As Range, secondRange As Range) As Boolean
        If firstRange Is Nothing OrElse secondRange Is Nothing Then Return False
        Return firstRange.Row = secondRange.Row AndAlso
               firstRange.Rows.Count = secondRange.Rows.Count
    End Function

    Private Function GetPrimaryInputWorkbook() As Workbook
        If Me.pLastInputMode = SankeyInputMode.StagedData Then
            Return Me.RefEdit_Stages.ExcelWorkBook
        End If
        Return Me.RefEdit_Source.ExcelWorkBook
    End Function

    ''' <summary>
    ''' Resolves a RefEdit without displaying validation messages. Compute-time callers use
    ''' this only after CheckInputs has already validated the selection.
    ''' </summary>
    Private Function ResolveRefEditRange(refEdit As Excel2007RefEdit) As Range
        If refEdit Is Nothing Then Throw New ArgumentNullException(NameOf(refEdit))
        If String.IsNullOrWhiteSpace(refEdit.Address) Then
            Throw New ArgumentException("The input range is empty.", NameOf(refEdit))
        End If

        Dim wb As Workbook = refEdit.ExcelWorkBook
        If wb Is Nothing Then wb = AppGlobals.app.ActiveWorkbook
        If wb Is Nothing Then Throw New InvalidOperationException("No active Excel workbook is available.")

        Dim ws As Worksheet = WorksheetFromRefAdress(refEdit.Address, wb)
        If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the worksheet for the selected input range.")
        Return ws.Range(refEdit.Address)
    End Function

    ''' <summary>
    ''' Returns the effective final row for a selected range. A bounded selection keeps its
    ''' explicit final row. A selection extending to the bottom of the worksheet (for example
    ''' A:D) is trimmed to the last used cell across ALL selected columns rather than the first
    ''' selected column only.
    ''' </summary>
    Private Function GetEffectiveLastRow(inputRange As Range) As Integer
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))

        Dim ws As Worksheet = TryCast(inputRange.Parent, Worksheet)
        If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the worksheet for the selected input range.")

        Dim firstRow As Integer = inputRange.Row
        Dim selectedLastRow As Long = CLng(firstRow) + CLng(inputRange.Rows.Count) - 1L

        'Honour a normal bounded selection exactly.
        If selectedLastRow < CLng(ws.Rows.Count) Then
            Return CInt(selectedLastRow)
        End If

        'For whole-column / to-bottom selections inspect every selected column.
        Dim lastUsedRow As Integer = firstRow
        For columnOffset As Integer = 0 To inputRange.Columns.Count - 1
            Dim columnNumber As Integer = inputRange.Column + columnOffset
            Dim bottomCell As Range = DirectCast(ws.Cells(ws.Rows.Count, columnNumber), Range)
            Dim candidate As Integer = CInt(bottomCell.End(XlDirection.xlUp).Row)
            If candidate >= firstRow AndAlso candidate > lastUsedRow Then
                lastUsedRow = candidate
            End If
        Next

        Return lastUsedRow
    End Function

    ''' <summary>
    ''' Determines one shared final row for every variable participating in the current Sankey
    ''' input. Optional blank RefEdits are ignored. This is important when one variable has a
    ''' legitimate trailing blank but another variable on the same row still contains data.
    ''' </summary>
    Private Function GetCommonInputLastRow(ParamArray refEdits() As Excel2007RefEdit) As Integer
        If refEdits Is Nothing OrElse refEdits.Length = 0 Then
            Throw New ArgumentException("At least one input range is required.", NameOf(refEdits))
        End If

        Dim commonLastRow As Integer = 0
        Dim commonFirstRow As Integer = -1

        For Each refEdit As Excel2007RefEdit In refEdits
            If refEdit Is Nothing OrElse String.IsNullOrWhiteSpace(refEdit.Address) Then Continue For

            Dim inputRange As Range = Me.ResolveRefEditRange(refEdit)
            If commonFirstRow < 0 Then
                commonFirstRow = inputRange.Row
            ElseIf inputRange.Row <> commonFirstRow Then
                Throw New ArgumentException("All Sankey input ranges must start on the same row.")
            End If

            Dim lastRow As Integer = Me.GetEffectiveLastRow(inputRange)
            If lastRow > commonLastRow Then commonLastRow = lastRow
        Next

        If commonLastRow <= 0 Then Throw New ArgumentException("No Sankey input data could be found.")
        Return commonLastRow
    End Function

    ''' <summary>
    ''' Builds a concrete sheet-qualified A1 range ending at the shared Sankey data row.
    ''' prepareRef2D is intentionally not used here: for full-column multi-column selections it
    ''' expands the columns back to whole-column references, which makes the shared importer
    ''' derive the row count from the first selected column only.
    ''' </summary>
    Private Function BuildBoundedReference(inputRange As Range, lastRow As Integer) As String
        If inputRange Is Nothing Then Throw New ArgumentNullException(NameOf(inputRange))

        Dim ws As Worksheet = TryCast(inputRange.Parent, Worksheet)
        If ws Is Nothing Then Throw New InvalidOperationException("Unable to resolve the worksheet for the selected input range.")

        Dim firstRow As Integer = inputRange.Row
        If lastRow < firstRow Then Throw New ArgumentException("The selected Sankey range does not contain any rows.")

        Dim firstColumn As Integer = inputRange.Column
        Dim lastColumn As Integer = firstColumn + inputRange.Columns.Count - 1
        Dim firstCell As Range = DirectCast(ws.Cells(firstRow, firstColumn), Range)
        Dim lastCell As Range = DirectCast(ws.Cells(lastRow, lastColumn), Range)
        Dim boundedRange As Range = ws.Range(firstCell, lastCell)
        Dim localAddress As String = boundedRange.Address(True, True, XlReferenceStyle.xlA1, False)
        Dim escapedSheetName As String = ws.Name.Replace("'", "''")
        Return "'" & escapedSheetName & "'!" & localAddress
    End Function

    ''' <summary>
    ''' Imports a selected Excel range through the shared Excel-DNA importer. The optional
    ''' sharedLastRow makes all variables in one Sankey input use the same concrete row extent.
    ''' </summary>
    Private Function ImportRefEdit(refEdit As Excel2007RefEdit,
                                   Optional sharedLastRow As Integer = 0) As CoreDataTable
        If refEdit Is Nothing Then Throw New ArgumentNullException(NameOf(refEdit))
        If String.IsNullOrWhiteSpace(refEdit.Address) Then Throw New ArgumentException("The input range is empty.", NameOf(refEdit))

        Dim wb As Workbook = refEdit.ExcelWorkBook
        If wb Is Nothing Then wb = AppGlobals.app.ActiveWorkbook
        If wb Is Nothing Then Throw New InvalidOperationException("No active Excel workbook is available.")

        wb.Activate()
        Dim inputRange As Range = Me.ResolveRefEditRange(refEdit)
        Dim finalRow As Integer = sharedLastRow
        If finalRow <= 0 Then finalRow = Me.GetEffectiveLastRow(inputRange)

        Dim boundedReference As String = Me.BuildBoundedReference(inputRange, finalRow)
        Return ExcelDnaDataImporter.Import(boundedReference, True)
    End Function

    Private Function ComputeStagedResult(options As SankeyPlotOptions) As SankeyPlotResult
        Dim commonLastRow As Integer = Me.GetCommonInputLastRow(Me.RefEdit_Stages,
                                                                Me.RefEdit_StagedWeight)
        Dim stageTable As CoreDataTable = Me.ImportRefEdit(Me.RefEdit_Stages, commonLastRow)
        If stageTable Is Nothing OrElse stageTable.ObjectMatrix Is Nothing Then
            Throw New InvalidOperationException("No staged data could be imported from the selected range.")
        End If

        Dim stageNames As String() = Nothing
        Dim stages As Object(,) = stageTable.ObjectMatrix

        If Me.ckStagedFirstRowLabels.Checked Then
            stageNames = Me.GetStageNamesFromFirstRow(stages)
            stages = Me.RemoveFirstRow(stages, "staged data")
        Else
            stageNames = Me.GetDefaultStageNames(stages.GetLength(1))
        End If

        Dim weights As Double() = Nothing
        If Not String.IsNullOrWhiteSpace(Me.RefEdit_StagedWeight.Address) Then
            Dim weightTable As CoreDataTable = Me.ImportRefEdit(Me.RefEdit_StagedWeight, commonLastRow)
            Dim weightMatrix As Object(,) = weightTable.ObjectMatrix
            If Me.ckStagedFirstRowLabels.Checked Then
                weightMatrix = Me.RemoveFirstRow(weightMatrix, "weight / frequency data")
            End If

            If weightMatrix.GetLength(0) <> stages.GetLength(0) Then
                Throw New ArgumentException("The weight / frequency range contains " &
                                            weightMatrix.GetLength(0).ToString(CultureInfo.InvariantCulture) &
                                            " data rows, while the staged-data range contains " &
                                            stages.GetLength(0).ToString(CultureInfo.InvariantCulture) & ".")
            End If
            weights = Me.ToWeightVector(weightMatrix, "Weight / frequency")
        End If

        Return SankeyPlot.ComputeStaged(stages,
                                        weights,
                                        stageNames,
                                        options)
    End Function

    Private Function ComputeSourceTargetResult(options As SankeyPlotOptions) As SankeyPlotResult
        Dim commonLastRow As Integer = Me.GetCommonInputLastRow(Me.RefEdit_Source,
                                                                Me.RefEdit_Target,
                                                                Me.RefEdit_LinkWeight,
                                                                Me.RefEdit_SourceStage,
                                                                Me.RefEdit_TargetStage)
        Dim sourceTable As CoreDataTable = Me.ImportRefEdit(Me.RefEdit_Source, commonLastRow)
        Dim targetTable As CoreDataTable = Me.ImportRefEdit(Me.RefEdit_Target, commonLastRow)

        Dim sourceMatrix As Object(,) = sourceTable.ObjectMatrix
        Dim targetMatrix As Object(,) = targetTable.ObjectMatrix

        If Me.ckLinkFirstRowLabels.Checked Then
            sourceMatrix = Me.RemoveFirstRow(sourceMatrix, "Source data")
            targetMatrix = Me.RemoveFirstRow(targetMatrix, "Target data")
        End If

        If sourceMatrix.GetLength(0) <> targetMatrix.GetLength(0) Then
            Throw New ArgumentException("Source and Target do not contain the same number of data rows.")
        End If

        Dim sourceValues As Object() = Me.ToObjectVector(sourceMatrix, "Source")
        Dim targetValues As Object() = Me.ToObjectVector(targetMatrix, "Target")

        Dim weights As Double() = Nothing
        If Not String.IsNullOrWhiteSpace(Me.RefEdit_LinkWeight.Address) Then
            Dim weightTable As CoreDataTable = Me.ImportRefEdit(Me.RefEdit_LinkWeight, commonLastRow)
            Dim weightMatrix As Object(,) = weightTable.ObjectMatrix
            If Me.ckLinkFirstRowLabels.Checked Then
                weightMatrix = Me.RemoveFirstRow(weightMatrix, "Weight / frequency data")
            End If
            If weightMatrix.GetLength(0) <> sourceValues.Length Then
                Throw New ArgumentException("The weight / frequency range does not contain the same number of data rows as Source and Target.")
            End If
            weights = Me.ToWeightVector(weightMatrix, "Weight / frequency")
        End If

        Dim sourceStages As Integer() = Nothing
        Dim targetStages As Integer() = Nothing
        Dim hasExplicitStages As Boolean = Not String.IsNullOrWhiteSpace(Me.RefEdit_SourceStage.Address)

        If hasExplicitStages Then
            Dim sourceStageTable As CoreDataTable = Me.ImportRefEdit(Me.RefEdit_SourceStage, commonLastRow)
            Dim targetStageTable As CoreDataTable = Me.ImportRefEdit(Me.RefEdit_TargetStage, commonLastRow)
            Dim sourceStageMatrix As Object(,) = sourceStageTable.ObjectMatrix
            Dim targetStageMatrix As Object(,) = targetStageTable.ObjectMatrix

            If Me.ckLinkFirstRowLabels.Checked Then
                sourceStageMatrix = Me.RemoveFirstRow(sourceStageMatrix, "Source stage data")
                targetStageMatrix = Me.RemoveFirstRow(targetStageMatrix, "Target stage data")
            End If

            If sourceStageMatrix.GetLength(0) <> sourceValues.Length OrElse
               targetStageMatrix.GetLength(0) <> sourceValues.Length Then
                Throw New ArgumentException("Source stage and Target stage must contain the same number of data rows as Source and Target.")
            End If

            sourceStages = Me.ToStageVector(sourceStageMatrix,
                                            sourceValues,
                                            targetValues,
                                            "Source stage")
            targetStages = Me.ToStageVector(targetStageMatrix,
                                            sourceValues,
                                            targetValues,
                                            "Target stage")
        End If

        Return SankeyPlot.Compute(sourceValues,
                                  targetValues,
                                  weights,
                                  sourceStages,
                                  targetStages,
                                  Nothing,
                                  options)
    End Function

    Private Function GetStageNamesFromFirstRow(values As Object(,)) As String()
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If values.GetLength(0) < 1 Then Throw New ArgumentException("The staged-data range is empty.")

        Dim count As Integer = values.GetLength(1)
        Dim result(count - 1) As String
        For columnIndex As Integer = 0 To count - 1
            Dim value As Object = values(0, columnIndex)
            If CoreDataTable.IsMissingValue(value) Then
                result(columnIndex) = "Stage " & (columnIndex + 1).ToString(CultureInfo.CurrentCulture)
            Else
                Dim label As String = Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
                result(columnIndex) = If(label = String.Empty,
                                         "Stage " & (columnIndex + 1).ToString(CultureInfo.CurrentCulture),
                                         label)
            End If
        Next
        Return result
    End Function

    Private Function GetDefaultStageNames(stageCount As Integer) As String()
        If stageCount < 1 Then Throw New ArgumentOutOfRangeException(NameOf(stageCount))
        Dim result(stageCount - 1) As String
        For i As Integer = 0 To stageCount - 1
            result(i) = "Stage " & (i + 1).ToString(CultureInfo.CurrentCulture)
        Next
        Return result
    End Function

    Private Function RemoveFirstRow(values As Object(,), displayName As String) As Object(,)
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        Dim rowCount As Integer = values.GetLength(0)
        Dim columnCount As Integer = values.GetLength(1)
        If rowCount <= 1 Then
            Throw New ArgumentException(displayName & " must contain at least one data row below the first-row labels.")
        End If

        Dim result(rowCount - 2, columnCount - 1) As Object
        For rowIndex As Integer = 1 To rowCount - 1
            For columnIndex As Integer = 0 To columnCount - 1
                result(rowIndex - 1, columnIndex) = values(rowIndex, columnIndex)
            Next
        Next
        Return result
    End Function

    Private Function ToObjectVector(values As Object(,), displayName As String) As Object()
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If values.GetLength(1) <> 1 Then
            Throw New ArgumentException(displayName & " must contain exactly one column.")
        End If

        Dim result(values.GetLength(0) - 1) As Object
        For i As Integer = 0 To result.Length - 1
            result(i) = values(i, 0)
        Next
        Return result
    End Function

    Private Function ToWeightVector(values As Object(,), displayName As String) As Double()
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If values.GetLength(1) <> 1 Then
            Throw New ArgumentException(displayName & " must contain exactly one column.")
        End If

        Dim result(values.GetLength(0) - 1) As Double
        For i As Integer = 0 To result.Length - 1
            Dim value As Object = values(i, 0)
            If CoreDataTable.IsMissingValue(value) Then
                result(i) = Double.NaN
            Else
                Dim number As Double
                If Not Me.TryConvertToDouble(value, number) Then
                    Throw New ArgumentException(displayName & " contains a non-numeric value at data row " &
                                                (i + 1).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                result(i) = number
            End If
        Next
        Return result
    End Function

    ''' <summary>
    ''' Converts an explicit stage column to integers.  Missing stage cells are tolerated
    ''' only on rows whose Source or Target is already missing because those rows are
    ''' excluded by the backend before stage validation.
    ''' </summary>
    Private Function ToStageVector(values As Object(,),
                                   sourceValues As Object(),
                                   targetValues As Object(),
                                   displayName As String) As Integer()
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If values.GetLength(1) <> 1 Then
            Throw New ArgumentException(displayName & " must contain exactly one column.")
        End If
        If values.GetLength(0) <> sourceValues.Length OrElse targetValues.Length <> sourceValues.Length Then
            Throw New ArgumentException(displayName & " must contain the same number of data rows as Source and Target.")
        End If

        Dim result(values.GetLength(0) - 1) As Integer
        For i As Integer = 0 To result.Length - 1
            Dim sourceMissing As Boolean = Me.IsMissingCategory(sourceValues(i))
            Dim targetMissing As Boolean = Me.IsMissingCategory(targetValues(i))
            Dim value As Object = values(i, 0)

            If sourceMissing OrElse targetMissing Then
                'The backend ignores this link row before consulting the stage arrays.
                result(i) = 0
                Continue For
            End If

            If CoreDataTable.IsMissingValue(value) Then
                Throw New ArgumentException(displayName & " is missing at data row " &
                                            (i + 1).ToString(CultureInfo.CurrentCulture) & ".")
            End If

            Dim number As Double
            If Not Me.TryConvertToDouble(value, number) OrElse
               Double.IsNaN(number) OrElse
               Double.IsInfinity(number) OrElse
               number < 0.0R OrElse
               number > Integer.MaxValue OrElse
               Math.Abs(number - Math.Round(number)) > 0.000000001R Then
                Throw New ArgumentException(displayName & " must contain non-negative whole-number stage values. " &
                                            "Invalid value at data row " &
                                            (i + 1).ToString(CultureInfo.CurrentCulture) & ".")
            End If

            result(i) = CInt(Math.Round(number))
        Next
        Return result
    End Function

    Private Function TryConvertToDouble(value As Object, ByRef result As Double) As Boolean
        result = Double.NaN
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return False
        If TypeOf value Is Boolean OrElse TypeOf value Is Date Then Return False

        Try
            If TypeOf value Is Byte OrElse
               TypeOf value Is SByte OrElse
               TypeOf value Is Short OrElse
               TypeOf value Is UShort OrElse
               TypeOf value Is Integer OrElse
               TypeOf value Is UInteger OrElse
               TypeOf value Is Long OrElse
               TypeOf value Is ULong OrElse
               TypeOf value Is Single OrElse
               TypeOf value Is Double OrElse
               TypeOf value Is Decimal Then
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                Return True
            End If

            Dim text As String = Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
            If text = String.Empty Then Return False

            If Double.TryParse(text,
                               NumberStyles.Float Or NumberStyles.AllowThousands,
                               CultureInfo.CurrentCulture,
                               result) Then
                Return True
            End If

            Return Double.TryParse(text,
                                   NumberStyles.Float Or NumberStyles.AllowThousands,
                                   CultureInfo.InvariantCulture,
                                   result)
        Catch
            Return False
        End Try
    End Function

    Private Function IsMissingCategory(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        Dim text As String = TryCast(value, String)
        Return text IsNot Nothing AndAlso String.IsNullOrWhiteSpace(text)
    End Function

    Private Function GetPlotOptions() As SankeyPlotOptions
        Dim options As New SankeyPlotOptions With {
            .NodeGapFraction = CDbl(Me.nudNodeGap.Value) / 100.0R,
            .NodeOrder = Me.GetSelectedNodeOrder(),
            .VerticalAlignment = Me.GetSelectedVerticalAlignment(),
            .ConnectAcrossMissingStages = Me.ckConnectAcrossMissingStages.Checked
        }
        Return options
    End Function

    Private Function GetSelectedNodeOrder() As SankeyNodeOrderMode
        Select Case Me.cmbNodeOrder.SelectedIndex
            Case 1
                Return SankeyNodeOrderMode.FirstAppearance
            Case 2
                Return SankeyNodeOrderMode.Alphabetical
            Case 3
                Return SankeyNodeOrderMode.WeightDescending
            Case Else
                Return SankeyNodeOrderMode.Automatic
        End Select
    End Function

    Private Function GetSelectedVerticalAlignment() As SankeyVerticalAlignment
        Select Case Me.cmbVerticalAlignment.SelectedIndex
            Case 1
                Return SankeyVerticalAlignment.Top
            Case 2
                Return SankeyVerticalAlignment.Bottom
            Case Else
                Return SankeyVerticalAlignment.Center
        End Select
    End Function

    Private Function GetPlotAppearance() As SankeyPlotAppearance
        Dim appearance As New SankeyPlotAppearance With {
            .ChartTitle = Me.txtChartTitle.Text.Trim(),
            .NodeColors = Me.GetGroupedPlotPalette(Me.cmbPalette.SelectedIndex),
            .NodeWidth = CDbl(Me.nudNodeWidth.Value),
            .RibbonColorMode = Me.GetSelectedRibbonColorMode(),
            .RibbonTransparency = CSng(CDbl(Me.nudRibbonTransparency.Value) / 100.0R),
            .ShowRibbonOutline = Me.ckShowRibbonOutline.Checked,
            .RibbonCurvature = CDbl(Me.nudRibbonCurvature.Value) / 100.0R,
            .ShowNodeLabels = Me.ckShowNodeLabels.Checked,
            .NodeLabelMode = Me.GetSelectedNodeLabelMode(),
            .PercentageBasis = Me.GetSelectedPercentageBasis(),
            .ShowStageLabels = Me.ckShowStageLabels.Checked,
            .ShowLegend = Me.ckShowLegend.Checked
        }
        Return appearance
    End Function

    Private Function GetSelectedRibbonColorMode() As SankeyRibbonColorMode
        Select Case Me.cmbRibbonColorMode.SelectedIndex
            Case 1
                Return SankeyRibbonColorMode.TargetNode
            Case 2
                Return SankeyRibbonColorMode.Neutral
            Case Else
                Return SankeyRibbonColorMode.SourceNode
        End Select
    End Function

    Private Function GetSelectedNodeLabelMode() As SankeyNodeLabelMode
        Select Case Me.cmbNodeLabelMode.SelectedIndex
            Case 1
                Return SankeyNodeLabelMode.NameAndValue
            Case 2
                Return SankeyNodeLabelMode.NameAndPercentage
            Case 3
                Return SankeyNodeLabelMode.NamePercentageAndValue
            Case Else
                Return SankeyNodeLabelMode.NameOnly
        End Select
    End Function

    Private Function GetSelectedPercentageBasis() As SankeyPercentageBasis
        If Me.cmbPercentageBasis.SelectedIndex = 1 Then
            Return SankeyPercentageBasis.StageTotal
        End If
        Return SankeyPercentageBasis.ReferenceTotal
    End Function

    ''' <summary>
    ''' Uses the same grouped-color presets as the categorical histogram and violin plot.
    ''' Values are Excel OLE RGB integers.
    ''' </summary>
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

    Private Sub btCompute_Click(sender As Object, e As System.EventArgs) Handles btCompute.Click
        Dim previousCursor As Cursor = Me.Cursor
        Try
            If Me.CheckInputs() Then Exit Sub

            Me.Cursor = Cursors.WaitCursor
            Me.btCompute.Enabled = False

            Dim inputWorkbook As Workbook = Me.GetPrimaryInputWorkbook()
            If inputWorkbook Is Nothing Then inputWorkbook = AppGlobals.app.ActiveWorkbook
            If inputWorkbook Is Nothing Then Throw New InvalidOperationException("No input workbook is available.")
            inputWorkbook.Activate()

            Dim options As SankeyPlotOptions = Me.GetPlotOptions()
            Dim result As SankeyPlotResult
            If Me.pLastInputMode = SankeyInputMode.StagedData Then
                result = Me.ComputeStagedResult(options)
            Else
                result = Me.ComputeSourceTargetResult(options)
            End If

            Dim appearance As SankeyPlotAppearance = Me.GetPlotAppearance()
            Dim outputWorksheet As Worksheet = Nothing
            Dim chartAnchor As Range = Nothing
            Me.ResolveOutputTarget(inputWorkbook, outputWorksheet, chartAnchor)

            DirectCast(outputWorksheet.Parent, Workbook).Activate()
            outputWorksheet.Activate()

            SankeyPlotExcel.AddChart(outputWorksheet,
                                     result,
                                     appearance,
                                     CDbl(chartAnchor.Left),
                                     CDbl(chartAnchor.Top),
                                     CDbl(Me.nudChartWidth.Value),
                                     CDbl(Me.nudChartHeight.Value))
        Catch ex As Exception
            CoreServices.Errors.LogAndThrow(ex,
                                            False,
                                            True,
                                            "Unable to create the Sankey plot")
        Finally
            Me.btCompute.Enabled = True
            Me.Cursor = previousCursor
        End Try
    End Sub

    ''' <summary>
    ''' Resolves the common BESHStatNG output choices to one worksheet and top-left
    ''' chart anchor.  Sankey output is graphical, so no worksheet cells are overwritten.
    ''' </summary>
    Private Sub ResolveOutputTarget(inputWorkbook As Workbook,
                                    ByRef outputWorksheet As Worksheet,
                                    ByRef chartAnchor As Range)
        outputWorksheet = Nothing
        chartAnchor = Nothing

        If Me.optWorkbook.Checked Then
            Dim wb As Workbook = AppGlobals.app.Workbooks.Add()
            outputWorksheet = DirectCast(wb.Worksheets(1), Worksheet)
            chartAnchor = DirectCast(outputWorksheet.Cells(1, 1), Range)
            Return
        End If

        If Me.optWorksheet.Checked Then
            If inputWorkbook Is Nothing Then Throw New ArgumentNullException(NameOf(inputWorkbook))
            inputWorkbook.Activate()
            outputWorksheet = DirectCast(inputWorkbook.Worksheets.Add(), Worksheet)
            chartAnchor = DirectCast(outputWorksheet.Cells(1, 1), Range)
            Return
        End If

        If Me.optOutputRange.Checked Then
            Dim selectedRange As Range = Nothing
            If Not Me.TryResolveRange(Me.RefEditOutput,
                                      "Output range",
                                      requireSingleColumn:=False,
                                      requireSingleArea:=True,
                                      resolvedRange:=selectedRange) Then
                Throw New ArgumentException("A valid output range is required.")
            End If

            outputWorksheet = TryCast(selectedRange.Parent, Worksheet)
            If outputWorksheet Is Nothing Then Throw New InvalidOperationException("Unable to resolve the output worksheet.")
            chartAnchor = DirectCast(selectedRange.Cells(1, 1), Range)
            Return
        End If

        Throw New InvalidOperationException("Select an output destination.")
    End Sub

End Class
