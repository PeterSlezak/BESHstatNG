Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.Agreement
Imports BESHStatNG.ExcelCommon
Public NotInheritable Class Agreement26Reference
    Public Shared Function Run(kind As Integer, profile As Integer, x As Double(), y As Double(), rows As Integer(), subjects As Object()) As (Result As Object, Tables As List(Of ResultTable), Options As Object)
        Dim axis As AgreementInputAxis = Nothing, tables As List(Of ResultTable) = Nothing, fitResult As Object = Nothing, options As Object = Nothing
        Dim effectiveInput As Object = Nothing, effectiveRows = rows

                Select Case kind
                    Case 0
                        Dim opts As New DemingOptions With {.VarianceModel = CType(profile Mod 3, DemingVarianceModel), .SDx = Enumerable.Repeat(0.5R, 12).ToArray(), .SDy = Enumerable.Repeat(0.6R, 12).ToArray(), .CVx = 0.05R, .CVy = 0.06R, .BootstrapReplicates = 200, .FitIntercept = profile Mod 2 = 0}
                        If profile = 6 Then opts.SDx(0) = 0 : opts.VarianceModel = DemingVarianceModel.KnownPointwiseSD
                        Dim request As New Agreement26Legacy.Deming(opts, profile = 3, profile = 4, profile = 0, profile = 1)
                        opts = request.CreateOptions() : options = opts
                        Dim engine As New WeightedDemingRegression(x, y, "reference", "test") With {.Options = opts}
                        Select Case request.Execution
                            Case DemingExecutionIntent.AnalyticalClosedForm : fitResult = engine.DemingAnalyticalCI()
                            Case DemingExecutionIntent.AnalyticalMcr : fitResult = engine.DemingAnalyticalCI_MCR()
                            Case DemingExecutionIntent.Jackknife : fitResult = engine.FitJackknifeCI()
                            Case Else : fitResult = engine.Fit(randomSeed:=12345)
                        End Select
                        tables = engine.wrapResults()
                    Case 1
                        Dim request As New Agreement26Legacy.Lin(New LinConcordanceOptions With {.Alpha = 0.1R, .NullConcordance = 0.3R, .BootstrapReplicates = If(profile = 7, 30, 200)}, profile Mod 3 = 0, profile Mod 3 = 1, profile Mod 3 = 2)
                        Dim opts = request.CreateOptions() : options = opts
                        Dim engine As New LinConcordanceCorrelation(x, y, "reference", "test") With {.Options = opts}
                        fitResult = engine.Fit(randomSeed:=12345) : tables = engine.wrapResults()
                    Case 2
                        Dim first As Object() = {"a", "b", "Ω", "a", "b", "Ω", "a", "b", "Ω", "a", "b", "Ω"}
                        Dim second As Object() = {"a", "b", "Ω", "b", "b", "Ω", "a", "a", "Ω", "a", "b", "Ω"}
                        If profile = 7 Then first(0) = Nothing : second(1) = ""
                        effectiveInput = New Object() {first, second}
                        axis = New AgreementInputAxis(12, {"rater1", "rater2"}, rows, categoricalFirst:=first, categoricalSecond:=second, header:=True)
                        Dim request As New Agreement26Legacy.Kappa(New KappaOptions With {.BootstrapReplicates = 200}, profile Mod 5, profile < 5, profile = 5, profile = 6, profile = 7)
                        Dim opts = request.CreateOptions() : options = opts
                        Dim engine As New WeightedKappaAgreement(first, second, "rater1", "rater2") With {.Options = opts}
                        fitResult = engine.Fit(randomSeed:=12345) : tables = engine.wrapResults()
                    Case 3
                        Dim raw = PairedOptionPolicy.ProjectBland(profile Mod 3, profile Mod 5, profile = 4, profile = 5, profile = 6)
                        raw.SubjectIds = If(profile Mod 2 = 0, subjects, Nothing) : raw.BootstrapReplicates = If(profile = 7, 30, 200)
                        Dim request As New Agreement26Legacy.Bland(raw, profile Mod 3, profile Mod 3), opts = request.CreateOptions()
                        options = opts
                        Dim engine As New BlandAltmanAgreement(x, y, "reference", "test", opts)
                        fitResult = engine.Fit(randomSeed:=12345) : tables = engine.wrapResults()
                    Case Else
                        Dim matrix As Double(,) = {{10, 11, 12}, {20, 19, 21}, {30, 29, 28}, {40, 41, 39}, {50, 49, 48}}
                        effectiveRows = rows.Take(matrix.GetLength(0)).ToArray()
                        axis = New AgreementInputAxis(matrix.GetLength(0), {"rater1", "rater2", "rater3"}, effectiveRows,
                            subjectIds:=Enumerable.Range(0, matrix.GetLength(0)).Select(Function(i) CObj("ICC subject " & i.ToString())).ToArray())
                        Dim engine As New IntraclassCorrelation, ci As ConfidenceIntervalResult = Nothing
                        Dim choice = profile Mod 6, flags = Enumerable.Range(0, 6).Select(Function(i) i = choice).ToArray()
                        Dim intent As New Agreement26Legacy.Icc(flags(0), flags(1), flags(2), flags(3), flags(4), flags(5), 0.05R, profile Mod 2 = 0)
                        options = intent
                        Dim jagged = Enumerable.Range(0, 5).Select(Function(i) New Double() {matrix(i, 0), matrix(i, 1), matrix(i, 2)}).ToArray()
                        effectiveInput = If(choice < 2, CObj(jagged), CObj(matrix))
                        Select Case intent.Choice
                            Case IccEstimatorChoice.OneOne : ci = engine.ICC11(jagged, intent.Alpha)
                            Case IccEstimatorChoice.OneAverage : ci = engine.ICC1k(jagged, intent.Alpha)
                            Case IccEstimatorChoice.TwoOne : ci = engine.ICC21(matrix, intent.Alpha)
                            Case IccEstimatorChoice.TwoAverage : ci = engine.ICC2k(matrix, intent.Alpha)
                            Case IccEstimatorChoice.ThreeOne : ci = engine.ICC31(matrix, intent.Alpha)
                            Case IccEstimatorChoice.ThreeAverage : ci = engine.ICC3k(matrix, intent.Alpha)
                        End Select
                        If intent.Repeatability Then
                            If choice < 2 Then
                                engine.RepeatabilityCoefficient_OneWay(jagged, choice = 1, intent.Alpha)
                            Else
                                engine.RepeatabilityCoefficient_TwoWay(matrix, choice < 4, choice = 3 OrElse choice = 5, intent.Alpha)
                            End If
                        End If
                        fitResult = ci : tables = engine.wrapResults(ci, intent.Label(3))
                End Select

        Return (fitResult, tables, options)
    End Function
End Class
