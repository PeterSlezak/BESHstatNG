Option Strict On
Option Explicit On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.CausalInference
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG.Test.Harness46
Namespace Harness47
Friend Partial Class UiGLMProjection47
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend pRegressionCalculationRunning As Boolean
 Friend pRegressionCancelRequested As Boolean
 Friend pRegressionCloseAfterCancel As Boolean
 Friend pRegressionInterruptRequested As Boolean
 Friend WithEvents ProgressBar1 As New FakeControl47()
 Friend WithEvents btCalculate As New FakeControl47()
 Friend WithEvents btInterrupt As New FakeControl47()
 Friend WithEvents lblProgress As New FakeControl47()
    Private Sub BeginRegressionComputation()
        pRegressionCancelRequested = False
        pRegressionInterruptRequested = False
        pRegressionCloseAfterCancel = False
        pRegressionCalculationRunning = True
        Me.Bridge.SetCallbacks(AddressOf IsRegressionCancellationRequested, AddressOf IsRegressionInterruptionRequested)

        Try
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = True
            Me.lblProgress.Text = "Preparing calculation..."
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
    Private Sub EndRegressionComputation()
        pRegressionCalculationRunning = False
        Me.Bridge.ClearCallbacks()

        Try
            If pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested Then
                Me.lblProgress.Text = "Calculation interrupted; latest accepted estimates returned."
            End If
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try

        If pRegressionCloseAfterCancel Then
            pRegressionCloseAfterCancel = False
            Try
                Me.Close()
            Catch
            End Try
        End If
    End Sub
    Private Function IsRegressionCancellationRequested() As Boolean
        Return pRegressionCancelRequested
    End Function
    Private Function IsRegressionInterruptionRequested() As Boolean
        Return pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested
    End Function
    Private Sub FinishRegressionComputation(message As String)
        Try
            Me.ProgressBar1.Style = FakeProgressStyle47.Continuous
            Me.lblProgress.Text = message
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pRegressionCalculationRunning Then Exit Sub

        pRegressionInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
End Class
Friend Partial Class Ui13GEEProjection47
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend pRegressionCalculationRunning As Boolean
 Friend pRegressionCancelRequested As Boolean
 Friend pRegressionCloseAfterCancel As Boolean
 Friend pRegressionInterruptRequested As Boolean
 Friend WithEvents ProgressBar1 As New FakeControl47()
 Friend WithEvents btCalculate As New FakeControl47()
 Friend WithEvents btInterrupt As New FakeControl47()
 Friend WithEvents lblProgress As New FakeControl47()
    Private Sub BeginRegressionComputation()
        pRegressionCancelRequested = False
        pRegressionInterruptRequested = False
        pRegressionCloseAfterCancel = False
        pRegressionCalculationRunning = True
        Me.Bridge.SetCallbacks(AddressOf IsRegressionCancellationRequested, AddressOf IsRegressionInterruptionRequested)

        Try
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = True
            Me.lblProgress.Text = "Preparing calculation..."
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
    Private Sub EndRegressionComputation()
        pRegressionCalculationRunning = False
        Me.Bridge.ClearCallbacks()

        Try
            If pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested Then
                Me.lblProgress.Text = "Calculation interrupted; latest accepted estimates returned."
            End If
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try

        If pRegressionCloseAfterCancel Then
            pRegressionCloseAfterCancel = False
            Try
                Me.Close()
            Catch
            End Try
        End If
    End Sub
    Private Function IsRegressionCancellationRequested() As Boolean
        Return pRegressionCancelRequested
    End Function
    Private Function IsRegressionInterruptionRequested() As Boolean
        Return pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested
    End Function
    Private Sub FinishRegressionComputation(message As String)
        Try
            Me.ProgressBar1.Style = FakeProgressStyle47.Continuous
            Me.lblProgress.Text = message
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pRegressionCalculationRunning Then Exit Sub

        pRegressionInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
