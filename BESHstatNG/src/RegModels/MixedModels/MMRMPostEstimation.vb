Option Explicit On
Option Strict On

Namespace regression

    ''' <summary>
    ''' Reusable MMRM-specific post-estimation table builders.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module contains MMRM-specific LS-means and contrast builders that were
    ''' originally implemented as private helpers in <c>Ui18MMRM</c>.
    ''' </para>
    ''' <para>
    ''' It intentionally has no WinForms dependency, so the same calculations can be
    ''' reused by:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>the MMRM GUI,</description></item>
    '''   <item><description>future MMRM UDFs, and</description></item>
    '''   <item><description>unit/reference tests.</description></item>
    ''' </list>
    ''' <para>
    ''' The estimates are currently observed-design-grid estimates.  That means each
    ''' displayed LS-mean uses the average fitted fixed-effect design row observed in
    ''' the requested profile cell.  A later reference-grid implementation can add
    ''' explicit user-specified covariate values and CLASS-style marginal weighting.
    ''' </para>
    ''' </remarks>
    Public Module MMRMPostEstimation

        Public Const MODE_NONE As String = "None"
        Public Const MODE_PAIRWISE As String = "Pairwise among group levels"
        Public Const MODE_CONTROL As String = "Each group vs control"
        Public Const MODE_SELECTED As String = "Selected comparison only"

        Public Const DIR_HIGHER_MINUS_LOWER As String = "Higher level - lower level"
        Public Const DIR_TREATMENT_MINUS_CONTROL As String = "Treatment - control"
        Public Const DIR_CONTROL_MINUS_TREATMENT As String = "Control - treatment"


        Public Function BuildEstimatedMeansByVisitTable(result As MixedModelResult,
                                                        x(,) As Double,
                                                        visit() As Double,
                                                        alpha As Double,
                                                        Optional rowMask() As Boolean = Nothing,
                                                        Optional profileDescription As String = Nothing) As Global.BESHStatNG.ResultTable
            If result Is Nothing OrElse x Is Nothing OrElse visit Is Nothing Then Return Nothing

            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            If visits Is Nothing OrElse visits.Length = 0 Then Return Nothing

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())
            Dim counts As New List(Of Integer)

            For Each v As Double In visits
                Dim l() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, Nothing, v, Double.NaN, rowMask)
                If l IsNot Nothing Then
                    rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v))
                    lRows.Add(l)
                    counts.Add(MixedModelPostEstimation.CountProfileRows(visit, Nothing, v, Double.NaN, rowMask))
                End If
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearEstimateResultTable(
                title:="Estimated marginal means by visit",
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                counts:=counts,
                result:=result,
                alpha:=alpha,
                footnote:=BuildEstimatedMeansFootnote(profileDescription))
        End Function

        Private Function BuildEstimatedMeansFootnote(profileDescription As String) As String
            Dim baseText As String = "Estimated means are computed as L*beta, where L is the average fitted fixed-effect design row among observations at the displayed visit."
            If Not String.IsNullOrWhiteSpace(profileDescription) Then
                baseText &= " The design rows are additionally restricted to the supplied profile: " & profileDescription.Trim() & "."
            End If

            Return baseText & " This is an observed-design-grid estimate; a later LS-means UI can add user-defined reference grids and pairwise contrasts."
        End Function

        Public Function BuildEstimatedMeansByVisitAndGroupTable(result As MixedModelResult,
                                                                x(,) As Double,
                                                                visit() As Double,
                                                                groupValues() As Double,
                                                                groupName As String,
                                                                alpha As Double) As Global.BESHStatNG.ResultTable
            If result Is Nothing OrElse x Is Nothing OrElse visit Is Nothing OrElse groupValues Is Nothing Then Return Nothing
            If groupValues.Length <> visit.Length Then Return Nothing

            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            Dim groups() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
            If visits Is Nothing OrElse groups Is Nothing OrElse visits.Length = 0 OrElse groups.Length < 2 Then Return Nothing

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())
            Dim counts As New List(Of Integer)
            Dim groupBaseName As String = Global.BESHStatNG.RegressionDesignCore.GetCoefBaseName(groupName)

            For Each v As Double In visits
                For Each g As Double In groups
                    Dim l() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g, Nothing)
                    If l IsNot Nothing Then
                        rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) & ", " &
                                 groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(g))
                        lRows.Add(l)
                        counts.Add(MixedModelPostEstimation.CountProfileRows(visit, groupValues, v, g, Nothing))
                    End If
                Next
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearEstimateResultTable(
                title:="Estimated marginal means by visit and " & groupBaseName,
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                counts:=counts,
                result:=result,
                alpha:=alpha,
                footnote:="Estimated means are computed as L*beta, where L is the average fitted fixed-effect design row among observations at the displayed visit/profile.  This is an observed-design-grid estimate.")
        End Function


        Public Function BuildVisitGroupDifferencesTable(result As MixedModelResult,
                                                        x(,) As Double,
                                                        visit() As Double,
                                                        groupValues() As Double,
                                                        groupName As String,
                                                        alpha As Double) As Global.BESHStatNG.ResultTable
            Return BuildVisitGroupDifferencesTableControlled(result:=result,
                                                             x:=x,
                                                             visit:=visit,
                                                             groupValues:=groupValues,
                                                             groupName:=groupName,
                                                             alpha:=alpha,
                                                             contrastMode:=MODE_PAIRWISE,
                                                             controlLevel:=Double.NaN,
                                                             comparisonLevel:=Double.NaN,
                                                             direction:=DIR_HIGHER_MINUS_LOWER)
        End Function


        Public Function BuildVisitGroupDifferencesTableControlled(result As MixedModelResult,
                                                                  x(,) As Double,
                                                                  visit() As Double,
                                                                  groupValues() As Double,
                                                                  groupName As String,
                                                                  alpha As Double,
                                                                  contrastMode As String,
                                                                  controlLevel As Double,
                                                                  comparisonLevel As Double,
                                                                  direction As String) As Global.BESHStatNG.ResultTable
            If result Is Nothing OrElse x Is Nothing OrElse visit Is Nothing OrElse groupValues Is Nothing Then Return Nothing
            If groupValues.Length <> visit.Length Then Return Nothing

            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            Dim groups() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
            If visits Is Nothing OrElse groups Is Nothing OrElse visits.Length = 0 OrElse groups.Length < 2 Then Return Nothing

            Dim groupBaseName As String = Global.BESHStatNG.RegressionDesignCore.GetCoefBaseName(groupName)

            If String.Equals(contrastMode, MODE_NONE, StringComparison.OrdinalIgnoreCase) Then Return Nothing

            If String.Equals(contrastMode, MODE_PAIRWISE, StringComparison.OrdinalIgnoreCase) Then
                Return BuildPairwiseVisitGroupDifferences(result, x, visit, groupValues, groupBaseName, alpha)
            End If

            If Not AppInfrastructure.IsFinite(controlLevel) Then controlLevel = groups(0)

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())

            If String.Equals(contrastMode, MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
                Dim comp As Double
                If Not ResolveComparisonLevel(groups, controlLevel, comparisonLevel, comp) Then
                    Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
                End If

                Dim firstLevel As Double
                Dim secondLevel As Double
                ResolveDirectedGroupPair(controlLevel, comp, direction, firstLevel, secondLevel)

                For Each v As Double In visits
                    Dim lFirst() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, firstLevel, Nothing)
                    Dim lSecond() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, secondLevel, Nothing)

                    If lFirst Is Nothing OrElse lSecond Is Nothing Then Continue For

                    rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) & ": " &
                             groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(firstLevel) & " - " &
                             groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(secondLevel))

                    lRows.Add(Matrix.M_SUB(lFirst, lSecond))
                Next

                If lRows.Count = 0 Then Return Nothing

                Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                    title:="MMRM selected group difference by visit",
                    rowLabels:=rows.ToArray(),
                    lRows:=lRows,
                    result:=result,
                    alpha:=alpha,
                    footnote:="Comparison/treatment level, control/reference level, and contrast direction are selected by the caller.")
            End If

            ' Each group vs control.
            For Each v As Double In visits
                Dim lControl() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, controlLevel, Nothing)
                If lControl Is Nothing Then Continue For

                For Each g As Double In groups
                    If MixedModelPostEstimation.NearlyEqual(g, controlLevel) Then Continue For

                    Dim lTreatment() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g, Nothing)
                    If lTreatment Is Nothing Then Continue For

                    rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) & ": " &
                             MixedModelPostEstimation.DirectedComparisonLabel(groupBaseName, g, controlLevel, direction, DIR_TREATMENT_MINUS_CONTROL, DIR_CONTROL_MINUS_TREATMENT))

                    lRows.Add(MixedModelPostEstimation.MakeDirectedDifference(lTreatment, lControl, direction, DIR_TREATMENT_MINUS_CONTROL, DIR_CONTROL_MINUS_TREATMENT))
                Next
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                title:="MMRM group differences vs control by visit",
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                result:=result,
                alpha:=alpha,
                footnote:="Control/reference level and contrast direction are selected by the caller.")
        End Function


        Private Function BuildPairwiseVisitGroupDifferences(result As MixedModelResult,
                                                            x(,) As Double,
                                                            visit() As Double,
                                                            groupValues() As Double,
                                                            groupBaseName As String,
                                                            alpha As Double) As Global.BESHStatNG.ResultTable
            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            Dim groups() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())

            For Each v As Double In visits
                For g1Index As Integer = 0 To groups.Length - 2
                    For g2Index As Integer = g1Index + 1 To groups.Length - 1
                        Dim g1 As Double = groups(g1Index)
                        Dim g2 As Double = groups(g2Index)
                        Dim l1() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g1, Nothing)
                        Dim l2() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g2, Nothing)
                        If l1 Is Nothing OrElse l2 Is Nothing Then Continue For

                        rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) & ": " &
                                 groupBaseName & " " & MixedModelPostEstimation.FormatProfileValue(g2) &
                                 " - " & MixedModelPostEstimation.FormatProfileValue(g1))
                        lRows.Add(Matrix.M_SUB(l2, l1))
                    Next
                Next
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                title:="MMRM pairwise group differences by visit",
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                result:=result,
                alpha:=alpha,
                footnote:="Contrasts are computed from observed-design LS-means as Ldiff*beta.  Group levels are ordered numerically; each row reports the higher-index level minus the lower-index level within the displayed visit.")
        End Function


        Public Function BuildChangeFromBaselineTable(result As MixedModelResult,
                                                     x(,) As Double,
                                                     visit() As Double,
                                                     alpha As Double) As Global.BESHStatNG.ResultTable
            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            If visits Is Nothing OrElse visits.Length = 0 Then Return Nothing

            Return BuildChangeFromBaselineTableControlled(result, x, visit, visits(0), alpha)
        End Function


        Public Function BuildChangeFromBaselineTableControlled(result As MixedModelResult,
                                                               x(,) As Double,
                                                               visit() As Double,
                                                               baseline As Double,
                                                               alpha As Double) As Global.BESHStatNG.ResultTable
            If result Is Nothing OrElse x Is Nothing OrElse visit Is Nothing Then Return Nothing

            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            If visits Is Nothing OrElse visits.Length < 2 Then Return Nothing
            If Not AppInfrastructure.IsFinite(baseline) Then baseline = visits(0)

            Dim lBase() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, Nothing, baseline, Double.NaN, Nothing)
            If lBase Is Nothing Then Return Nothing

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())

            For Each v As Double In visits
                If MixedModelPostEstimation.NearlyEqual(v, baseline) Then Continue For

                Dim lVisit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, Nothing, v, Double.NaN, Nothing)
                If lVisit Is Nothing Then Continue For

                rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) &
                         " - baseline visit " & MixedModelPostEstimation.FormatProfileValue(baseline))
                lRows.Add(Matrix.M_SUB(lVisit, lBase))
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                title:="MMRM change from baseline by visit",
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                result:=result,
                alpha:=alpha,
                footnote:="Baseline is selected by the caller.  Changes are computed from observed-design LS-means as (Lvisit - Lbaseline)*beta.")
        End Function


        Public Function BuildChangeFromBaselineByGroupTable(result As MixedModelResult,
                                                            x(,) As Double,
                                                            visit() As Double,
                                                            groupValues() As Double,
                                                            groupName As String,
                                                            alpha As Double) As Global.BESHStatNG.ResultTable
            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            If visits Is Nothing OrElse visits.Length = 0 Then Return Nothing

            Return BuildChangeFromBaselineByGroupTableControlled(result, x, visit, groupValues, groupName, visits(0), alpha)
        End Function


        Public Function BuildChangeFromBaselineByGroupTableControlled(result As MixedModelResult,
                                                                      x(,) As Double,
                                                                      visit() As Double,
                                                                      groupValues() As Double,
                                                                      groupName As String,
                                                                      baseline As Double,
                                                                      alpha As Double) As Global.BESHStatNG.ResultTable
            If result Is Nothing OrElse x Is Nothing OrElse visit Is Nothing OrElse groupValues Is Nothing Then Return Nothing
            If groupValues.Length <> visit.Length Then Return Nothing

            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            Dim groups() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
            If visits Is Nothing OrElse groups Is Nothing OrElse visits.Length < 2 OrElse groups.Length < 1 Then Return Nothing
            If Not AppInfrastructure.IsFinite(baseline) Then baseline = visits(0)

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())
            Dim groupBaseName As String = Global.BESHStatNG.RegressionDesignCore.GetCoefBaseName(groupName)

            For Each g As Double In groups
                Dim lBase() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, baseline, g, Nothing)
                If lBase Is Nothing Then Continue For

                For Each v As Double In visits
                    If MixedModelPostEstimation.NearlyEqual(v, baseline) Then Continue For

                    Dim lVisit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g, Nothing)
                    If lVisit Is Nothing Then Continue For

                    rows.Add(groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(g) &
                             ": visit " & MixedModelPostEstimation.FormatProfileValue(v) &
                             " - baseline visit " & MixedModelPostEstimation.FormatProfileValue(baseline))
                    lRows.Add(Matrix.M_SUB(lVisit, lBase))
                Next
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                title:="MMRM change from baseline by visit and " & groupBaseName,
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                result:=result,
                alpha:=alpha,
                footnote:="Baseline is selected by the caller.  Changes are computed within each group level from observed-design LS-means.")
        End Function


        Public Function BuildDifferenceInChangeFromBaselineTable(result As MixedModelResult,
                                                                 x(,) As Double,
                                                                 visit() As Double,
                                                                 groupValues() As Double,
                                                                 groupName As String,
                                                                 alpha As Double) As Global.BESHStatNG.ResultTable
            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            If visits Is Nothing OrElse visits.Length = 0 Then Return Nothing

            Return BuildDifferenceInChangeFromBaselineTableControlled(result:=result,
                                                                      x:=x,
                                                                      visit:=visit,
                                                                      groupValues:=groupValues,
                                                                      groupName:=groupName,
                                                                      baseline:=visits(0),
                                                                      alpha:=alpha,
                                                                      contrastMode:=MODE_PAIRWISE,
                                                                      controlLevel:=Double.NaN,
                                                                      comparisonLevel:=Double.NaN,
                                                                      direction:=DIR_HIGHER_MINUS_LOWER)
        End Function


        Public Function BuildDifferenceInChangeFromBaselineTableControlled(result As MixedModelResult,
                                                                           x(,) As Double,
                                                                           visit() As Double,
                                                                           groupValues() As Double,
                                                                           groupName As String,
                                                                           baseline As Double,
                                                                           alpha As Double,
                                                                           contrastMode As String,
                                                                           controlLevel As Double,
                                                                           comparisonLevel As Double,
                                                                           direction As String) As Global.BESHStatNG.ResultTable
            If result Is Nothing OrElse x Is Nothing OrElse visit Is Nothing OrElse groupValues Is Nothing Then Return Nothing
            If groupValues.Length <> visit.Length Then Return Nothing

            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            Dim groups() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)
            If visits Is Nothing OrElse groups Is Nothing OrElse visits.Length < 2 OrElse groups.Length < 2 Then Return Nothing
            If Not AppInfrastructure.IsFinite(baseline) Then baseline = visits(0)

            Dim groupBaseName As String = Global.BESHStatNG.RegressionDesignCore.GetCoefBaseName(groupName)

            If String.Equals(contrastMode, MODE_NONE, StringComparison.OrdinalIgnoreCase) Then Return Nothing

            If String.Equals(contrastMode, MODE_PAIRWISE, StringComparison.OrdinalIgnoreCase) Then
                Return BuildPairwiseDifferenceInChange(result, x, visit, groupValues, groupBaseName, baseline, alpha)
            End If

            If Not AppInfrastructure.IsFinite(controlLevel) Then controlLevel = groups(0)

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())

            If String.Equals(contrastMode, MODE_SELECTED, StringComparison.OrdinalIgnoreCase) Then
                Dim comp As Double
                If Not ResolveComparisonLevel(groups, controlLevel, comparisonLevel, comp) Then
                    Throw New ApplicationException("Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level.")
                End If

                Dim firstLevel As Double
                Dim secondLevel As Double
                ResolveDirectedGroupPair(controlLevel, comp, direction, firstLevel, secondLevel)

                For Each v As Double In visits
                    If MixedModelPostEstimation.NearlyEqual(v, baseline) Then Continue For

                    Dim lFirstBase() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, baseline, firstLevel, Nothing)
                    Dim lFirstVisit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, firstLevel, Nothing)
                    Dim lSecondBase() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, baseline, secondLevel, Nothing)
                    Dim lSecondVisit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, secondLevel, Nothing)

                    If lFirstBase Is Nothing OrElse lFirstVisit Is Nothing OrElse lSecondBase Is Nothing OrElse lSecondVisit Is Nothing Then Continue For

                    Dim changeFirst() As Double = Matrix.M_SUB(lFirstVisit, lFirstBase)
                    Dim changeSecond() As Double = Matrix.M_SUB(lSecondVisit, lSecondBase)

                    rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) &
                             " vs baseline " & MixedModelPostEstimation.FormatProfileValue(baseline) & ": Δ(" &
                             groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(firstLevel) & " - " &
                             groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(secondLevel) & ")")

                    lRows.Add(Matrix.M_SUB(changeFirst, changeSecond))
                Next

                If lRows.Count = 0 Then Return Nothing

                Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                    title:="MMRM selected difference in change from baseline by " & groupBaseName,
                    rowLabels:=rows.ToArray(),
                    lRows:=lRows,
                    result:=result,
                    alpha:=alpha,
                    footnote:="Comparison/treatment level, control/reference level, baseline visit, and contrast direction are selected by the caller.")
            End If

            ' Each group vs control.
            For Each v As Double In visits
                If MixedModelPostEstimation.NearlyEqual(v, baseline) Then Continue For

                Dim lControlBase() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, baseline, controlLevel, Nothing)
                Dim lControlVisit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, controlLevel, Nothing)
                If lControlBase Is Nothing OrElse lControlVisit Is Nothing Then Continue For

                Dim changeControl() As Double = Matrix.M_SUB(lControlVisit, lControlBase)

                For Each g As Double In groups
                    If MixedModelPostEstimation.NearlyEqual(g, controlLevel) Then Continue For

                    Dim lTreatBase() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, baseline, g, Nothing)
                    Dim lTreatVisit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g, Nothing)
                    If lTreatBase Is Nothing OrElse lTreatVisit Is Nothing Then Continue For

                    Dim changeTreat() As Double = Matrix.M_SUB(lTreatVisit, lTreatBase)
                    Dim lDiff() As Double = MixedModelPostEstimation.MakeDirectedDifference(changeTreat, changeControl, direction, DIR_TREATMENT_MINUS_CONTROL, DIR_CONTROL_MINUS_TREATMENT)

                    rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) &
                             " vs baseline " & MixedModelPostEstimation.FormatProfileValue(baseline) & ": Δ(" &
                             MixedModelPostEstimation.DirectedComparisonLabel(groupBaseName, g, controlLevel, direction, DIR_TREATMENT_MINUS_CONTROL, DIR_CONTROL_MINUS_TREATMENT) & ")")

                    lRows.Add(lDiff)
                Next
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                title:="MMRM difference in change from baseline by " & groupBaseName,
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                result:=result,
                alpha:=alpha,
                footnote:="Control/reference level, baseline visit, and contrast direction are selected by the caller.")
        End Function


        Private Function BuildPairwiseDifferenceInChange(result As MixedModelResult,
                                                         x(,) As Double,
                                                         visit() As Double,
                                                         groupValues() As Double,
                                                         groupBaseName As String,
                                                         baseline As Double,
                                                         alpha As Double) As Global.BESHStatNG.ResultTable
            Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(visit)
            Dim groups() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(groupValues)

            Dim rows As New List(Of String)
            Dim lRows As New List(Of Double())

            For Each v As Double In visits
                If MixedModelPostEstimation.NearlyEqual(v, baseline) Then Continue For

                For g1Index As Integer = 0 To groups.Length - 2
                    For g2Index As Integer = g1Index + 1 To groups.Length - 1
                        Dim g1 As Double = groups(g1Index)
                        Dim g2 As Double = groups(g2Index)

                        Dim lG1Base() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, baseline, g1, Nothing)
                        Dim lG1Visit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g1, Nothing)
                        Dim lG2Base() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, baseline, g2, Nothing)
                        Dim lG2Visit() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, visit, groupValues, v, g2, Nothing)

                        If lG1Base Is Nothing OrElse lG1Visit Is Nothing OrElse lG2Base Is Nothing OrElse lG2Visit Is Nothing Then Continue For

                        Dim changeG1() As Double = Matrix.M_SUB(lG1Visit, lG1Base)
                        Dim changeG2() As Double = Matrix.M_SUB(lG2Visit, lG2Base)
                        Dim diffChange() As Double = Matrix.M_SUB(changeG2, changeG1)

                        rows.Add("Visit " & MixedModelPostEstimation.FormatProfileValue(v) &
                                 " vs baseline " & MixedModelPostEstimation.FormatProfileValue(baseline) &
                                 ": Δ(" & groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(g2) &
                                 ") - Δ(" & groupBaseName & "=" & MixedModelPostEstimation.FormatProfileValue(g1) & ")")

                        lRows.Add(diffChange)
                    Next
                Next
            Next

            If lRows.Count = 0 Then Return Nothing

            Return MixedModelPostEstimation.BuildLinearContrastResultTable(
                title:="MMRM difference in change from baseline by " & groupBaseName,
                rowLabels:=rows.ToArray(),
                lRows:=lRows,
                result:=result,
                alpha:=alpha,
                footnote:="Each row is a difference-in-change contrast from observed-design LS-means.")
        End Function


        Private Function ResolveComparisonLevel(groupLevels() As Double,
                                                controlLevel As Double,
                                                requestedComparisonLevel As Double,
                                                ByRef comparisonLevel As Double) As Boolean
            comparisonLevel = Double.NaN
            If groupLevels Is Nothing OrElse groupLevels.Length = 0 Then Return False

            If AppInfrastructure.IsFinite(requestedComparisonLevel) AndAlso Not MixedModelPostEstimation.NearlyEqual(requestedComparisonLevel, controlLevel) Then
                For Each g As Double In groupLevels
                    If MixedModelPostEstimation.NearlyEqual(g, requestedComparisonLevel) Then
                        comparisonLevel = g
                        Return True
                    End If
                Next
            End If

            For Each g As Double In groupLevels
                If Not MixedModelPostEstimation.NearlyEqual(g, controlLevel) Then
                    comparisonLevel = g
                    Return True
                End If
            Next

            Return False
        End Function


        Private Sub ResolveDirectedGroupPair(controlLevel As Double,
                                             comparisonLevel As Double,
                                             direction As String,
                                             ByRef firstLevel As Double,
                                             ByRef secondLevel As Double)
            If String.Equals(direction, DIR_TREATMENT_MINUS_CONTROL, StringComparison.OrdinalIgnoreCase) Then
                firstLevel = comparisonLevel
                secondLevel = controlLevel
                Return
            End If

            If String.Equals(direction, DIR_CONTROL_MINUS_TREATMENT, StringComparison.OrdinalIgnoreCase) Then
                firstLevel = controlLevel
                secondLevel = comparisonLevel
                Return
            End If

            If comparisonLevel >= controlLevel Then
                firstLevel = comparisonLevel
                secondLevel = controlLevel
            Else
                firstLevel = controlLevel
                secondLevel = comparisonLevel
            End If
        End Sub

    End Module

End Namespace
