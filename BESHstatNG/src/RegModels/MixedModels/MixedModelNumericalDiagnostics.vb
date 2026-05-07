Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text

Namespace regression

    ''' <summary>
    ''' Result details from a tolerance-controlled matrix inversion attempt.
    ''' </summary>
    Public Class MixedModelNumericalInverseResult
        Public Property Success As Boolean = False
        Public Property Method As String = String.Empty
        Public Property Rank As Integer = 0
        Public Property ConditionNumber As Double = Double.NaN
        Public Property UsedPseudoInverse As Boolean = False
        Public Property DiagnosticMessage As String = String.Empty
    End Class

    ''' <summary>
    ''' Numerical helper methods used by mixed-model post-estimation and Kenward-Roger inference.
    ''' </summary>
    ''' <remarks>
    ''' These routines keep the high-risk linear algebra decisions in one place: use Cholesky first
    ''' for symmetric positive-definite matrices, fall back to an SVD pseudoinverse only when allowed,
    ''' and report rank/condition-number diagnostics so GUI and UDF output can warn users instead of
    ''' failing with an opaque matrix exception.
    ''' </remarks>
    Public Module MixedModelNumericalDiagnostics

        Public Const DefaultRelativeTolerance As Double = 0.0000000001
        Public Const DefaultAbsoluteTolerance As Double = 0.000000000001
        Public Const NearSingularConditionWarningThreshold As Double = 10000000000.0

        Public Function TryInvertSymmetric(a(,) As Double,
                                           ByRef inv(,) As Double,
                                           Optional ByRef diagnostic As String = Nothing,
                                           Optional allowPseudoInverse As Boolean = True,
                                           Optional relativeTolerance As Double = DefaultRelativeTolerance,
                                           Optional absoluteTolerance As Double = DefaultAbsoluteTolerance,
                                           Optional inverseResult As MixedModelNumericalInverseResult = Nothing) As Boolean
            inv = Nothing
            diagnostic = String.Empty

            Dim detail As MixedModelNumericalInverseResult = If(inverseResult, New MixedModelNumericalInverseResult())
            detail.Success = False
            detail.Method = String.Empty
            detail.Rank = 0
            detail.ConditionNumber = Double.NaN
            detail.UsedPseudoInverse = False
            detail.DiagnosticMessage = String.Empty

            If a Is Nothing Then
                diagnostic = "Matrix is Nothing."
                detail.DiagnosticMessage = diagnostic
                Return False
            End If

            If a.GetLength(0) <> a.GetLength(1) Then
                diagnostic = "Matrix is not square."
                detail.DiagnosticMessage = diagnostic
                Return False
            End If

            If Not Matrix.MatrixIsFinite(a) Then
                diagnostic = "Matrix contains non-finite values."
                detail.DiagnosticMessage = diagnostic
                Return False
            End If

            Dim work(,) As Double = DirectCast(a.Clone(), Double(,))
            MixedModelEngine.SymmetrizeInPlace(work)

            Dim condition As Double = EstimateConditionNumberBySvd(work, relativeTolerance, absoluteTolerance)
            Dim rank As Integer = NumericRankBySvd(work, relativeTolerance, absoluteTolerance)
            detail.ConditionNumber = condition
            detail.Rank = rank

            Dim chol(,) As Double = Nothing
            Dim trace As String = String.Empty
            If MixedModelCovariance.TryCholesky(work, chol, trace) Then
                Try
                    inv = Global.BESHStatNG.Matrix.Matrix.CholInv(chol)
                    If inv IsNot Nothing AndAlso Matrix.MatrixIsFinite(inv) Then
                        detail.Success = True
                        detail.Method = "Cholesky"
                        detail.UsedPseudoInverse = False
                        diagnostic = BuildInverseDiagnostic(detail)
                        detail.DiagnosticMessage = diagnostic
                        Return True
                    End If
                Catch ex As Exception
                    trace = If(trace, String.Empty) & " Cholesky inverse failed: " & ex.Message
                End Try
            End If

            If Not allowPseudoInverse Then
                diagnostic = "Cholesky inversion failed and pseudoinverse fallback is disabled. " & If(trace, String.Empty)
                detail.DiagnosticMessage = diagnostic
                Return False
            End If

            Try
                Dim singularTol As Double = ResolveSvdTolerance(work, relativeTolerance, absoluteTolerance)
                inv = Global.BESHStatNG.Matrix.Matrix.pseudoInverse(work, singularTol)
                If inv Is Nothing OrElse Not Matrix.MatrixIsFinite(inv) Then
                    diagnostic = "SVD pseudoinverse returned an invalid matrix."
                    detail.DiagnosticMessage = diagnostic
                    Return False
                End If

                MixedModelEngine.SymmetrizeInPlace(inv)
                detail.Success = True
                detail.Method = "SVD pseudoinverse"
                detail.UsedPseudoInverse = True
                diagnostic = BuildInverseDiagnostic(detail)
                detail.DiagnosticMessage = diagnostic
                Return True

            Catch ex As Exception
                diagnostic = "SVD pseudoinverse failed: " & ex.Message & " " & If(trace, String.Empty)
                detail.DiagnosticMessage = diagnostic
                Return False
            End Try
        End Function

        Public Function EstimateConditionNumberBySvd(a(,) As Double,
                                                     Optional relativeTolerance As Double = DefaultRelativeTolerance,
                                                     Optional absoluteTolerance As Double = DefaultAbsoluteTolerance) As Double
            If a Is Nothing Then Return Double.NaN

            Try
                Dim copy(,) As Double = DirectCast(a.Clone(), Double(,))
                Dim svd As Global.BESHStatNG.Matrix.Matrix.SVDoutput = Global.BESHStatNG.Matrix.Matrix.SVD_decomp(copy)
                If svd Is Nothing OrElse svd.Wvect Is Nothing OrElse svd.Wvect.Length = 0 Then Return Double.NaN

                Dim maxS As Double = 0.0
                For Each s As Double In svd.Wvect
                    If AppInfrastructure.IsFinite(s) Then maxS = Math.Max(maxS, Math.Abs(s))
                Next
                If maxS <= 0.0 Then Return Double.PositiveInfinity

                Dim tol As Double = Math.Max(absoluteTolerance, Math.Abs(relativeTolerance) * maxS * CDbl(Math.Max(a.GetLength(0), a.GetLength(1))))
                Dim minS As Double = Double.PositiveInfinity

                For Each s As Double In svd.Wvect
                    Dim asv As Double = Math.Abs(s)
                    If AppInfrastructure.IsFinite(asv) AndAlso asv > tol Then minS = Math.Min(minS, asv)
                Next

                If Double.IsPositiveInfinity(minS) OrElse minS <= 0.0 Then Return Double.PositiveInfinity
                Return maxS / minS

            Catch
                Return Double.NaN
            End Try
        End Function

        Public Function NumericRankBySvd(a(,) As Double,
                                         Optional relativeTolerance As Double = DefaultRelativeTolerance,
                                         Optional absoluteTolerance As Double = DefaultAbsoluteTolerance) As Integer
            If a Is Nothing Then Return 0

            Try
                Dim copy(,) As Double = DirectCast(a.Clone(), Double(,))
                Dim svd As Global.BESHStatNG.Matrix.Matrix.SVDoutput = Global.BESHStatNG.Matrix.Matrix.SVD_decomp(copy)
                If svd Is Nothing OrElse svd.Wvect Is Nothing Then Return 0

                Dim maxS As Double = 0.0
                For Each s As Double In svd.Wvect
                    If AppInfrastructure.IsFinite(s) Then maxS = Math.Max(maxS, Math.Abs(s))
                Next
                If maxS <= 0.0 Then Return 0

                Dim tol As Double = Math.Max(absoluteTolerance, Math.Abs(relativeTolerance) * maxS * CDbl(Math.Max(a.GetLength(0), a.GetLength(1))))
                Dim rank As Integer = 0
                For Each s As Double In svd.Wvect
                    If AppInfrastructure.IsFinite(s) AndAlso Math.Abs(s) > tol Then rank += 1
                Next
                Return rank

            Catch
                Return 0
            End Try
        End Function

        Public Function BuildMatrixSignature(a(,) As Double, Optional digits As Integer = 12) As String
            If a Is Nothing Then Return "<null>"
            Dim sb As New StringBuilder()
            sb.Append(a.GetLength(0).ToString(CultureInfo.InvariantCulture)).Append("x").Append(a.GetLength(1).ToString(CultureInfo.InvariantCulture)).Append(":"c)

            Dim format As String = "G" & Math.Max(1, Math.Min(17, digits)).ToString(CultureInfo.InvariantCulture)
            For r As Integer = 0 To a.GetLength(0) - 1
                For c As Integer = 0 To a.GetLength(1) - 1
                    If r > 0 OrElse c > 0 Then sb.Append(";"c)
                    sb.Append(a(r, c).ToString(format, CultureInfo.InvariantCulture))
                Next
            Next
            Return sb.ToString()
        End Function

        Public Function WarningForConditionNumber(label As String, conditionNumber As Double) As String
            If Not AppInfrastructure.IsFinite(conditionNumber) Then
                If Double.IsPositiveInfinity(conditionNumber) Then Return label & " is numerically rank deficient."
                Return String.Empty
            End If

            If conditionNumber >= NearSingularConditionWarningThreshold Then
                Return label & " is near singular; estimated condition number = " & conditionNumber.ToString("G6", CultureInfo.InvariantCulture) & "."
            End If

            Return String.Empty
        End Function

        Public Sub AddUniqueWarning(warnings As IList(Of String), message As String)
            If warnings Is Nothing OrElse String.IsNullOrWhiteSpace(message) Then Exit Sub
            For Each one As String In warnings
                If String.Equals(one, message, StringComparison.Ordinal) Then Exit Sub
            Next
            warnings.Add(message)
        End Sub

        Private Function ResolveSvdTolerance(a(,) As Double, relativeTolerance As Double, absoluteTolerance As Double) As Double
            Try
                Dim copy(,) As Double = DirectCast(a.Clone(), Double(,))
                Dim svd As Global.BESHStatNG.Matrix.Matrix.SVDoutput = Global.BESHStatNG.Matrix.Matrix.SVD_decomp(copy)
                Dim maxS As Double = 0.0
                If svd IsNot Nothing AndAlso svd.Wvect IsNot Nothing Then
                    For Each s As Double In svd.Wvect
                        If AppInfrastructure.IsFinite(s) Then maxS = Math.Max(maxS, Math.Abs(s))
                    Next
                End If
                Return Math.Max(absoluteTolerance, Math.Abs(relativeTolerance) * maxS * CDbl(Math.Max(a.GetLength(0), a.GetLength(1))))
            Catch
                Return Math.Max(absoluteTolerance, Math.Abs(relativeTolerance))
            End Try
        End Function

        Private Function BuildInverseDiagnostic(detail As MixedModelNumericalInverseResult) As String
            Dim conditionText As String = If(AppInfrastructure.IsFinite(detail.ConditionNumber),
                                             detail.ConditionNumber.ToString("G6", CultureInfo.InvariantCulture),
                                             If(Double.IsPositiveInfinity(detail.ConditionNumber), "Infinity", "NaN"))
            Return "Inversion method=" & detail.Method & "; rank=" & detail.Rank.ToString(CultureInfo.InvariantCulture) & "; condition=" & conditionText & "."
        End Function

    End Module

End Namespace