End Class
Friend Partial Class Ui4CoxProjection47
 Friend Tag As HelpTopic
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend pRegressionCalculationRunning As Boolean
 Friend pRegressionCancelRequested As Boolean
 Friend pRegressionCloseAfterCancel As Boolean
 Friend pRegressionInterruptRequested As Boolean
 Friend WithEvents ProgressBar1 As New FakeControl47()
 Friend WithEvents btCalculate As New FakeControl47()
 Friend WithEvents btInterrupt As New FakeControl47()
 Friend WithEvents lblProgress As New FakeControl47()
    Private Sub BeginRegressionComputation()
        pRegressionCancelRequested = False
        pRegressionInterruptRequested = False
        pRegressionCloseAfterCancel = False
        pRegressionCalculationRunning = True
        Me.Bridge.SetCallbacks(AddressOf IsRegressionCancellationRequested, AddressOf IsRegressionInterruptionRequested)

        Try
            Me.btCalculate.Enabled = False
            Me.btInterrupt.Enabled = True
            Me.lblProgress.Text = "Preparing calculation..."
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
    Private Sub EndRegressionComputation()
        pRegressionCalculationRunning = False
        Me.Bridge.ClearCallbacks()

        Try
            If pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested Then
                Me.lblProgress.Text = "Calculation interrupted; latest accepted estimates returned."
            End If
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try

        If pRegressionCloseAfterCancel Then
            pRegressionCloseAfterCancel = False
            Try
                Me.Close()
            Catch
            End Try
        End If
    End Sub
    Private Function IsRegressionCancellationRequested() As Boolean
        Return pRegressionCancelRequested
    End Function
    Private Function IsRegressionInterruptionRequested() As Boolean
        Return pRegressionInterruptRequested AndAlso Not pRegressionCancelRequested
    End Function
    Private Sub FinishRegressionComputation(message As String)
        Try
            Me.ProgressBar1.Style = FakeProgressStyle47.Continuous
            Me.lblProgress.Text = message
            Me.btCalculate.Enabled = True
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pRegressionCalculationRunning Then Exit Sub

        pRegressionInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
End Class
Friend Partial Class Ui18MMRMProjection47
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend pMmrmCalculationRunning As Boolean
 Friend pMmrmCancelRequested As Boolean
 Friend pMmrmInterruptRequested As Boolean
 Friend WithEvents btInterrupt As New FakeControl47()
 Friend WithEvents lblProgress As New FakeControl47()
    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pMmrmCalculationRunning Then Exit Sub

        pMmrmInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting MMRM; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
    Private Function IsMMRMCancellationRequested() As Boolean
        Return pMmrmCancelRequested
    End Function
    Private Function IsMMRMInterruptionRequested() As Boolean
        Return pMmrmInterruptRequested AndAlso Not pMmrmCancelRequested
    End Function
