Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

Friend Module SpcTestFixtures

    Public Const TightTolerance As Double = 0.0000000001
    Public Const StandardTolerance As Double = 0.0000001

    Public Function AssertThrowsAssignable(Of TException As Exception)(
        action As Action,
        Optional message As String = Nothing) As TException

        If action Is Nothing Then
            Throw New ArgumentNullException(NameOf(action))
        End If

        Try
            action()
        Catch ex As Exception
            If TypeOf ex Is TException Then
                Return DirectCast(ex, TException)
            End If

            Assert.Fail(
                If(message, "Unexpected exception type.") &
                " Expected a type assignable to " & GetType(TException).FullName &
                ", but " & ex.GetType().FullName & " was thrown.")
        End Try

        Assert.Fail(
            If(message, "Expected an exception.") &
            " Expected a type assignable to " & GetType(TException).FullName & ".")
        Return Nothing
    End Function


    Public Sub AssertClose(expected As Double,
                           actual As Double,
                           Optional tolerance As Double = StandardTolerance,
                           Optional message As String = Nothing)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) OrElse
           Math.Abs(expected - actual) > tolerance Then
            Assert.Fail(If(message, "Values differ.") &
                        " Expected " & expected.ToString("R") &
                        ", actual " & actual.ToString("R") & ".")
        End If
    End Sub

    Public Sub AssertFinite(value As Double, Optional message As String = Nothing)
        Assert.IsFalse(Double.IsNaN(value) OrElse Double.IsInfinity(value),
                       If(message, "Expected a finite value."))
    End Sub

    Public Sub AssertFiniteVector(values As Double(), Optional message As String = Nothing)
        Assert.IsNotNull(values)
        For i As Integer = 0 To values.Length - 1
            AssertFinite(values(i), If(message, "Vector") & " index " & i.ToString())
        Next
    End Sub

    Public Function NoRulesOptions(
        Optional missingPolicy As SpcMissingValuePolicy = SpcMissingValuePolicy.Reject,
        Optional estimator As SpcWithinSigmaEstimator = SpcWithinSigmaEstimator.Automatic,
        Optional parameterSource As SpcParameterSource = SpcParameterSource.EstimateFromPhaseI,
        Optional method As SpcControlLimitMethod = SpcControlLimitMethod.ShewhartSigma,
        Optional movingRangeLength As Integer = 2,
        Optional useBiasCorrection As Boolean = True) As SpcAnalysisOptions

        Return New SpcAnalysisOptions With {
            .MissingValuePolicy = missingPolicy,
            .ControlLimits = New SpcControlLimitOptions With {
                .ParameterSource = parameterSource,
                .Method = method,
                .SigmaMultiplier = 3.0,
                .WithinSigmaEstimator = estimator,
                .NaturalLimitPolicy = SpcNaturalLimitPolicy.ClipToFeasibleRange,
                .MovingRangeLength = movingRangeLength,
                .UseBiasCorrection = useBiasCorrection
            },
            .Rules = New SpcRuleOptions With {
                .Preset = SpcRulePreset.None,
                .PhaseScope = SpcRulePhaseScope.All,
                .GapBehavior = SpcSequenceGapBehavior.BreakSequence,
                .MarkingMode = SpcSignalMarkingMode.TerminalPointOnly
            }
        }
    End Function

    Public Function OptionsWithRules(preset As SpcRulePreset,
                                     Optional phaseScope As SpcRulePhaseScope = SpcRulePhaseScope.All,
                                     Optional gapBehavior As SpcSequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence,
                                     Optional markingMode As SpcSignalMarkingMode = SpcSignalMarkingMode.TerminalPointOnly) As SpcAnalysisOptions
        Dim options As SpcAnalysisOptions = NoRulesOptions()
        options.Rules = New SpcRuleOptions With {
            .Preset = preset,
            .PhaseScope = phaseScope,
            .GapBehavior = gapBehavior,
            .MarkingMode = markingMode
        }
        Return options
    End Function

    Public Function Labels(count As Integer, Optional prefix As String = "P") As String()
        Dim result(count - 1) As String
        For i As Integer = 0 To count - 1
            result(i) = prefix & (i + 1).ToString("00")
        Next
        Return result
    End Function

    Public Function Sequence(count As Integer) As Double()
        Dim result(count - 1) As Double
        For i As Integer = 0 To count - 1
            result(i) = CDbl(i + 1)
        Next
        Return result
    End Function

    Public Function BaselineMonitoringStages(pointCount As Integer,
                                             baselineCount As Integer) As SpcStageDefinition()
        Return {
            New SpcStageDefinition("Baseline", 0, baselineCount - 1,
                                   SpcPhase.PhaseI,
                                   SpcStageLimitMode.EstimateFromStageData),
            New SpcStageDefinition("Monitoring", baselineCount, pointCount - 1,
                                   SpcPhase.PhaseII,
                                   SpcStageLimitMode.UseReferenceStage,
                                   referenceStageId:="Baseline")
        }
    End Function

    Public Function IndividualsData() As Double()
        Return {
            9.8, 10.1, 9.9, 10.2, 10.0, 9.7, 10.3, 10.0, 9.9, 10.1,
            9.8, 10.2, 10.0, 9.9, 10.1, 10.0, 9.7, 10.3, 9.8, 10.2,
            10.8, 10.9, 11.0, 10.7, 11.1, 10.9, 11.2, 10.8, 11.0, 10.9
        }
    End Function

    Public Function WideSubgroups() As Double(,)
        Dim means As Double() = {
            50.0, 50.1, 49.9, 50.05, 49.95, 50.08,
            49.92, 50.03, 50.0, 49.97, 50.06, 49.94,
            50.45, 50.5, 50.55, 50.4, 50.6, 50.5,
            50.52, 50.48, 50.58, 50.42, 50.54, 50.46
        }
        Dim offsets As Double() = {-0.2, -0.1, 0.0, 0.1, 0.2}
        Dim result(means.Length - 1, offsets.Length - 1) As Double
        For i As Integer = 0 To means.Length - 1
            For j As Integer = 0 To offsets.Length - 1
                result(i, j) = means(i) + offsets(j)
            Next
        Next
        Return result
    End Function

    Public Sub StackWide(values As Double(,),
                         ByRef stackedValues As Double(),
                         ByRef subgroupIds As String(),
                         ByRef stackedLabels As String(),
                         ByRef stackedSequence As Double())
        Dim rows As Integer = values.GetLength(0)
        Dim columns As Integer = values.GetLength(1)
        ReDim stackedValues(rows * columns - 1)
        ReDim subgroupIds(rows * columns - 1)
        ReDim stackedLabels(rows * columns - 1)
        ReDim stackedSequence(rows * columns - 1)
        Dim position As Integer = 0
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To columns - 1
                stackedValues(position) = values(i, j)
                subgroupIds(position) = "G" & (i + 1).ToString("00")
                stackedLabels(position) = "G" & (i + 1).ToString("00")
                stackedSequence(position) = CDbl(i + 1)
                position += 1
            Next
        Next
    End Sub

    Public Function AttributeCounts() As Double()
        Return {
            1, 2, 1, 3, 2, 1, 2, 3, 1, 2, 2, 1, 3, 2, 1,
            2, 1, 3, 2, 1, 7, 6, 8, 7, 9, 6, 8, 7, 9, 8
        }
    End Function

    Public Function SampleSizes() As Double()
        Return {
            100, 95, 105, 110, 90, 100, 98, 102, 96, 104,
            100, 92, 108, 97, 103, 100, 95, 105, 110, 90,
            100, 98, 102, 96, 104, 100, 92, 108, 97, 103
        }
    End Function

    Public Function ConstantSampleSizes(Optional count As Integer = 30,
                                        Optional sampleSize As Double = 100.0) As Double()
        Dim result(count - 1) As Double
        For i As Integer = 0 To result.Length - 1
            result(i) = sampleSize
        Next
        Return result
    End Function

    Public Function ExposuresARRAY() As Double()
        Return {
            1.0, 1.2, 0.9, 1.1, 1.0, 1.3, 0.8, 1.2, 1.0, 0.9,
            1.1, 1.0, 1.2, 0.8, 1.3, 1.0, 0.9, 1.1, 1.2, 0.8,
            1.0, 1.3, 0.9, 1.1, 1.0, 1.2, 0.8, 1.3, 0.9, 1.1
        }
    End Function

    Public Function IndividualMultivariateData() As Double(,)
        Dim n As Integer = 48
        Dim result(n - 1, 2) As Double
        For i As Integer = 0 To n - 1
            Dim t As Double = CDbl(i + 1)
            Dim shift As Double = If(i >= 36, 2.5, 0.0)
            result(i, 0) = Math.Sin(t * 0.41) + 0.03 * t + shift
            result(i, 1) = Math.Cos(t * 0.29) - 0.02 * t + 0.7 * shift
            result(i, 2) = Math.Sin(t * 0.17) + Math.Cos(t * 0.37) + 0.4 * shift
        Next
        Return result
    End Function

    Public Sub MultivariatePhasesAndStages(rowCount As Integer,
                                           baselineCount As Integer,
                                           ByRef phases As SpcPhase(),
                                           ByRef stages As String())
        ReDim phases(rowCount - 1)
        ReDim stages(rowCount - 1)
        For i As Integer = 0 To rowCount - 1
            phases(i) = If(i < baselineCount, SpcPhase.PhaseI, SpcPhase.PhaseII)
            stages(i) = If(i < baselineCount, "Baseline", "Monitoring")
        Next
    End Sub

    Public Sub GroupedMultivariateData(ByRef values As Double(,),
                                       ByRef subgroupIds As String(),
                                       ByRef phases As SpcPhase(),
                                       ByRef stages As String())
        Const groupCount As Integer = 14
        Const groupSize As Integer = 5
        ReDim values(groupCount * groupSize - 1, 2)
        ReDim subgroupIds(groupCount * groupSize - 1)
        ReDim phases(groupCount * groupSize - 1)
        ReDim stages(groupCount * groupSize - 1)

        Dim row As Integer = 0
        For g As Integer = 0 To groupCount - 1
            Dim groupShift As Double = If(g >= 10, 1.5, 0.0)
            For j As Integer = 0 To groupSize - 1
                Dim centered As Double = CDbl(j - 2)
                values(row, 0) = 10.0 + 0.18 * centered + 0.03 * g + groupShift
                values(row, 1) = 20.0 - 0.12 * centered + 0.02 * g + 0.7 * groupShift + 0.015 * centered * centered
                values(row, 2) = 5.0 + 0.08 * centered + 0.025 * g + 0.5 * groupShift + 0.01 * centered * centered * centered
                subgroupIds(row) = "SG" & (g + 1).ToString("00")
                phases(row) = If(g < 10, SpcPhase.PhaseI, SpcPhase.PhaseII)
                stages(row) = If(g < 10, "Baseline", "Monitoring")
                row += 1
            Next
        Next
    End Sub

    Public Function StandardizedPanel(zValues As Double(),
                                      Optional panelType As SpcPanelType = SpcPanelType.IndividualValue,
                                      Optional phases As SpcPhase() = Nothing,
                                      Optional stageIds As String() = Nothing,
                                      Optional excludedIndex As Nullable(Of Integer) = Nothing) As SpcPanelResult
        Dim points(zValues.Length - 1) As SpcPointResult
        For i As Integer = 0 To zValues.Length - 1
            Dim phase As SpcPhase = If(phases Is Nothing, SpcPhase.PhaseI, phases(i))
            Dim stage As String = If(stageIds Is Nothing, "Stage1", stageIds(i))
            Dim scope As SpcExclusionScope = SpcExclusionScope.None
            Dim includedInRules As Boolean = True
            If excludedIndex.HasValue AndAlso excludedIndex.Value = i Then
                scope = SpcExclusionScope.RuleEvaluation
                includedInRules = False
            End If
            points(i) = New SpcPointResult(
                i, zValues(i), 0.0, -3.0, 3.0,
                stageId:=stage,
                phase:=phase,
                standardError:=1.0,
                standardizedValue:=zValues(i),
                lowerOneSigmaLimit:=-1.0,
                upperOneSigmaLimit:=1.0,
                lowerTwoSigmaLimit:=-2.0,
                upperTwoSigmaLimit:=2.0,
                includedInRuleEvaluation:=includedInRules,
                exclusionScope:=scope,
                exclusionReason:=If(includedInRules, Nothing, "Synthetic gap"))
        Next
        Return New SpcPanelResult(panelType, panelType.ToString(), points)
    End Function

End Module
