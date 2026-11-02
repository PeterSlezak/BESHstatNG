Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    Public Enum PairedDifferenceDirection
        FirstMinusSecond = 0
        SecondMinusFirst = 1
    End Enum
    Public Enum PairedAlternative
        TwoSided = 0
        Less = 1
        Greater = 2
    End Enum

    ''' <summary>Numeric request validation; host cultures and control state are captured by the caller.</summary>
    Public NotInheritable Class PairedOptionPolicy
        Private Sub New()
        End Sub
        Public Shared Function Validate(alpha As Double, alternative As PairedAlternative, direction As PairedDifferenceDirection) As SpreadsheetValidationReport
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If Not Finite(alpha) OrElse alpha <= 0 OrElse alpha >= 1 Then Add(issues, "paired.alpha", "Alpha must be finite and strictly between zero and one.", "options.alpha")
            If Not [Enum].IsDefined(GetType(PairedAlternative), alternative) Then Add(issues, "paired.alternative", "Unknown alternative.", "options.alternative")
            If Not [Enum].IsDefined(GetType(PairedDifferenceDirection), direction) Then Add(issues, "paired.direction", "Unknown difference direction.", "options.direction")
            Return New SpreadsheetValidationReport(issues)
        End Function

        Public Shared Function ValidateLimits(enabled As Boolean, lower As Double, upper As Double) As SpreadsheetValidationReport
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If enabled Then
                If Not Finite(lower) OrElse Not Finite(upper) Then
                    Add(issues, "bland.limits.finite", "Acceptable limits must be finite.", "options.limits")
                ElseIf lower > upper Then
                    Add(issues, "bland.limits.order", "The lower acceptable limit must not exceed the upper acceptable limit.", "options.limits")
                End If
            End If
            Return New SpreadsheetValidationReport(issues)
        End Function

        ''' <summary>Exact legacy UI fallback/precedence, with no fitting or unit conversion.</summary>
        Public Shared Function ProjectBland(modeIndex As Integer, scaleIndex As Integer, bootstrap As Boolean, bca As Boolean, jackknife As Boolean) As Agreement.BlandAltmanOptions
            Return New Agreement.BlandAltmanOptions With {
                .Mode = If(modeIndex = 1, Agreement.RepeatedBlandAltmanMode.SimplePairs, If(modeIndex = 2, Agreement.RepeatedBlandAltmanMode.RepeatedBySubject, Agreement.RepeatedBlandAltmanMode.Auto)),
                .Scale = If(scaleIndex >= 0 AndAlso scaleIndex <= 4, CType(scaleIndex, Agreement.BlandAltmanScale), Agreement.BlandAltmanScale.RawDifference),
                .CiMethod = If(bootstrap, Agreement.AgreementCiMethod.BootstrapPercentile, If(bca, Agreement.AgreementCiMethod.BootstrapBCa, If(jackknife, Agreement.AgreementCiMethod.Jackknife, Agreement.AgreementCiMethod.Analytical)))
            }
        End Function

        Public Shared Function ValidateBland(options As Agreement.BlandAltmanOptions, x As IReadOnlyList(Of Double), y As IReadOnlyList(Of Double)) As SpreadsheetValidationReport
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If options Is Nothing OrElse x Is Nothing OrElse y Is Nothing OrElse x.Count = 0 OrElse x.Count <> y.Count Then
                Add(issues, "bland.shape", "Matching nonempty paired values and options are required.", "input")
                Return New SpreadsheetValidationReport(issues)
            End If
            issues.AddRange(Validate(options.Alpha, PairedAlternative.TwoSided, PairedDifferenceDirection.SecondMinusFirst).Issues)
            If Not [Enum].IsDefined(GetType(Agreement.BlandAltmanScale), options.Scale) Then Add(issues, "bland.scale", "Unknown scale.", "options.scale")
            If Not [Enum].IsDefined(GetType(Agreement.RepeatedBlandAltmanMode), options.Mode) Then Add(issues, "bland.mode", "Unknown mode.", "options.mode")
            If Not [Enum].IsDefined(GetType(Agreement.AgreementCiMethod), options.CiMethod) Then Add(issues, "bland.ci", "Unknown interval method.", "options.ci")
            If options.SubjectIds IsNot Nothing AndAlso options.SubjectIds.Length <> x.Count Then Add(issues, "bland.subjects", "Subject IDs must match the paired observations.", "options.subjectIds")
            If options.Mode = Agreement.RepeatedBlandAltmanMode.RepeatedBySubject AndAlso options.SubjectIds Is Nothing AndAlso Not options.AllowFallbackToSimple Then Add(issues, "bland.subjects.required", "Repeated mode requires subject IDs when fallback is disabled.", "options.subjectIds")
            For i = 0 To x.Count - 1
                Dim requiresSubject = options.Mode = Agreement.RepeatedBlandAltmanMode.RepeatedBySubject OrElse
                    (options.Mode = Agreement.RepeatedBlandAltmanMode.Auto AndAlso options.SubjectIds IsNot Nothing)
                If requiresSubject AndAlso options.SubjectIds IsNot Nothing AndAlso options.SubjectIds.Length = x.Count Then
                    Dim subject = options.SubjectIds(i)
                    If subject Is Nothing OrElse Convert.IsDBNull(subject) OrElse
                        (TypeOf subject Is String AndAlso String.IsNullOrWhiteSpace(DirectCast(subject, String))) Then
                        issues.Add(New SpreadsheetValidationIssue("bland.subject.filtered", SpreadsheetValidationSeverity.Warning, "Missing-subject whole pair is filtered by the engine before scale transformation.", "input.pairs", i + 1))
                        Continue For
                    End If
                End If
                ' The authoritative engine filters nonfinite whole pairs before transforming.
                If Not Finite(x(i)) OrElse Not Finite(y(i)) Then
                    issues.Add(New SpreadsheetValidationIssue("bland.pair.filtered", SpreadsheetValidationSeverity.Warning, "Nonfinite whole pair is filtered by the engine.", "input.pairs", i + 1))
                    Continue For
                End If
                Dim invalid As Boolean = False
                Select Case options.Scale
                    Case Agreement.BlandAltmanScale.PercentOfMean : invalid = invalid OrElse (x(i) + y(i)) * 0.5 = 0
                    Case Agreement.BlandAltmanScale.PercentOfReference : invalid = invalid OrElse x(i) = 0
                    Case Agreement.BlandAltmanScale.PercentOfTest : invalid = invalid OrElse y(i) = 0
                    Case Agreement.BlandAltmanScale.LogRatio : invalid = invalid OrElse x(i) <= 0 OrElse y(i) <= 0
                End Select
                If invalid Then issues.Add(New SpreadsheetValidationIssue("bland.scale.domain", SpreadsheetValidationSeverity.Error, "Paired values are outside the active scale's domain.", "input.pairs", i + 1))
            Next
            Return New SpreadsheetValidationReport(issues)
        End Function

        Private Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
        Private Shared Sub Add(issues As List(Of SpreadsheetValidationIssue), code As String, message As String, field As String)
            issues.Add(New SpreadsheetValidationIssue(code, SpreadsheetValidationSeverity.Error, message, field))
        End Sub
    End Class
End Namespace