End Class
Friend Partial Class Ui19LMMProjection47
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend pLmmCalculationRunning As Boolean
 Friend plmmInterruptRequested As Boolean
 Friend WithEvents btInterrupt As New FakeControl47()
 Friend WithEvents lblProgress As New FakeControl47()
    Private Sub btInterrupt_Click(sender As Object, e As System.EventArgs) Handles btInterrupt.Click
        If Not pLmmCalculationRunning Then Exit Sub

        plmmInterruptRequested = True

        Try
            Me.lblProgress.Text = "Interrupting LMM; latest accepted estimates will be returned..."
            Me.btInterrupt.Enabled = False
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
End Class
Friend Partial Class Ui20PropensityScoreMatchingProjection47
 Friend Tag As HelpTopic
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend WithEvents ProgressBar1 As New FakeControl47()
 Friend WithEvents btAddScore As New FakeControl47()
 Friend WithEvents btCalculate As New FakeControl47()
 Friend WithEvents btRemoveScore As New FakeControl47()
 Friend WithEvents cbCaliperScale As New FakeControl47()
 Friend WithEvents cbCommonSupport As New FakeControl47()
 Friend WithEvents cbDistanceMetric As New FakeControl47()
 Friend WithEvents cbEstimand As New FakeControl47()
 Friend WithEvents cbMatchingOrder As New FakeControl47()
 Friend WithEvents cbRunMethod As New FakeControl47()
 Friend WithEvents cbScoreMethod As New FakeControl47()
 Friend WithEvents chkDoublyRobust As New FakeControl47()
 Friend WithEvents chkLovePlot As New FakeControl47()
 Friend WithEvents chkOverlapDiagnostics As New FakeControl47()
 Friend WithEvents chkStandardizeCovariates As New FakeControl47()
 Friend WithEvents chkWeightDiagnostics As New FakeControl47()
 Friend WithEvents chkWithReplacement As New FakeControl47()
 Friend WithEvents chkWriteDiagnostics As New FakeControl47()
 Friend WithEvents chkWriteMatches As New FakeControl47()
 Friend WithEvents ckIntercept As New FakeControl47()
 Friend WithEvents grpIterOptions As New FakeControl47()
 Friend WithEvents lbScore As New FakeControl47()
 Friend WithEvents lblCaliper As New FakeControl47()
 Friend WithEvents lblCaliperScale As New FakeControl47()
 Friend WithEvents lblCemBins As New FakeControl47()
 Friend WithEvents lblDistanceMetric As New FakeControl47()
 Friend WithEvents lblMatchingOrder As New FakeControl47()
 Friend WithEvents lblMatchingRatio As New FakeControl47()
 Friend WithEvents lblMaxIterations As New FakeControl47()
 Friend WithEvents lblProgress As New FakeControl47()
 Friend WithEvents lblRidgePenalty As New FakeControl47()
 Friend WithEvents lblScore As New FakeControl47()
 Friend WithEvents lblStrata As New FakeControl47()
 Friend WithEvents lblTolerance As New FakeControl47()
 Friend WithEvents tbCaliper As New FakeControl47()
 Friend WithEvents tbCemBins As New FakeControl47()
 Friend WithEvents tbExtremeWeight As New FakeControl47()
 Friend WithEvents tbLoveThreshold As New FakeControl47()
 Friend WithEvents tbMatchingRatio As New FakeControl47()
 Friend WithEvents tbMaxIterations As New FakeControl47()
 Friend WithEvents tbOverlapBins As New FakeControl47()
 Friend WithEvents tbRidgePenalty As New FakeControl47()
 Friend WithEvents tbStrata As New FakeControl47()
 Friend WithEvents tbTolerance As New FakeControl47()
 Friend WithEvents tbTrimLower As New FakeControl47()
 Friend WithEvents tbTrimUpper As New FakeControl47()
 Friend Sub New()
