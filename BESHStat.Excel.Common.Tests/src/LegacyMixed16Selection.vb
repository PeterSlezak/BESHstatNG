Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports regression = BESHStatNG.regression

' Frozen prechange form methods. Only control reads/visibility are adapted;
' original Core calls, parser order, error strings and comparison branches remain.
Public NotInheritable Class LegacyMixed16Selection
    Private ReadOnly _options As MixedModelReportOptions
    Public Sub New(options As MixedModelReportOptions)
        _options = options.Copy()
    End Sub
    Private Shared Function SelectedText(value As String, fallback As String) As String
        Return If(String.IsNullOrWhiteSpace(value), fallback, value)
    End Function
    Private Const MMRM_PROGRESS_REFRESH_INTERVAL_MS As Double = 100.0
    Private Const MMRM_GROUP_AUTO As String = "(Auto)"
    Private Const MMRM_GROUP_NONE As String = "(None)"
    Private Const MMRM_BASELINE_SMALLEST As String = "(Smallest)"
    Private Const MMRM_CONTROL_FIRST As String = "(First)"
    Private Const MMRM_MODE_NONE As String = "None"
    Private Const MMRM_MODE_PAIRWISE As String = "Pairwise among group levels"
    Private Const MMRM_MODE_CONTROL As String = "Each group vs control"
    Private Const MMRM_MODE_SELECTED As String = "Selected comparison only"
    Private Const MMRM_DIR_HIGHER_MINUS_LOWER As String = "Higher level - lower level"
    Private Const MMRM_DIR_TREATMENT_MINUS_CONTROL As String = "Treatment - control"
    Private Const MMRM_DIR_CONTROL_MINUS_TREATMENT As String = "Control - treatment"
    Private Const MMRM_LSMEANS_OBSERVED_GRID As String = "Observed design grid"
    Private Const MMRM_LSMEANS_REFERENCE_GRID As String = "Reference grid"
    Private Const MMRM_RG_WEIGHT_EQUAL As String = "Equal class-cell weights"
    Private Const MMRM_RG_WEIGHT_OBSERVED As String = "Observed class-cell weights"
    Private Const MMRM_RG_COVARIATE_MEANS As String = "Continuous covariates at observed means"
    Private Const MMRM_RG_COVARIATE_ZERO As String = "Continuous covariates at 0"
    Private Const MMRM_MULT_NONE As String = "None"
    Private Const MMRM_MULT_BONFERRONI As String = "Bonferroni"
    Private Const MMRM_MULT_HOLM As String = "Holm"
    Private Const MMRM_MULT_SIDAK As String = "Sidak"
    Private Const MMRM_OPT_AI As String = "AI/Fisher scoring (default)"
    Private Const MMRM_OPT_BFGS_AUTO As String = "Projected BFGS (auto gradient)"
    Private Const MMRM_OPT_BFGS_ANALYTIC As String = "Projected BFGS (analytic gradient)"
    Private Const MMRM_OPT_BFGS_NUMERICAL As String = "Projected BFGS (finite-difference gradient)"
    Private Const MMRM_GRAD_AUTO As String = "Auto (analytic where available)"
    Private Const MMRM_GRAD_ANALYTIC As String = "Analytic score"
    Private Const MMRM_GRAD_VALIDATE As String = "Analytic score + finite-difference validation"
    Private Const MMRM_GRAD_NUMERICAL As String = "Numerical finite difference"
    Public Function ResolveSelectedBaselineVisit(visit() As Double) As Double
        Dim visits() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
        If visits Is Nothing OrElse visits.Length = 0 Then Return Double.NaN

        Dim sel As String = SelectedText(_options.BaselineSelection, MMRM_BASELINE_SMALLEST)
        If String.Equals(sel, MMRM_BASELINE_SMALLEST, StringComparison.OrdinalIgnoreCase) Then Return visits(0)

        For Each v As Double In visits
            If String.Equals(regression.MixedModelPostEstimation.FormatProfileValue(v), sel, StringComparison.OrdinalIgnoreCase) Then Return v
        Next

        Dim parsed As Double
        If Double.TryParse(sel, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, parsed) Then Return parsed
        If Double.TryParse(sel, parsed) Then Return parsed

        Return visits(0)
    End Function
    Public Function ResolveSelectedControlLevel(groupValues() As Double) As Double
        Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
        If groups Is Nothing OrElse groups.Length = 0 Then Return Double.NaN

        Dim sel As String = SelectedText(_options.ControlSelection, MMRM_CONTROL_FIRST)
        If String.Equals(sel, MMRM_CONTROL_FIRST, StringComparison.OrdinalIgnoreCase) Then Return groups(0)

        For Each g As Double In groups
            If String.Equals(regression.MixedModelPostEstimation.FormatProfileValue(g), sel, StringComparison.OrdinalIgnoreCase) Then Return g
        Next

        Dim parsed As Double
        If Double.TryParse(sel, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, parsed) Then Return parsed
        If Double.TryParse(sel, parsed) Then Return parsed

        Return groups(0)
    End Function
    Public Function ResolveSelectedComparisonLevel(groupLevels As Double(),
                                                    controlLevel As Double,
                                                    ByRef comparisonLevel As Double) As Boolean

        comparisonLevel = Double.NaN

        If groupLevels Is Nothing OrElse groupLevels.Length = 0 Then Return False

        Dim selectedText As String = If(_options.ComparisonSelection Is Nothing, "(Auto)", _options.ComparisonSelection.Trim())

        If selectedText.Length = 0 OrElse selectedText.Equals("(Auto)", StringComparison.OrdinalIgnoreCase) Then
            For Each g As Double In groupLevels
                If Not regression.MixedModelPostEstimation.NearlyEqual(g, controlLevel) Then
                    comparisonLevel = g
                    Return True
                End If
            Next
            Return False
        End If

        Dim parsed As Double
        If Double.TryParse(selectedText, Globalization.NumberStyles.Any, Globalization.CultureInfo.CurrentCulture, parsed) _
           OrElse Double.TryParse(selectedText, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, parsed) Then

            If regression.MixedModelPostEstimation.NearlyEqual(parsed, controlLevel) Then Return False

            comparisonLevel = parsed
            Return True
        End If

        For Each g As Double In groupLevels
            If String.Equals(regression.MixedModelPostEstimation.FormatProfileValue(g), selectedText, StringComparison.OrdinalIgnoreCase) Then
                If regression.MixedModelPostEstimation.NearlyEqual(g, controlLevel) Then Return False
                comparisonLevel = g
                Return True
            End If
        Next

        Return False
    End Function
    Public Function BuildMMRMVisitGroupDifferencesTableControlled(result As regression.MixedModelResult,
                                                               x(,) As Double,
                                                               visit() As Double,
                                                               groupValues() As Double,
                                                               groupName As String,
                                                               alpha As Double,
                                                               contrastMode As String,
                                                               controlLevel As Double,
                                                               direction As String) As ResultTable
        Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)

        Dim comparisonLevel As Double = Double.NaN
        If String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
            If Not ResolveSelectedComparisonLevel(groups, controlLevel, comparisonLevel) Then
                Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
            End If
        End If

        Return regression.MMRMPostEstimation.BuildVisitGroupDifferencesTableControlled(result:=result,
                                                                                   x:=x,
                                                                                   visit:=visit,
                                                                                   groupValues:=groupValues,
                                                                                   groupName:=groupName,
                                                                                   alpha:=alpha,
                                                                                   contrastMode:=contrastMode,
                                                                                   controlLevel:=controlLevel,
                                                                                   comparisonLevel:=comparisonLevel,
                                                                                   direction:=direction)
    End Function
    Public Function BuildMMRMDifferenceInChangeFromBaselineTableControlled(result As regression.MixedModelResult,
                                                                        x(,) As Double,
                                                                        visit() As Double,
                                                                        groupValues() As Double,
                                                                        groupName As String,
                                                                        baseline As Double,
                                                                        alpha As Double,
                                                                        contrastMode As String,
                                                                        controlLevel As Double,
                                                                        direction As String) As ResultTable
        Dim groups() As Double = regression.MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)

        Dim comparisonLevel As Double = Double.NaN
        If String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
            If Not ResolveSelectedComparisonLevel(groups, controlLevel, comparisonLevel) Then
                Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
            End If
        End If

        Return regression.MMRMPostEstimation.BuildDifferenceInChangeFromBaselineTableControlled(result:=result,
                                                                                           x:=x,
                                                                                           visit:=visit,
                                                                                           groupValues:=groupValues,
                                                                                           groupName:=groupName,
                                                                                           baseline:=baseline,
                                                                                           alpha:=alpha,
                                                                                           contrastMode:=contrastMode,
                                                                                           controlLevel:=controlLevel,
                                                                                           comparisonLevel:=comparisonLevel,
                                                                                           direction:=direction)
    End Function
    Public Function BuildMMRMReferenceGridGroupContrastsTable(result As regression.MixedModelResult,
                                                          rows As List(Of regression.MixedModelReferenceGridRow),
                                                          spec As regression.MixedModelReferenceGridSpec,
                                                          groupKey As String,
                                                          contrastMode As String,
                                                          controlLevel As Double,
                                                          contrastDirection As String) As ResultTable
        If rows Is Nothing OrElse rows.Count = 0 Then Return Nothing

        Dim groupProfileName As String = Global.BESHStatNG.regression.MixedModelReferenceGridService.NormalizeProfileName(groupKey)

        If String.Equals(contrastMode, MMRM_MODE_PAIRWISE, StringComparison.OrdinalIgnoreCase) Then
            Return regression.MixedModelReferenceGridService.BuildPairwiseContrastsByFactor(rows,
                                                                                       result,
                                                                                       spec,
                                                                                       groupProfileName,
                                                                                       "Reference-grid group contrasts")
        End If

        Dim groupLevels() As Double =
        regression.MixedModelReferenceGridService.GetLevelsFromRows(rows, groupProfileName)

        Dim comparisonLevel As Double = Double.NaN
        Dim useSingleComparison As Boolean = False

        If String.Equals(contrastMode, MMRM_MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
            If Not ResolveSelectedComparisonLevel(groupLevels, controlLevel, comparisonLevel) Then
                Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
            End If

            useSingleComparison = True
        End If

        Return regression.MixedModelReferenceGridService.BuildContrastsAgainstControlByFactor(
                    rows:=rows,
                    result:=result,
                    spec:=spec,
                    factorName:=groupProfileName,
                    controlLevel:=controlLevel,
                    comparisonLevel:=comparisonLevel,
                    useSingleComparison:=useSingleComparison,
                    direction:=contrastDirection,
                    treatmentMinusControlText:=MMRM_DIR_TREATMENT_MINUS_CONTROL,
                    controlMinusTreatmentText:=MMRM_DIR_CONTROL_MINUS_TREATMENT,
                    title:="Reference-grid group contrasts")
    End Function
End Class
