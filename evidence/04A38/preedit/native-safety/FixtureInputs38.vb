Option Strict On
Imports System
Imports System.Globalization
Public NotInheritable Class UdfFixtureInputs38


    Public Shared Sub BuildSyntheticMmrmInputs(ByRef y(,) As Object,
                                                ByRef x(,) As Object,
                                                ByRef subject(,) As Object,
                                                ByRef visit(,) As Object,
                                                ByRef names(,) As Object)
        Dim subjects As Integer = 18
        Dim visits As Integer = 3
        Dim n As Integer = subjects * visits

        ReDim y(n - 1, 0)
        ReDim x(n - 1, 4)
        ReDim subject(n - 1, 0)
        ReDim visit(n - 1, 0)
        ReDim names(0, 4)

        names(0, 0) = "treatment_active"
        names(0, 1) = "visit2"
        names(0, 2) = "visit3"
        names(0, 3) = "treatment_active:visit2"
        names(0, 4) = "treatment_active:visit3"

        Dim r As Integer = 0

        For sid As Integer = 1 To subjects
            Dim trt As Double = If(sid Mod 2 = 0, 1.0, 0.0)
            Dim subjectOffset As Double = 0.15 * CDbl((sid Mod 5) - 2)

            For v As Integer = 1 To visits
                Dim v2 As Double = If(v = 2, 1.0, 0.0)
                Dim v3 As Double = If(v = 3, 1.0, 0.0)
                Dim deterministicNoise As Double = 0.03 * CDbl(((sid + v) Mod 4) - 1)

                Dim response As Double =
                    20.0 +
                    1.4 * trt +
                    0.8 * CDbl(v) +
                    0.35 * trt * CDbl(v) +
                    subjectOffset +
                    deterministicNoise

                y(r, 0) = response
                x(r, 0) = trt
                x(r, 1) = v2
                x(r, 2) = v3
                x(r, 3) = trt * v2
                x(r, 4) = trt * v3
                subject(r, 0) = "S" & sid.ToString("00", CultureInfo.InvariantCulture)
                visit(r, 0) = CDbl(v)

                r += 1
            Next
        Next
    End Sub


    Public Shared Sub BuildSyntheticLmmInputs(ByRef y(,) As Object,
                                               ByRef x(,) As Object,
                                               ByRef z(,) As Object,
                                               ByRef subject(,) As Object,
                                               ByRef visit(,) As Object,
                                               ByRef xNames(,) As Object,
                                               ByRef zNames(,) As Object,
                                               randomFormulaMode As Boolean)
        Dim subjects As Integer = 18
        Dim visits As Integer = 5
        Dim n As Integer = subjects * visits

        ReDim y(n - 1, 0)
        ReDim x(n - 1, 2)
        If randomFormulaMode Then
            ReDim z(n - 1, 1)
        Else
            ReDim z(n - 1, 0)
        End If
        ReDim subject(n - 1, 0)
        ReDim visit(n - 1, 0)
        ReDim xNames(0, 2)
        If randomFormulaMode Then
            ReDim zNames(0, 1)
        Else
            ReDim zNames(0, 0)
        End If

        xNames(0, 0) = "Days"
        xNames(0, 1) = "Treatment"
        xNames(0, 2) = "Difficulty"
        zNames(0, 0) = "Days"
        If randomFormulaMode Then zNames(0, 1) = "Difficulty"

        Dim r As Integer = 0
        For sid As Integer = 1 To subjects
            Dim treatment As Double = If(sid Mod 2 = 0, 1.0, 0.0)
            Dim subjectIntercept As Double = 0.35 * CDbl((sid Mod 6) - 2)
            Dim subjectSlope As Double = 0.04 * CDbl((sid Mod 5) - 2)
            Dim ageShift As Double = 0.03 * CDbl(sid - 9)

            For v As Integer = 0 To visits - 1
                Dim days As Double = CDbl(v) - 2.0
                Dim difficulty As Double = -0.2 + 0.15 * CDbl(v) + 0.04 * CDbl(sid Mod 4)
                Dim deterministicNoise As Double = 0.035 * CDbl(((sid + v) Mod 5) - 2)

                y(r, 0) = 12.0 +
                          0.9 * days +
                          1.1 * treatment +
                          0.55 * difficulty +
                          0.28 * days * treatment +
                          ageShift +
                          subjectIntercept +
                          subjectSlope * days +
                          deterministicNoise

                x(r, 0) = days
                x(r, 1) = treatment
                x(r, 2) = difficulty
                z(r, 0) = days
                If randomFormulaMode Then z(r, 1) = difficulty
                subject(r, 0) = "S" & sid.ToString("00", CultureInfo.InvariantCulture)
                visit(r, 0) = CDbl(v + 1)

                r += 1
            Next
        Next
    End Sub
End Class