AddHandler cbScoreMethod.SelectedIndexChanged, AddressOf OptionComboChanged
AddHandler cbRunMethod.SelectedIndexChanged, AddressOf OptionComboChanged
AddHandler cbEstimand.SelectedIndexChanged, AddressOf OptionComboChanged
AddHandler cbDistanceMetric.SelectedIndexChanged, AddressOf OptionComboChanged
AddHandler cbCaliperScale.SelectedIndexChanged, AddressOf OptionComboChanged
 End Sub
    Private Function BuildFitOptions() As PsmComprehensiveFitOptions
        Dim standard As New PsmOptions With {
            .ScoreMethod = ParseEnum(Of PsmScoreMethod)(cbScoreMethod.Text),
            .Estimand = ParseEnum(Of PsmEstimand)(cbEstimand.Text),
            .DistanceMetric = ParseEnum(Of PsmDistanceMetric)(cbDistanceMetric.Text),
            .MatchingRatio = Math.Max(1, FakeText47.ParseUiInteger(tbMatchingRatio.Text, "matching ratio")),
            .WithReplacement = chkWithReplacement.Checked,
            .CaliperScale = ParseEnum(Of PsmCaliperScale)(cbCaliperScale.Text),
            .Caliper = ParseOptionalDouble(tbCaliper.Text, Double.NaN),
            .MatchingOrder = ParseEnum(Of PsmMatchingOrder)(cbMatchingOrder.Text),
            .CommonSupport = ParseEnum(Of PsmCommonSupportMode)(cbCommonSupport.Text),
            .IncludeIntercept = ckIntercept.Checked,
            .StandardizeCovariates = chkStandardizeCovariates.Checked,
            .LogisticMaxIterations = Math.Max(1, FakeText47.ParseUiInteger(tbMaxIterations.Text, "maximum iterations")),
            .LogisticTolerance = SourceNumbers46.ParseUiDouble(tbTolerance.Text, "tolerance"),
            .LogisticRidgePenalty = Math.Max(0.0, SourceNumbers46.ParseUiDouble(tbRidgePenalty.Text, "ridge penalty")),
            .BalanceSmdThreshold = SourceNumbers46.ParseUiDouble(tbLoveThreshold.Text, "Love-plot/SMD threshold"),
            .SubclassificationStrata = Math.Max(2, FakeText47.ParseUiInteger(tbStrata.Text, "subclassification strata")),
            .TrimPropensityLower = SourceNumbers46.ParseUiDouble(tbTrimLower.Text, "lower propensity trim"),
            .TrimPropensityUpper = SourceNumbers46.ParseUiDouble(tbTrimUpper.Text, "upper propensity trim")
        }

        Dim fitOptions As New PsmComprehensiveFitOptions With {
            .StandardOptions = standard,
            .RunMethod = ParseEnum(Of PsmBackendRunMethod)(cbRunMethod.Text),
            .IncludeDoublyRobustEstimate = chkDoublyRobust.Checked,
            .IncludeOverlapDiagnostics = chkOverlapDiagnostics.Checked,
            .IncludeWeightDiagnostics = chkWeightDiagnostics.Checked,
            .IncludeLovePlotRows = chkLovePlot.Checked,
            .OverlapBinCount = Math.Max(2, FakeText47.ParseUiInteger(tbOverlapBins.Text, "overlap bins")),
            .LovePlotThreshold = SourceNumbers46.ParseUiDouble(tbLoveThreshold.Text, "Love-plot threshold"),
            .ExtremeWeightCutoff = SourceNumbers46.ParseUiDouble(tbExtremeWeight.Text, "extreme weight cutoff")
        }

        If fitOptions.RunMethod = PsmBackendRunMethod.CoarsenedExactMatching Then
            Dim bins As Integer = Math.Max(2, FakeText47.ParseUiInteger(tbCemBins.Text, "CEM quantile bins"))
            fitOptions.CoarseningSpec = PsmRequestSelection.CreateCemSpec(bins, standard)
        End If

        Return New PsmRequestSelection(fitOptions).CreateEffectiveOptions()
    End Function
    Private Shared Function ParseEnum(Of TEnum As Structure)(text As String) As TEnum
        Return CType([Enum].Parse(GetType(TEnum), text), TEnum)
    End Function
    Private Shared Function ParseOptionalDouble(text As String, defaultValue As Double) As Double
        If String.IsNullOrWhiteSpace(text) Then Return defaultValue
        Return SourceNumbers46.ParseUiDouble(text, "optional numeric value")
    End Function
    Private Sub PopulateEnumCombo(combo As FakeControl47, enumType As Type, defaultName As String)
        combo.DropDownStyle = 0
        combo.Items.Clear()
        For Each name As String In [Enum].GetNames(enumType)
            combo.Items.Add(name)
        Next
        Dim idx As Integer = combo.FindStringExact(defaultName)
        If idx >= 0 Then
            combo.SelectedIndex = idx
        ElseIf combo.Items.Count > 0 Then
            combo.SelectedIndex = 0
        End If
    End Sub
    Private Sub InitializeDefaultUiValues()
        Me.tbTolerance.Text = SourceNumbers46.FormatUiDouble(PsmSelectionDefaults.LogisticTolerance)
        Me.tbRidgePenalty.Text = SourceNumbers46.FormatUiDouble(PsmSelectionDefaults.LogisticRidgePenalty)
        Me.tbTrimLower.Text = SourceNumbers46.FormatUiDouble(PsmSelectionDefaults.LowerTrim)
        Me.tbTrimUpper.Text = SourceNumbers46.FormatUiDouble(PsmSelectionDefaults.UpperTrim)
        Me.tbLoveThreshold.Text = SourceNumbers46.FormatUiDouble(PsmSelectionDefaults.SmdThreshold)
        Me.tbMaxIterations.Text = PsmSelectionDefaults.MaximumIterations.ToString()
        Me.tbMatchingRatio.Text = PsmSelectionDefaults.MatchingRatio.ToString()
        Me.tbStrata.Text = PsmSelectionDefaults.Strata.ToString()
        Me.tbCemBins.Text = PsmSelectionDefaults.CemBins.ToString()
        Me.tbOverlapBins.Text = PsmSelectionDefaults.OverlapBins.ToString()
        Me.tbExtremeWeight.Text = SourceNumbers46.FormatUiDouble(PsmSelectionDefaults.ExtremeWeightCutoff)
        Me.tbCaliper.Text = String.Empty

        PopulateEnumCombo(Me.cbScoreMethod, GetType(PsmScoreMethod), PsmScoreMethod.LogisticRegression.ToString())
        PopulateEnumCombo(Me.cbRunMethod, GetType(PsmBackendRunMethod), PsmBackendRunMethod.StandardNearestNeighbor.ToString())
        PopulateEnumCombo(Me.cbEstimand, GetType(PsmEstimand), PsmEstimand.ATT.ToString())
        PopulateEnumCombo(Me.cbDistanceMetric, GetType(PsmDistanceMetric), PsmDistanceMetric.PropensityScore.ToString())
        PopulateEnumCombo(Me.cbCaliperScale, GetType(PsmCaliperScale), PsmCaliperScale.None.ToString())
        PopulateEnumCombo(Me.cbMatchingOrder, GetType(PsmMatchingOrder), PsmMatchingOrder.PropensityDescending.ToString())
        PopulateEnumCombo(Me.cbCommonSupport, GetType(PsmCommonSupportMode), PsmCommonSupportMode.None.ToString())

        Me.chkWithReplacement.Checked = False
        Me.chkStandardizeCovariates.Checked = True
        Me.chkDoublyRobust.Checked = True
        Me.chkOverlapDiagnostics.Checked = True
        Me.chkWeightDiagnostics.Checked = True
        Me.chkLovePlot.Checked = True
        Me.chkWriteDiagnostics.Checked = True
        Me.chkWriteMatches.Checked = True
    End Sub
    Private Sub SetOptionControlsForCurrentSelections()
        Dim scoreMethod As PsmScoreMethod = PsmScoreMethod.LogisticRegression
        Dim runMethod As PsmBackendRunMethod = PsmBackendRunMethod.StandardNearestNeighbor
        Dim estimand As PsmEstimand = PsmEstimand.ATT
        Dim caliperScale As PsmCaliperScale = PsmCaliperScale.None
        Dim distanceMetric As PsmDistanceMetric = PsmDistanceMetric.PropensityScore

        Try
            If Me.cbScoreMethod.SelectedItem IsNot Nothing Then scoreMethod = ParseEnum(Of PsmScoreMethod)(Me.cbScoreMethod.Text)
            If Me.cbRunMethod.SelectedItem IsNot Nothing Then runMethod = ParseEnum(Of PsmBackendRunMethod)(Me.cbRunMethod.Text)
            If Me.cbEstimand.SelectedItem IsNot Nothing Then estimand = ParseEnum(Of PsmEstimand)(Me.cbEstimand.Text)
            If Me.cbCaliperScale.SelectedItem IsNot Nothing Then caliperScale = ParseEnum(Of PsmCaliperScale)(Me.cbCaliperScale.Text)
            If Me.cbDistanceMetric.SelectedItem IsNot Nothing Then distanceMetric = ParseEnum(Of PsmDistanceMetric)(Me.cbDistanceMetric.Text)
        Catch
        End Try

        RefreshEstimandChoicesForRunMethod(runMethod)
        Try
            estimand = ParseEnum(Of PsmEstimand)(Me.cbEstimand.Text)
        Catch
            estimand = PsmEstimand.ATT
        End Try

        Dim intent As New PsmControlIntent(runMethod, estimand, scoreMethod)
        Dim supplied As Boolean = intent.Supplied
        Me.lbScore.Enabled = supplied
        Me.btAddScore.Enabled = supplied
        Me.btRemoveScore.Enabled = supplied
        Me.lblScore.Enabled = supplied
        'Keep the score-method, trimming, and common-support controls enabled even when
        'the user supplies propensity scores. Only logistic-regression iteration controls
        'are disabled, otherwise the user cannot switch back from Supplied mode.
        Me.grpIterOptions.Enabled = True
        Me.tbMaxIterations.Enabled = Not supplied
        Me.lblMaxIterations.Enabled = Not supplied
        Me.tbTolerance.Enabled = Not supplied
        Me.lblTolerance.Enabled = Not supplied
        Me.tbRidgePenalty.Enabled = Not supplied
        Me.lblRidgePenalty.Enabled = Not supplied

        Dim matching As Boolean = intent.Matching
        Dim nearestNeighbor As Boolean = intent.NearestNeighbor
        Dim optimalPair As Boolean = intent.OptimalPair

        If optimalPair Then
            Me.tbMatchingRatio.Text = "1"
            Me.chkWithReplacement.Checked = False
        End If

        If PsmControlIntent.NeedsCaliperDefault(distanceMetric, caliperScale) Then
            Dim idx As Integer = Me.cbCaliperScale.FindStringExact(PsmCaliperScale.StandardizedLogitPropensityScore.ToString())
            If idx >= 0 Then
                Me.cbCaliperScale.SelectedIndex = idx
                caliperScale = PsmCaliperScale.StandardizedLogitPropensityScore
            End If
            If String.IsNullOrWhiteSpace(Me.tbCaliper.Text) Then Me.tbCaliper.Text = SourceNumbers46.FormatUiDouble(PsmSelectionDefaults.MahalanobisCaliper)
        End If

        Me.tbMatchingRatio.Enabled = nearestNeighbor
        Me.lblMatchingRatio.Enabled = Me.tbMatchingRatio.Enabled
        Me.chkWithReplacement.Enabled = nearestNeighbor
        Me.cbDistanceMetric.Enabled = matching
        Me.lblDistanceMetric.Enabled = matching
        Me.cbMatchingOrder.Enabled = nearestNeighbor
        Me.lblMatchingOrder.Enabled = Me.cbMatchingOrder.Enabled
        Me.cbCaliperScale.Enabled = matching
        Me.lblCaliperScale.Enabled = matching
        Me.tbCaliper.Enabled = matching AndAlso caliperScale <> PsmCaliperScale.None
        Me.lblCaliper.Enabled = Me.tbCaliper.Enabled
        Me.tbStrata.Enabled = intent.Subclassification
        Me.lblStrata.Enabled = Me.tbStrata.Enabled
        Me.tbCemBins.Enabled = intent.Cem
        Me.lblCemBins.Enabled = Me.tbCemBins.Enabled
        Me.chkDoublyRobust.Enabled = intent.DoublyRobust
        If Not Me.chkDoublyRobust.Enabled Then Me.chkDoublyRobust.Checked = False
    End Sub
    Private Sub RefreshEstimandChoicesForRunMethod(runMethod As PsmBackendRunMethod)
        Dim current As String = If(Me.cbEstimand.SelectedItem Is Nothing, PsmEstimand.ATT.ToString(), Me.cbEstimand.Text)
        Dim allowed As PsmEstimand() = PsmControlIntent.Estimands(runMethod)
        Dim allowedNames As String() = allowed.Select(Function(e) e.ToString()).ToArray()
        If Me.cbEstimand.Items.Count = allowedNames.Length Then
            Dim same As Boolean = True
            For i As Integer = 0 To allowedNames.Length - 1
                If Not String.Equals(CStr(Me.cbEstimand.Items(i)), allowedNames(i), StringComparison.Ordinal) Then
                    same = False
                    Exit For
                End If
            Next
            If same Then Return
        End If

        Me.cbEstimand.Items.Clear()
        For Each name As String In allowedNames
            Me.cbEstimand.Items.Add(name)
        Next

        Dim idx As Integer = Me.cbEstimand.FindStringExact(current)
        If idx < 0 Then idx = Me.cbEstimand.FindStringExact(PsmEstimand.ATT.ToString())
        If idx < 0 AndAlso Me.cbEstimand.Items.Count > 0 Then idx = 0
        If idx >= 0 Then Me.cbEstimand.SelectedIndex = idx
    End Sub
    Private Sub OptionComboChanged(sender As Object, e As System.EventArgs)
        SetOptionControlsForCurrentSelections()
    End Sub
    Private Sub BeginPsmComputation()
        Me.btCalculate.Enabled = False
        Me.ProgressBar1.Style = FakeProgressStyle47.Marquee
        Me.lblProgress.Text = "Preparing propensity score matching..."
        Me.Pump.DoEvents()
    End Sub
    Private Sub FinishPsmComputation(message As String)
        Me.ProgressBar1.Style = FakeProgressStyle47.Continuous
        Me.ProgressBar1.Value = 0
        Me.lblProgress.Text = message
        Me.Pump.DoEvents()
    End Sub
    Private Sub EndPsmComputation()
        Try
            Me.ProgressBar1.Style = FakeProgressStyle47.Continuous
            Me.btCalculate.Enabled = True
            Me.Pump.DoEvents()
        Catch
        End Try
    End Sub
