Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Shared validation and small utility helpers used across the resampling infrastructure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The bootstrap and jackknife runner modules originally carried several near-identical private helpers for
    ''' delegate validation, finite-statistic checks, parameter-label checks, cluster-block validation, identity-index
    ''' construction, and progress callbacks. Centralizing those routines here keeps the shared resampling layer
    ''' consistent and prevents behavior from drifting between bootstrap and jackknife execution paths.
    ''' </para>
    ''' <para>
    ''' This module deliberately preserves the current resampling exception semantics by routing failures through
    ''' <c>AppGlobals.BSerr.LogAndThrow(...)</c> rather than using plain <c>Throw</c>.
    ''' </para>
    ''' </remarks>
    Public Module ResamplingValidation

        Friend Sub ValidateStatisticDelegate(Of T)(statistic As T, paramName As String)
            If statistic Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(paramName))
        End Sub

        Friend Sub ValidateMinimumSuccessfulReplicates(value As Integer,
                                                       Optional paramName As String = "value")
            ValidatePositiveReplicates(value, paramName, 1)
        End Sub

        Friend Sub ValidateFiniteScalar(value As Double, contextLabel As String)
            If Not IsFinite(value) Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException($"The {contextLabel} must be finite."))
            End If
        End Sub

        Friend Sub ValidateFiniteVector(values As Double(),
                                        contextLabel As String,
                                        Optional paramName As String = "values")
            If values Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(paramName))
            If values.Length = 0 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException($"The {contextLabel} must contain at least one parameter."))
            End If
            ValidateFiniteStatistics(values, paramName)
        End Sub

        Friend Sub ValidateParameterLabels(parameterLabels As String(),
                                          parameterCount As Integer,
                                          Optional paramName As String = "parameterLabels")
            If parameterLabels Is Nothing OrElse parameterLabels.Length = 0 Then Exit Sub
            If parameterLabels.Length <> parameterCount Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("ParameterLabels must be Nothing or have the same length as the observed parameter vector.", paramName))
            End If
        End Sub

        Friend Function BuildIdentityIndices(sampleSize As Integer,
                                             minimumSampleSize As Integer,
                                             Optional paramName As String = "sampleSize") As Integer()
            ValidatePositiveReplicates(sampleSize, paramName, minimumSampleSize)

            Dim idx(sampleSize - 1) As Integer
            For i As Integer = 0 To sampleSize - 1
                idx(i) = i
            Next
            Return idx
        End Function

        Friend Function BuildOriginalIndicesFromBlocks(clusterBlocks As List(Of Integer()),
                                                       minimumSampleSize As Integer,
                                                       Optional paramName As String = "clusterBlocks") As Integer()
            ValidateClusterBlocks(clusterBlocks, 1, paramName)

            Dim total As Integer = 0
            For Each block As Integer() In clusterBlocks
                If block Is Nothing OrElse block.Length = 0 Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Cluster blocks must not contain empty blocks.", paramName))
                End If
                total += block.Length
            Next

            Return BuildIdentityIndices(total, minimumSampleSize)
        End Function

        Friend Sub ValidateClusterBlocks(clusterBlocks As List(Of Integer()),
                                         Optional minimumClusterCount As Integer = 1,
                                         Optional paramName As String = "clusterBlocks")
            If clusterBlocks Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(paramName))

            If clusterBlocks.Count = 0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least one cluster block is required.", paramName))
            End If

            If minimumClusterCount > 1 AndAlso clusterBlocks.Count < minimumClusterCount Then
                If minimumClusterCount = 2 Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two clusters are required for cluster jackknife.", paramName))
                Else
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException($"At least {minimumClusterCount} clusters are required.", paramName))
                End If
            End If
        End Sub

        Friend Sub ValidateLeaveOneOutEstimates(leaveOneOutEstimates As Double(),
                                                paramName As String)
            If leaveOneOutEstimates Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(paramName))
            If leaveOneOutEstimates.Length < 2 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two leave-one-out estimates are required.", paramName))
            End If

            ValidateFiniteStatistics(leaveOneOutEstimates,
                                     paramName,
                                     "At least two leave-one-out estimates are required.",
                                     "Leave-one-out estimates must contain only finite values.")
        End Sub

        Friend Sub ReportProgress(progressCallback As Action(Of Integer, Integer),
                                  completed As Integer,
                                  total As Integer)
            If progressCallback Is Nothing Then Exit Sub
            progressCallback(completed, total)
        End Sub

    End Module

End Namespace
