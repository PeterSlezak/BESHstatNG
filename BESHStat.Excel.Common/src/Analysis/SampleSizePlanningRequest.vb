Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum SampleSizeFamily
        PairedT
        UnpairedT
        SingleProportion
        IndependentProportions
        LogRank
        Cox
        Icc
        BlandAltman
    End Enum
    Public Enum SampleSizePlanningMethod
        LogRankSampleSize
        CoxEventCountContinuousCovariate
        CoxEventCountBinaryCovariate
        IccHypothesisTestSampleSize
        BlandAltmanLoASampleSize
        NonInferiorityIndependentProportions
        EquivalenceIndependentProportions
        IndependentProportions
        SingleProportion
        PairedTTest
        NonInferiorityUnpairedTTest
        EquivalenceUnpairedTTest
        UnpairedTTest
    End Enum
    Public Enum SampleSizeHypothesis
        NotApplicable
        Superiority
        Noninferiority
        Equivalence
    End Enum
    Public NotInheritable Class SampleSizeMethodDescription
        Public Sub New(family As SampleSizeFamily, methodName As String, roles As IEnumerable(Of String))
            Me.Family = family : Me.MethodName = methodName : FieldRoles = Array.AsReadOnly(roles.ToArray())
            If methodName.StartsWith("NonInferiority", StringComparison.Ordinal) Then
                Hypothesis = SampleSizeHypothesis.Noninferiority
            ElseIf methodName.StartsWith("Equivalence", StringComparison.Ordinal) Then
                Hypothesis = SampleSizeHypothesis.Equivalence
            ElseIf family = SampleSizeFamily.UnpairedT OrElse family = SampleSizeFamily.IndependentProportions Then
                Hypothesis = SampleSizeHypothesis.Superiority
            Else
                Hypothesis = SampleSizeHypothesis.NotApplicable
            End If
        End Sub
        Public ReadOnly Property Family As SampleSizeFamily
        Public ReadOnly Property MethodName As String
        Public ReadOnly Property FieldRoles As IReadOnlyList(Of String)
        Public ReadOnly Property Hypothesis As SampleSizeHypothesis
        Public ReadOnly Property AlphaIsOneSided As Boolean
            Get
                Return Family = SampleSizeFamily.Icc OrElse Hypothesis = SampleSizeHypothesis.Noninferiority OrElse Hypothesis = SampleSizeHypothesis.Equivalence
            End Get
        End Property
        Public ReadOnly Property BetaVisible As Boolean
            Get
                Return Family <> SampleSizeFamily.BlandAltman
            End Get
        End Property
    End Class
    ''' <summary>Already converted numeric intent; beta remains type-II error, never power.
    ''' ICC integer conversion and all culture/control validation remain host-owned.</summary>
    Public NotInheritable Class SampleSizePlanningRequest
        Private ReadOnly values As Double()
        Public Sub New(method As SampleSizePlanningMethod, alpha As Double, beta As Double, inputs As Double(),
                       Optional observationsPerSubject As Integer = 0, Optional overallEventProvided As Boolean = True,
                       Optional rSquaredProvided As Boolean = True)
            Me.Method = method : Me.Alpha = alpha : Me.Beta = beta
            values = CType(inputs.Clone(), Double())
            Me.ObservationsPerSubject = observationsPerSubject
            Me.OverallEventProvided = overallEventProvided : Me.RSquaredProvided = rSquaredProvided
            Description = SampleSizePlanningDescriptions.Describe(method)
        End Sub
        Public ReadOnly Property Method As SampleSizePlanningMethod
        Public ReadOnly Property Description As SampleSizeMethodDescription
        Public ReadOnly Property Alpha As Double
        Public ReadOnly Property Beta As Double
        Public ReadOnly Property ObservationsPerSubject As Integer
        Public ReadOnly Property OverallEventProvided As Boolean
        Public ReadOnly Property RSquaredProvided As Boolean
        Public Function Value(index As Integer) As Double
            Return values(index)
        End Function
        Public Function Inputs() As Double()
            Return CType(values.Clone(), Double())
        End Function
        Public Function CandidateDiagnostics() As SpreadsheetValidationReport
            'Observation-only: Windows keeps its original CheckData/errors and Core its domain checks.
            Return PairedOptionPolicy.Validate(Alpha, PairedAlternative.TwoSided, PairedDifferenceDirection.SecondMinusFirst)
        End Function
    End Class
    ''' <summary>Frozen exact text and numeric Core result. Core result ownership passes to caller;
    ''' later result mutation cannot alter the captured text or request.</summary>
    Public NotInheritable Class SampleSizePlanningReport
        Public Sub New(request As SampleSizePlanningRequest, coreResult As Object, text As String)
            Me.Request = request : Me.CoreResult = coreResult : Me.Text = text
        End Sub
        Public ReadOnly Property Request As SampleSizePlanningRequest
        Public ReadOnly Property CoreResult As Object
        Public ReadOnly Property Text As String
        Public ReadOnly Property Lines As IReadOnlyList(Of String)
            Get
                Return Array.AsReadOnly(Text.Split({PlanningTextNewLine.Value}, StringSplitOptions.None))
            End Get
        End Property
    End Class
    ''' <summary>The original VB newline is CRLF on every host, including portable projections.</summary>
    Public NotInheritable Class PlanningTextNewLine
        Private Sub New()
        End Sub
        Public Shared ReadOnly Value As String = String.Concat(Convert.ToChar(13), Convert.ToChar(10))
    End Class
End Namespace