End Class
Friend Partial Class Ui21ControlChartsProjection47
 Friend Tag As HelpTopic
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend WithEvents lblLastPhaseIPoint As New FakeControl47()
 Friend WithEvents rbPhaseIThenPhaseII As New FakeControl47()
 Friend WithEvents rbSinglePhaseI As New FakeControl47()
 Friend WithEvents spinLastPhaseIPoint As New FakeControl47()
    Private Sub rbPhase_CheckedChanged(sender As Object, e As System.EventArgs) Handles rbSinglePhaseI.CheckedChanged, rbPhaseIThenPhaseII.CheckedChanged
        UpdateQuickPhaseControls()
    End Sub
    Private Sub UpdateQuickPhaseControls()
        spinLastPhaseIPoint.Enabled = rbPhaseIThenPhaseII.Checked
        lblLastPhaseIPoint.Enabled = rbPhaseIThenPhaseII.Checked
    End Sub
    Private Shared Sub SetRoleEnabled(list As FakeControl47,
                                      label As FakeControl47,
                                      addButton As FakeControl47,
                                      removeButton As FakeControl47,
                                      enabled As Boolean)
        If Not enabled Then
            ' Disabled assignments are ignored by request construction. Remove
            ' them immediately so the form cannot display stale roles from the
            ' previously selected chart or data layout.
            list.Items.Clear()
        End If

        list.Enabled = enabled
        label.Enabled = enabled
        addButton.Enabled = enabled
        removeButton.Enabled = enabled
    End Sub
    Private Shared Function IsAttributeChart(chartType As SpcChartType) As Boolean
        Return SpcRequestPreparation.IsAttributeChart(chartType)
    End Function
    Private Shared Function IsSubgroupChart(chartType As SpcChartType) As Boolean
        Return SpcRequestPreparation.IsSubgroupChart(chartType)
    End Function
    Private Shared Function IsTimeWeightedChart(chartType As SpcChartType) As Boolean
        Return SpcRequestPreparation.IsTimeWeightedChart(chartType)
    End Function
    Private Shared Function ChartCanUseSpecificationLines(chartType As SpcChartType) As Boolean
        Return SpcRequestPreparation.ChartCanUseSpecificationLines(chartType)
    End Function
