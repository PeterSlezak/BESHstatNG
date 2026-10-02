Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace survival

    ''' <summary>Host-neutral survival observation used by Kaplan-Meier, log-rank, and Cox analyses.</summary>
    Public Structure SurvivalRecord
        Public Time As Double
        Public Censorship As Integer
        Public Group As Integer
        Public strGroup As String
        Public Stratum As String
        Public strStratum As String
        Public Covariates As Double()
        Public Index As Integer
    End Structure

    ''' <summary>Host-neutral row of Kaplan-Meier tabular output.</summary>
    Public Structure SurvivalTableRecord
        Public Time As Double
        Public Group As Integer
        Public strGroup As String
        Public AtRisk As Integer
        Public Prob As Double
        Public SE As Double
        Public ProbCILL As Double
        Public ProbCIUL As Double
    End Structure

    ''' <summary>Neutral plotting payload prepared by survival calculations and rendered by a host adapter.</summary>
    Public NotInheritable Class KaplanMeierPlotData
        Public Property Alpha As Double
        Public Property GroupLabels As String()
        Public Property MaximumTimeByGroup As Double()
        Public Property CensoredCountByGroup As Integer()
        Public Property CensorMarkerProbability As Double(,)
        Public Property CensorMarkerTime As Double(,)
        Public Property LowerConfidenceLimit As Double(,)
        Public Property UpperConfidenceLimit As Double(,)
        Public Property SurvivalTime As Double()
        Public Property SurvivalProbability As Double(,)
    End Class

    ''' <summary>Host-neutral construction and formatting helpers shared by survival analyses.</summary>
    Public Module Survival

        Public Function survivalRecord2str(x As SurvivalRecord) As String
            Return $"Time:{x.Time}; censor:{x.Censorship}; group:{x.Group}; strGroup:{x.strGroup}; strata:{x.Stratum}; strStrata:{x.strStratum}"
        End Function

        Public Function survRecList2str(x As List(Of SurvivalRecord)) As String
            Dim s As String = survivalRecord2str(x(0))
            For i As Integer = 1 To x.Count - 1
                s &= Environment.NewLine & survivalRecord2str(x(i))
            Next
            Return s
        End Function

        Public Function SurvivalTableRecord2array(x As SurvivalTableRecord) As Object()
            Return New Object() {x.Time, x.strGroup, x.AtRisk, x.Prob, x.SE, x.ProbCILL, x.ProbCIUL}
        End Function

        ''' <summary>
        ''' Constructs aligned survival records from host-neutral input vectors.
        ''' Group and stratum numeric identifiers preserve first-seen label order.
        ''' </summary>
        Public Function CreatSurvivalData(t() As Double,
                                           s() As Integer,
                                           g() As String,
                                           strat() As String,
                                           ByRef strErr As String) As List(Of SurvivalRecord)
            Dim output As New List(Of SurvivalRecord)()

            If t.Length <> s.Length OrElse t.Length <> g.Length OrElse t.Length <> strat.Length Then
                strErr = "Invalid input dimensions"
                CoreServices.Log(strErr)
                Return Nothing
            End If

            Dim grpIds As List(Of String) = g.Distinct().ToList()
            Dim stratumIds As List(Of String) = strat.Distinct().ToList()

            For i As Integer = 0 To t.Length - 1
                If t(i) < 0.0 Then
                    strErr = "Unexpected time value (values less then zero are expected) but got = " &
                             Convert.ToString(s(i), CultureInfo.CurrentCulture)
                    CoreServices.Log(strErr)
                    Return Nothing
                End If

                If s(i) < 0 OrElse s(i) > 1 Then
                    strErr = "Unexpected censoring indictor (0/1 values are expected) but got = " &
                             Convert.ToString(s(i), CultureInfo.CurrentCulture)
                    CoreServices.Log(strErr)
                    Return Nothing
                End If

                Dim sr As New SurvivalRecord With {
                    .Time = t(i),
                    .Censorship = s(i),
                    .strGroup = g(i),
                    .Group = grpIds.IndexOf(g(i)),
                    .strStratum = strat(i),
                    .Stratum = Convert.ToString(stratumIds.IndexOf(strat(i)), CultureInfo.CurrentCulture)
                }
                output.Add(sr)
            Next

            Return output
        End Function

    End Module

End Namespace