End Class
Friend Partial Class Ui22MultivariateControlChartsProjection47
 Friend Tag As HelpTopic
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend WithEvents cbDiagnosticsScope As New FakeControl47()
 Friend WithEvents chkWriteDiagnostics As New FakeControl47()
 Friend WithEvents lblDiagnosticsScope As New FakeControl47()
 Friend WithEvents rbPcaFixedComponents As New FakeControl47()
 Friend WithEvents rbPcaVarianceSelection As New FakeControl47()
 Friend WithEvents spinPcaComponentCount As New FakeControl47()
 Friend WithEvents spinPcaCumulativeVariance As New FakeControl47()
    Private Sub rbPca_CheckedChanged(sender As Object, e As System.EventArgs) Handles rbPcaVarianceSelection.CheckedChanged, rbPcaFixedComponents.CheckedChanged
        UpdatePcaSelectionControls()
    End Sub
    Private Sub UpdatePcaSelectionControls()
        spinPcaCumulativeVariance.Enabled = rbPcaVarianceSelection.Checked
        spinPcaComponentCount.Enabled = rbPcaFixedComponents.Checked
    End Sub
    Private Sub chkWriteDiagnostics_CheckedChanged(sender As Object, e As System.EventArgs) Handles chkWriteDiagnostics.CheckedChanged
        UpdateDiagnosticsControls()
    End Sub
    Private Sub UpdateDiagnosticsControls()
        cbDiagnosticsScope.Enabled = chkWriteDiagnostics.Checked
        lblDiagnosticsScope.Enabled = chkWriteDiagnostics.Checked
    End Sub
    Private Shared Sub SetRoleEnabled(list As FakeControl47, label As FakeControl47,
                                      addButton As FakeControl47, removeButton As FakeControl47,
                                      enabled As Boolean)
        If Not enabled Then list.Items.Clear()
        list.Enabled = enabled
        label.Enabled = enabled
        addButton.Enabled = enabled
        removeButton.Enabled = enabled
    End Sub
End Class
Friend Partial Class Ui4KMandLogRankProjection47
 Friend Tag As HelpTopic
 Friend ReadOnly Pump As New FakePump47()
 Friend ReadOnly Bridge As New FakeRegressionBridge47()
 Friend Closes As Integer
 Friend Sub Close()
 Closes += 1
 End Sub
 Friend WithEvents RefEdit1_SurvivalTime As New FakeRefEdit46()
 Friend WithEvents RefEdit2_Censor As New FakeRefEdit46()
 Friend WithEvents RefEdit3_GroupID As New FakeRefEdit46()
 Friend WithEvents RefEdit4_StrataID As New FakeRefEdit46()
 Friend WithEvents RefEditOutput As New FakeRefEdit46()
 Friend WithEvents TabMultipage As New FakeControl47()
 Friend WithEvents optOutputRange As New FakeControl47()
 Friend WithEvents optWorkbook As New FakeControl47()
 Friend WithEvents optWorksheet As New FakeControl47()
    Private Function checkInputs() As Boolean
        Dim bOut As Boolean
        'check input data------------------------------
        If FakeRanges46.CheckRefEdit(Me.RefEdit1_SurvivalTime.Address, True) Then
            Me.TabMultipage.SelectedIndex = 0
            FakeRanges46.RefEditReset(Me.RefEdit1_SurvivalTime)
            bOut = True
        End If

        If FakeRanges46.CheckRefEdit(Me.RefEdit2_Censor.Address, True) Then
            Me.TabMultipage.SelectedIndex = 0
            FakeRanges46.RefEditReset(Me.RefEdit2_Censor)
            bOut = True
        End If

        'It's optional for KM plot
        If (Me.Tag = HelpTopic.KaplanMeierPlot And Me.RefEdit3_GroupID.Address <> String.Empty) Or
            Me.Tag = HelpTopic.LogrankTest Then
            If FakeRanges46.CheckRefEdit(Me.RefEdit3_GroupID.Address, True) Then
                Me.TabMultipage.SelectedIndex = 0
                FakeRanges46.RefEditReset(Me.RefEdit3_GroupID)
                bOut = True
            End If
        End If

        'It's optional for Logrank and not applicable for KM
        If Me.Tag = HelpTopic.LogrankTest And Me.RefEdit4_StrataID.Address <> String.Empty Then
            If FakeRanges46.CheckRefEdit(Me.RefEdit4_StrataID.Address, True) Then
                Me.TabMultipage.SelectedIndex = 0
                FakeRanges46.RefEditReset(Me.RefEdit4_StrataID)
                bOut = True
            End If
        End If

        If Me.optOutputRange.Checked Then
            If FakeRanges46.CheckRefEdit(Me.RefEditOutput.Address) Then
                Me.TabMultipage.SelectedIndex = 0
                FakeRanges46.RefEditReset(Me.RefEditOutput)
                bOut = True
            End If
        End If

        Return bOut
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
End Class
End Namespace
