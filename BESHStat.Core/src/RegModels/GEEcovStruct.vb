Option Explicit On
Option Strict On

Imports System
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Internal formatting helpers used only for diagnostic logging in the GEE engine.
    ''' </summary>
    Friend Module GEECoreFormatting

        Friend Function ArrayToString(Of T)(values() As T) As String
            If values Is Nothing Then Return String.Empty
            Return String.Join(", ", values.Select(Function(value) Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture)))
        End Function

        Friend Function ArrayToString(Of T)(values(,) As T) As String
            If values Is Nothing Then Return String.Empty

            Dim builder As New System.Text.StringBuilder()
            For row As Integer = 0 To values.GetUpperBound(0)
                If row > 0 Then builder.Append("; ")
                For col As Integer = 0 To values.GetUpperBound(1)
                    If col > 0 Then builder.Append(", ")
                    builder.Append(Convert.ToString(values(row, col), System.Globalization.CultureInfo.CurrentCulture))
                Next
            Next

            Return builder.ToString()
        End Function

    End Module

    Public Module GEEcovStructUtils
        Public Function createGEEcovMat(type As String) As regression.GEEcovStruct
            Dim key As String = NormalizeGeeCovarianceName(type)

            Select Case key
                Case "Independence"
                    Return New regression.Independence
                Case "Exchangable"
                    Return New regression.Exchangable
                Case "Autoregressive"
                    Return New regression.Autoregressive
                Case "Toeplitz"
                    Return New regression.Toeplitz
                Case "Unstructured"
                    Return New regression.Unstructured
                Case Else
                    CoreServices.Errors.LogAndThrow(New ApplicationException("Unsupported gee correlation type = " & If(type, String.Empty)))
                    Return Nothing
            End Select
        End Function

        Friend Function NormalizeGeeCovarianceName(type As String) As String
            Dim s As String = If(type, String.Empty).Trim()
            If s = String.Empty Then Return "Independence"

            Dim key As String = New String(s.ToLowerInvariant().Where(Function(ch) Char.IsLetterOrDigit(ch)).ToArray())

            Select Case key
                Case "independence", "independent", "id", "identity"
                    Return "Independence"
                Case "exchangable", "exchangeable", "exch", "cs", "compoundsymmetry", "exchangeablecs", "compoundsymmetrycs"
                    Return "Exchangable"
                Case "autoregressive", "ar", "ar1", "arone", "autoregressivear1", "ar1autoregressive"
                    Return "Autoregressive"
                Case "toeplitz", "toep", "toeplitztoep", "toeptoeplitz", "stationary", "stationarymdependent"
                    Return "Toeplitz"
                Case "unstructured", "un", "uns", "unstructuredun"
                    Return "Unstructured"
                Case Else
                    Return s
            End Select
        End Function
    End Module

    ''' <summary>
    ''' Abstract base class for all Generalized Estimating Equation (GEE)
    ''' working‑correlation (covariance) structures.  
    ''' Provides the interface for:
    ''' <list type="bullet">
    '''   <item><description>Constructing the working covariance matrix</description></item>
    '''   <item><description>Solving weighted residual and design‑matrix systems</description></item>
    '''   <item><description>Updating association (correlation) parameters</description></item>
    '''   <item><description>Returning the current dependence‑parameter matrix</description></item>
    ''' </list>
    ''' Concrete subclasses implement specific structures such as independence,
    ''' exchangeable, AR(1), Toeplitz, and unstructured covariance.
    ''' </summary>
    Public MustInherit Class GEEcovStruct

        Protected Friend pDepParams(,) As Double = Nothing 'for Exchangeble, Autoregressive structure it is double; for Unstructured it is a double array
        Public Shared CovStructsList() As String = {"Independence", "Exchangable", "Autoregressive", "Toeplitz", "Unstructured"}

        ''' <summary>
        ''' Solves the weighted design‑matrix and residual systems using the
        ''' inverse of the working covariance matrix.
        ''' </summary>
        ''' <param name="expval">Expected values for the cluster.</param>
        ''' <param name="index">Cluster index.</param>
        ''' <param name="gee">The parent <c>GEE</c> model.</param>
        ''' <param name="stDev">Standard deviations for the variance function.</param>
        ''' <param name="wdmat">Weighted design matrix.</param>
        ''' <param name="wresid">Weighted residual vector.</param>
        ''' <param name="res_wdmat">Output: transformed design matrix.</param>
        ''' <param name="res_wresid">Output: transformed residual vector.</param>
        ''' <param name="strTrace">Optional trace/debug output.</param>
        Public MustOverride Sub covarianceMatrixSolve(expval() As Double, index As Integer, gee As GEE, stDev() As Double,
                          wdmat(,) As Double, wresid() As Double,
                          ByRef res_wdmat(,) As Double, ByRef res_wresid() As Double,
                          ByRef Optional strTrace As String = Nothing)

        ''' <summary>
        ''' Updates the association (correlation) parameters for the covariance structure
        ''' using the current residuals and fitted values.
        ''' </summary>
        ''' <param name="gee">The parent <c>GEE</c> model.</param>
        ''' <param name="strTrace">Optional trace/debug output.</param>
        Public MustOverride Sub updateAssoc(gee As GEE, ByRef Optional strTrace As String = Nothing)

        ''' <summary>
        ''' Computes the working covariance matrix for a given cluster.
        ''' </summary>
        ''' <param name="endog_expval">Expected values for the cluster.</param>
        ''' <param name="gee">The parent <c>GEE</c> model.</param>
        ''' <param name="index">Cluster index.</param>
        ''' <returns>The working covariance matrix.</returns>
        Public MustOverride Function covarianceMatrix(endog_expval() As Double, gee As GEE, index As Integer) As Double(,)

        ''' <summary>
        ''' Returns the current dependence‑parameter matrix for the covariance structure.
        ''' </summary>
        ''' <param name="gee">The parent <c>GEE</c> model.</param>
        ''' <param name="bFullCov">
        ''' If True, returns the full working‑correlation matrix.  
        ''' Subclasses may override to return reduced or structured forms.
        ''' </param>
        ''' <returns>A matrix of dependence parameters.</returns>
        Public Overridable Function DepParams(gee As GEE, Optional bFullCov As Boolean = True) As Double(,)
            Return Me.pDepParams
        End Function

    End Class


    ''' <summary>
    ''' Implements the independence working‑correlation structure for GEE.
    ''' The working covariance matrix is the identity matrix and no association
    ''' parameters are estimated.
    ''' </summary>
    Public Class Independence
        Inherits GEEcovStruct
        Private Shadows pDepParams As Double = 1

        Public Overrides Function tostring() As String
            Return "Independence"
        End Function

        ''' <summary>
        ''' Returns the identity matrix as the dependence‑parameter matrix.
        ''' </summary>
        Public Overrides Function DepParams(gee As GEE, Optional bFullCov As Boolean = True) As Double(,)
            Return Matrix.MatrixArithmeticCore.IdentityMatrix(gee.TimesDict.Count - 1)
        End Function

        ''' <summary>
        ''' Solves the covariance system by dividing each row/column by the
        ''' variance function (diagonal covariance).
        ''' </summary>
        Public Overrides Sub covarianceMatrixSolve(expval() As Double, index As Integer, gee As GEE, stDev() As Double,
                                               wdmat(,) As Double, wresid() As Double,
                                               ByRef res_wdmat(,) As Double, ByRef res_wresid() As Double,
                                               ByRef Optional strTrace As String = Nothing)

            Dim V(stDev.GetUpperBound(0)) As Double, tmpTrace As String = String.Empty
            For i = 0 To stDev.GetUpperBound(0)
                V(i) = stDev(i) ^ 2
            Next

            res_wdmat = Matrix.MatrixArithmeticCore.Divide(wdmat, V, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
            res_wresid = Matrix.MatrixArithmeticCore.Divide(wresid, V, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
        End Sub

        ''' <summary>
        ''' Independence structure has no association parameter to update.
        ''' </summary>
        Public Overrides Sub updateAssoc(gee As GEE, ByRef Optional strTrace As String = Nothing)
            'there is nothing to update for Independence structure
        End Sub

        ''' <summary>
        ''' Returns an identity working‑correlation matrix for the cluster.
        ''' </summary>
        Public Overrides Function covarianceMatrix(endog_expval() As Double, gee As GEE, index As Integer) As Double(,)
            Return Matrix.MatrixArithmeticCore.IdentityMatrix(gee.TimesDict.Count - 1)
        End Function
    End Class


    ''' <summary>
    ''' Implements the exchangeable (compound‑symmetry) working‑correlation
    ''' structure for GEE.  
    ''' All off‑diagonal correlations equal ρ, and diagonal entries equal 1.
    ''' </summary>
    Public Class Exchangable
        Inherits GEEcovStruct
        Private Shadows pDepParams As Double

        Public Overrides Function tostring() As String
            Return "Exchangable"
        End Function

        ''' <summary>
        ''' Returns the full exchangeable correlation matrix with diagonal 1
        ''' and off‑diagonal entries equal to the estimated correlation parameter ρ.
        ''' </summary>
        Public Overrides Function DepParams(gee As GEE, Optional bFullCov As Boolean = True) As Double(,)
            Dim out(gee.TimesDict.Count - 1, gee.TimesDict.Count - 1) As Double
            For i = 0 To gee.TimesDict.Count - 1
                For j = 0 To gee.TimesDict.Count - 1
                    out(i, j) = If(i = j, 1, Me.pDepParams)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Solves the covariance system using the closed‑form inverse of the
        ''' exchangeable correlation matrix.
        ''' </summary>
        Public Overrides Sub covarianceMatrixSolve(expval() As Double, index As Integer, gee As GEE, stDev() As Double,
                                               wdmat(,) As Double, wresid() As Double,
                                               ByRef res_wdmat(,) As Double, ByRef res_wresid() As Double,
                                               ByRef Optional strTrace As String = Nothing)
            Dim k As Integer = expval.Length
            Dim c As Double = (Me.pDepParams / (1.0 - Me.pDepParams)) / (1.0 + Me.pDepParams * (k - 1))

            'Process WDMAT and WRESID
            res_wdmat = CovMatSolveExchangable(c, wdmat, stDev)
            res_wresid = CovMatSolveExchangable(c, wresid, stDev)
        End Sub

        ''' <summary>
        ''' Updates the exchangeable correlation parameter ρ using the method of
        ''' moments based on standardized residuals.  
        ''' Supports both Pearson‑based and model‑based scale estimation.
        ''' </summary>
        Public Overrides Sub updateAssoc(gee As GEE, ByRef Optional strTrace As String = Nothing)
            Dim fsum1 As Double, fsum2 As Double, residsq_sum As Double, ngrp As Long, npr As Double, n_pairs As Double
            Dim tmpTrace As String = String.Empty, scaleEst As Double
            Dim tmpCachedMeans As List(Of (Double(), Double(,))) = gee.CachedMeans
            Dim tmpEndogLi As List(Of Double()) = gee.EndogClustered

            For i = 0 To gee.NoGroup - 1
                Dim expval() As Double = tmpCachedMeans(i).Item1
                Dim endog() As Double = tmpEndogLi(i)

                Dim sdev(expval.GetUpperBound(0)) As Double
                For j = 0 To expval.GetUpperBound(0)
                    Dim v As Double = gee.Family.Variance(expval(j))
                    If v < 0.000000000001 Then v = 0.000000000001
                    sdev(j) = Math.Sqrt(v)
                Next

                Dim resid() As Double = Matrix.MatrixArithmeticCore.Divide(Matrix.MatrixArithmeticCore.Subtract(endog, expval), sdev, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace
                Dim ssr As Double = SumSq(resid)
                ngrp = resid.Length
                scaleEst += ssr
                fsum1 += endog.Length

                residsq_sum += ((resid.Sum() ^ 2 - ssr) / 2.0)
                npr = 0.5 * ngrp * (ngrp - 1)
                fsum2 += npr
                n_pairs += npr
            Next

            If gee.UseP Then
                scaleEst /= (fsum1 * gee.DFresid / gee.Nobs)
                residsq_sum /= scaleEst
                Me.pDepParams = residsq_sum / (fsum2 * (n_pairs - gee.Nparams) / n_pairs)
            Else
                scaleEst /= fsum1
                residsq_sum /= scaleEst
                Me.pDepParams = residsq_sum / fsum2
            End If
        End Sub

        ''' <summary>
        ''' Returns the exchangeable working‑correlation matrix for the cluster.
        ''' </summary>
        Public Overrides Function covarianceMatrix(endog_expval() As Double, gee As GEE, index As Integer) As Double(,)
            Dim out(endog_expval.GetUpperBound(0), endog_expval.GetUpperBound(0)) As Double
            For i = 0 To endog_expval.GetUpperBound(0)
                For j = 0 To endog_expval.GetUpperBound(0)
                    out(i, j) = If(i = j, 1, Me.pDepParams)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Helper routine for solving the exchangeable covariance system when the
        ''' input is a vector (1‑D case).
        ''' </summary>
        Private Function CovMatSolveExchangable(c As Double, inM() As Double, stDev() As Double, ByRef Optional strTrace As String = "") As Double()
            'helper sub to process Exchangable covariance structure when inM is 1D
            Dim tmp2() As Double, tmpTrace As String = String.Empty

            Dim tmp() As Double = Matrix.MatrixArithmeticCore.Divide(inM, stDev)
            Dim sumTot As Double = tmp.Sum()
            ReDim tmp2(tmp.GetUpperBound(0))
            For i = 0 To tmp.GetUpperBound(0)
                tmp2(i) = (tmp(i) / (1 - Me.pDepParams)) - (c * sumTot)
            Next

            CovMatSolveExchangable = Matrix.MatrixArithmeticCore.Divide(tmp2, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace
        End Function

        ''' <summary>
        ''' Helper routine for solving the exchangeable covariance system when the
        ''' input is a matrix (2‑D case).
        ''' </summary>
        Private Function CovMatSolveExchangable(c As Double, inM(,) As Double, stDev() As Double, ByRef Optional strTrace As String = "") As Double(,)
            'helper sub to process Exchangable covariance structure when inM is 2D
            Dim tmpTrace As String = String.Empty

            Dim tmp(,) As Double = Matrix.MatrixArithmeticCore.Divide(inM, stDev)
            Dim tmp2(tmp.GetUpperBound(0), tmp.GetUpperBound(1)) As Double, arrSumtot(tmp.GetUpperBound(1)) As Double
            'Get column sums
            For i = 0 To tmp.GetUpperBound(0)
                For j = 0 To tmp.GetUpperBound(1)
                    arrSumtot(j) += tmp(i, j)
                Next
            Next

            For i = 0 To tmp.GetUpperBound(0)
                For j = 0 To tmp.GetUpperBound(1)
                    tmp2(i, j) = (tmp(i, j) / (1 - Me.pDepParams)) - (c * arrSumtot(j))
                Next
            Next

            CovMatSolveExchangable = Matrix.MatrixArithmeticCore.Divide(tmp2, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace
        End Function

    End Class


    ''' <summary>
    ''' Implements the AR(1) working‑correlation structure for GEE.  
    ''' Correlation between time points i and j is ρ^{|i−j|}.  
    ''' Assumes equally spaced measurement times.
    ''' </summary>
    Public Class Autoregressive
        Inherits GEEcovStruct
        Private Shadows pDepParams As Double

        Public Overrides Function tostring() As String
            Return "Autoregressive"
        End Function

        ''' <summary>
        ''' Returns the AR(1) correlation matrix with entries ρ^{|i−j|}.
        ''' </summary>
        Public Overrides Function DepParams(gee As GEE, Optional bFullCov As Boolean = True) As Double(,)
            Dim out(gee.TimesDict.Count - 1, gee.TimesDict.Count - 1) As Double
            For i = 0 To gee.TimesDict.Count - 1
                For j = i To gee.TimesDict.Count - 1
                    If i = j Then
                        out(i, j) = 1
                    Else
                        out(i, j) = Me.pDepParams ^ (j - i)
                        out(j, i) = out(i, j)
                    End If
                Next
            Next
            Return out
        End Function


        ''' <summary>
        ''' Solves the covariance system using the closed‑form inverse of the AR(1)
        ''' correlation matrix.  
        ''' Handles the special cases k=1 and k=2 separately.
        ''' </summary>
        Public Overrides Sub covarianceMatrixSolve(expval() As Double, index As Integer, gee As GEE, stDev() As Double,
                                               wdmat(,) As Double, wresid() As Double,
                                               ByRef res_wdmat(,) As Double, ByRef res_wresid() As Double,
                                               ByRef Optional strTrace As String = Nothing)
            'The AR(1) working correlation matrix structure is computed assuming the measurements are equally spaced for all subjects.
            Dim tmpTrace As String = String.Empty

            Dim k As Integer = expval.Length
            If k = 1 Then 'wdmat/wresid has one row
                Dim V(stDev.GetUpperBound(0)) As Double
                For i = 0 To stDev.GetUpperBound(0)
                    V(i) = stDev(i) * stDev(i)
                Next

                res_wdmat = Matrix.MatrixArithmeticCore.Divide(wdmat, V, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
                res_wresid = Matrix.MatrixArithmeticCore.Divide(wresid, V, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace

            ElseIf k = 2 Then 'wdmat/wresid has two rows
                Dim mat(1, 1) As Double
                For i = 0 To 1
                    For j = 0 To 1
                        mat(i, j) = If(i = j, 1.0 / (1.0 - Me.pDepParams ^ 2), -Me.pDepParams / (1.0 - Me.pDepParams ^ 2))
                    Next
                Next
                res_wdmat = covMatSolveAR1_2(wdmat, stDev, mat, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
                res_wresid = DataManagement.ArrayUtilities.GetColumn(covMatSolveAR1_2(wresid, stDev, mat, tmpTrace), 0)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace

            ElseIf k >= 3 Then ' >= 3 rows: values c0, c1, c2 defined below give the inverse.
                ' c0 is on the diagonal, except for the 1st and last position.
                ' c1 is on the first and last position of the diagonal.
                ' c2 is on the sub/super diagonal.
                Dim c0 As Double = (1.0 + pDepParams ^ 2) / (1.0 - pDepParams ^ 2)
                Dim c1 As Double = 1.0 / (1.0 - pDepParams ^ 2)
                Dim c2 As Double = -pDepParams / (1.0 - pDepParams ^ 2)

                res_wdmat = covMatSolveAR1_3(wdmat, stDev, c0, c1, c2, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
                res_wresid = covMatSolveAR1_3(wresid, stDev, c0, c1, c2, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
            End If
        End Sub


        ''' <summary>
        ''' Updates the AR(1) correlation parameter ρ using lag‑0 and lag‑1
        ''' standardized residual products.  
        ''' Supports both Pearson‑based and model‑based scale estimation.
        ''' </summary>
        Public Overrides Sub updateAssoc(gee As GEE, ByRef Optional strTrace As String = Nothing)
            'This is a grid implementation. Assumptions are:
            ' - equal spacing between time points
            ' - time-points can be missing but only at the end (i.e. right-censored)
            Dim tmpTrace As String = String.Empty, totN As Integer, totN1 As Integer, lg0 As Double, lg1 As Double
            Dim tmpCachedMeans As List(Of (Double(), Double(,))) = gee.CachedMeans
            Dim tmpEndogLi As List(Of Double()) = gee.EndogClustered
            Dim scaleEst As Double = gee.EstimateScale()
            Dim lag0 As Double = 0.0
            Dim lag1 As Double = 0.0

            For i = 0 To gee.NoGroup - 1
                Dim expval() As Double = tmpCachedMeans(i).Item1
                Dim endog() As Double = tmpEndogLi(i)
                Dim n As Integer = expval.Length
                Dim resid(n - 1) As Double, sdev(n - 1) As Double

                For j = 0 To n - 1
                    Dim v As Double = scaleEst * gee.Family.Variance(expval(j))
                    If v < 0.000000000001 Then v = 0.000000000001
                    sdev(j) = Math.Sqrt(v)
                Next

                resid = Matrix.MatrixArithmeticCore.Divide(Matrix.MatrixArithmeticCore.Subtract(endog, expval), sdev, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace

                totN1 += (n - 1)
                totN += n
                If n > 1 Then
                    lg0 = 0 : lg1 = 0
                    For j = 0 To n - 1
                        If j < n - 1 Then lg1 += (resid(j) * resid(j + 1))
                        lg0 += resid(j) * resid(j)
                    Next

                    lag1 += lg1 'this should match SPSS and R
                    lag0 += lg0 'this should match SPSS and R 
                End If
            Next

            'pDepParams = lag1 / lag0 'this will match python statsmodels
            If gee.UseP Then
                Me.pDepParams = (lag1 / (totN1 - gee.Nparams)) / (lag0 / (totN - gee.Nparams)) 'this should match SPSS and R
            Else
                Me.pDepParams = (lag1 / totN1) / (lag0 / totN)
            End If
        End Sub

        ''' <summary>
        ''' AR(1) does not provide a direct covariance‑matrix constructor for
        ''' arbitrary time patterns; calling this method raises an exception.
        ''' </summary>
        Public Overrides Function covarianceMatrix(endog_expval() As Double, gee As GEE, index As Integer) As Double(,)
            CoreServices.Errors.LogAndThrow(New NotImplementedException("convarianceMatrix not applicable"))
            Return Nothing
        End Function

        ''' <summary>
        ''' Solves the AR(1) covariance system for clusters of size 2.
        ''' </summary>
        Private Function covMatSolveAR1_2(inM(,) As Double, stDev() As Double,
                                      mat(,) As Double, ByRef Optional strTrace As String = "") As Double(,)
            Dim tmpTrace As String = String.Empty

            Dim x(,) As Double = Matrix.MatrixArithmeticCore.Divide(inM, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace
            Dim x1(,) As Double = Matrix.MatrixArithmeticCore.Multiply(mat, x)
            covMatSolveAR1_2 = Matrix.MatrixArithmeticCore.Divide(x1, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace
        End Function

        ''' <summary>
        ''' Solves the AR(1) covariance system for clusters of size 2.
        ''' </summary>
        Private Function covMatSolveAR1_2(inM() As Double, stDev() As Double,
                                      mat(,) As Double, ByRef Optional strTrace As String = "") As Double(,)
            Dim tmpTrace As String = String.Empty

            Dim x() As Double = Matrix.MatrixArithmeticCore.Divide(inM, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace
            Dim x1(,) As Double = Matrix.MatrixArithmeticCore.Multiply(mat, x)
            covMatSolveAR1_2 = Matrix.MatrixArithmeticCore.Divide(x1, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace
        End Function

        ''' <summary>
        ''' Solves the AR(1) covariance system for clusters of size ≥ 3 using the
        ''' tridiagonal inverse of the AR(1) correlation matrix.
        ''' </summary>
        Private Function covMatSolveAR1_3(inM() As Double, stDev() As Double,
                                      c0 As Double, c1 As Double, c2 As Double, ByRef Optional strTrace As String = "") As Double()
            Dim tmpTrace As String = String.Empty
            Dim x() As Double = Matrix.MatrixArithmeticCore.Divide(inM, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace

            Dim y(x.GetUpperBound(0)) As Double, rhs1(x.GetUpperBound(0)) As Double, rhs2(x.GetUpperBound(0)) As Double
            For i = 0 To x.GetUpperBound(0) - 1
                rhs1(i) = x(i + 1)
                rhs2(i + 1) = x(i)
            Next

            For i = 0 To x.GetUpperBound(0)
                If i = 0 Then
                    y(i) = c1 * x(i) + c2 * x(i + 1)
                ElseIf i = x.GetUpperBound(0) Then
                    y(i) = c1 * x(i) + c2 * x(i - 1)
                Else
                    y(i) = c0 * x(i) + c2 * rhs1(i) + c2 * rhs2(i)
                End If
            Next

            covMatSolveAR1_3 = Matrix.MatrixArithmeticCore.Divide(y, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace

        End Function

        ''' <summary>
        ''' Solves the AR(1) covariance system for clusters of size ≥ 3 using the
        ''' tridiagonal inverse of the AR(1) correlation matrix.
        ''' </summary>
        Private Function covMatSolveAR1_3(inM(,) As Double, stDev() As Double,
                                      c0 As Double, c1 As Double, c2 As Double, ByRef Optional strTrace As String = "") As Double(,)

            Dim tmpTrace As String = String.Empty
            Dim x(,) As Double = Matrix.MatrixArithmeticCore.Divide(inM, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace

            Dim y(x.GetUpperBound(0), x.GetUpperBound(1)) As Double, rhs1(x.GetUpperBound(0), x.GetUpperBound(1)) As Double, rhs2(x.GetUpperBound(0), x.GetUpperBound(1)) As Double
            For i = 0 To x.GetUpperBound(0) - 1
                For j = 0 To x.GetUpperBound(1)
                    rhs1(i, j) = x(i + 1, j)
                    rhs2(i + 1, j) = x(i, j)
                Next
            Next
            For i = 0 To x.GetUpperBound(0)
                For j = 0 To x.GetUpperBound(1)
                    If i = 0 Then
                        y(i, j) = c1 * x(i, j) + c2 * x(i + 1, j)
                    ElseIf i = x.GetUpperBound(0) Then
                        y(i, j) = c1 * x(i, j) + c2 * x(i - 1, j)
                    Else
                        y(i, j) = c0 * x(i, j) + c2 * rhs1(i, j) + c2 * rhs2(i, j)
                    End If
                Next
            Next

            covMatSolveAR1_3 = Matrix.MatrixArithmeticCore.Divide(y, stDev, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
        End Function

    End Class

    ''' <summary>
    ''' Implements a stationary Toeplitz working-correlation structure for GEE.
    ''' Each lag has its own correlation parameter, so observations one time step apart share
    ''' one correlation, observations two time steps apart share another, and so on.
    ''' </summary>
    Public Class Toeplitz
        Inherits GEEcovStruct

        Public Overrides Function tostring() As String
            Return "Toeplitz"
        End Function

        Public Overrides Function DepParams(gee As GEE, Optional bFullCov As Boolean = True) As Double(,)
            Dim q As Integer = Math.Max(1, gee.TimesDict.Count)
            If Me.pDepParams Is Nothing OrElse Me.pDepParams.GetLength(0) <> q Then
                Me.pDepParams = Matrix.MatrixArithmeticCore.IdentityMatrix(q - 1)
            End If
            Return Me.pDepParams
        End Function

        Public Overrides Sub covarianceMatrixSolve(expval() As Double, index As Integer, gee As GEE, stDev() As Double,
                                               wdmat(,) As Double, wresid() As Double,
                                               ByRef res_wdmat(,) As Double, ByRef res_wresid() As Double,
                                               ByRef Optional strTrace As String = Nothing)

            Dim vco(,) As Double = Nothing, iErr As Integer, bSuccess As Boolean
            Dim vmat(,) As Double = covarianceMatrix(expval, gee, index)
            Dim tmp(,) As Double = Matrix.MatrixArithmeticCore.OuterProduct(stDev, stDev)

            For i = 0 To vmat.GetUpperBound(0)
                For k = 0 To vmat.GetUpperBound(1)
                    vmat(i, k) = vmat(i, k) * tmp(i, k)
                Next
            Next

            Dim threshold As Double = 0.01
            For i = 0 To 20
                iErr = 0
                vco = Matrix.MatrixFactorizationCore.Cholesky(vmat, iErr, False)
                If iErr > 0 Then
                    strTrace &= " WARNING: CHOLESKY. Toeplitz working correlation was not positive-definite. Calling CovNearest." & Environment.NewLine
                    bSuccess = False
                    vmat = StatFunc.CovNearest(vmat, threshold)
                    threshold *= 2
                Else
                    bSuccess = True
                    Exit For
                End If
            Next

            If Not bSuccess Then
                For i = 0 To vmat.GetUpperBound(0)
                    For k = 0 To vmat.GetUpperBound(1)
                        If i <> k Then vmat(i, k) = 0
                    Next
                Next
                CoreServices.Log($"WARNING: Toeplitz CovNearest was Not successful. Using diagonal working covariance. vmat={GEECoreFormatting.ArrayToString(vmat)}", AppInfrastructure.LogMsgType.Warn)
                strTrace &= $"WARNING: Toeplitz CovNearest was Not successful. Using diagonal working covariance. vmat={GEECoreFormatting.ArrayToString(vmat)}"
                vco = Matrix.MatrixFactorizationCore.Cholesky(vmat, iErr, False)
            End If

            res_wdmat = Matrix.MatrixFactorizationCore.CholeskySolve(vco, wdmat)
            res_wresid = Matrix.MatrixFactorizationCore.CholeskySolve(vco, wresid)
        End Sub

        Public Overrides Sub updateAssoc(gee As GEE, ByRef Optional strTrace As String = Nothing)
            Dim q As Integer = Math.Max(1, gee.TimesDict.Count)
            Dim corr(q - 1, q - 1) As Double
            For i = 0 To q - 1
                corr(i, i) = 1.0R
            Next

            If q <= 1 Then
                Me.pDepParams = corr
                Return
            End If

            Dim crossProducts(q - 1, q - 1) As Double
            Dim pairCounts(q - 1, q - 1) As Double
            Dim tmpTrace As String = String.Empty
            'For association-parameter estimation we need the Pearson dispersion even for
            'families such as Poisson and Binomial where the estimating-equation scale is
            'otherwise fixed at 1.  Using EstimateScale() here would return 1 for Poisson,
            'which over-inflates Toeplitz lag correlations on overdispersed count data and
            'then collapses the fitted lags to the +/-0.99 safety clamp.
            Dim scaleEst As Double = gee.EstimateScale(True)
            If Double.IsNaN(scaleEst) OrElse Double.IsInfinity(scaleEst) OrElse scaleEst <= 0.0R Then scaleEst = 1.0R

            Dim tmpCachedMeans As List(Of (Double(), Double(,))) = gee.CachedMeans
            Dim tmpEndogLi As List(Of Double()) = gee.EndogClustered
            Dim tmpTimeLi As List(Of Double()) = gee.TimeClustered
            Dim dict As Dictionary(Of Double, Integer) = gee.TimesDict

            For g = 0 To gee.NoGroup - 1
                Dim expval() As Double = tmpCachedMeans(g).Item1
                Dim endog() As Double = tmpEndogLi(g)
                Dim times() As Double = tmpTimeLi(g)
                Dim n As Integer = expval.Length
                If n <= 1 Then Continue For

                Dim resid(n - 1) As Double, sdev(n - 1) As Double
                For j = 0 To n - 1
                    Dim v As Double = gee.Family.Variance(expval(j))
                    If v < 0.000000000001 Then v = 0.000000000001
                    sdev(j) = Math.Sqrt(v)
                Next

                resid = Matrix.MatrixArithmeticCore.Divide(Matrix.MatrixArithmeticCore.Subtract(endog, expval), sdev, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace

                For a = 0 To n - 2
                    If Not dict.ContainsKey(times(a)) Then Continue For
                    Dim ia As Integer = dict.Item(times(a))
                    For b = a + 1 To n - 1
                        If Not dict.ContainsKey(times(b)) Then Continue For
                        Dim ib As Integer = dict.Item(times(b))
                        If ia = ib Then Continue For

                        crossProducts(ia, ib) += resid(a) * resid(b)
                        crossProducts(ib, ia) = crossProducts(ia, ib)
                        pairCounts(ia, ib) += 1.0R
                        pairCounts(ib, ia) = pairCounts(ia, ib)
                    Next
                Next
            Next

            For lag = 1 To q - 1
                Dim lagNumer As Double = 0.0R
                Dim lagDenom As Double = 0.0R

                For i = 0 To q - 1 - lag
                    Dim count As Double = pairCounts(i, i + lag)
                    If count <= 0.0R Then Continue For

                    Dim denom As Double
                    If gee.UseP Then
                        If count >= gee.Nparams Then
                            denom = (count - gee.Nparams) * scaleEst
                        Else
                            'Match the existing unstructured-working-correlation fallback for sparse pairs.
                            denom = count * scaleEst
                        End If
                    Else
                        denom = count * scaleEst
                    End If

                    If denom <= 0.0R OrElse Double.IsNaN(denom) OrElse Double.IsInfinity(denom) Then Continue For

                    lagNumer += crossProducts(i, i + lag)
                    lagDenom += denom
                Next

                Dim rho As Double = 0.0R
                If lagDenom > 0.0R Then rho = lagNumer / lagDenom
                If Double.IsNaN(rho) OrElse Double.IsInfinity(rho) Then rho = 0.0R
                If rho > 0.99R Then rho = 0.99R
                If rho < -0.99R Then rho = -0.99R

                For i = 0 To q - 1 - lag
                    corr(i, i + lag) = rho
                    corr(i + lag, i) = rho
                Next
            Next

            Dim iErr As Integer = 0
            Dim testChol(,) As Double = Matrix.MatrixFactorizationCore.Cholesky(corr, iErr, False)
            If iErr > 0 Then
                corr = StatFunc.CovNearest(corr, 0.01)
                For i = 0 To q - 1
                    corr(i, i) = 1.0R
                Next
            End If

            Me.pDepParams = corr
        End Sub

        Public Overrides Function covarianceMatrix(endog_expval() As Double, gee As GEE, index As Integer) As Double(,)
            Dim out(,) As Double
            If Me.pDepParams Is Nothing Then Me.pDepParams = Matrix.MatrixArithmeticCore.IdentityMatrix(Math.Max(1, gee.TimesDict.Count) - 1)

            If gee.hasTime Then
                Dim time_li As List(Of Double()) = gee.TimeClustered
                Dim dict As Dictionary(Of Double, Integer) = gee.TimesDict
                ReDim out(time_li(index).GetUpperBound(0), time_li(index).GetUpperBound(0))

                Dim i As Integer = 0
                For Each idi In time_li(index)
                    Dim j As Integer = 0
                    For Each idj In time_li(index)
                        out(i, j) = Me.pDepParams(dict.Item(idi), dict.Item(idj))
                        j += 1
                    Next idj
                    i += 1
                Next idi
            Else
                Dim endog_li As List(Of Double()) = gee.EndogClustered
                ReDim out(endog_li(index).GetUpperBound(0), endog_li(index).GetUpperBound(0))
                For i = 0 To endog_li(index).GetUpperBound(0)
                    For j = 0 To endog_li(index).GetUpperBound(0)
                        out(i, j) = Me.pDepParams(i, j)
                    Next
                Next
            End If

            Return out
        End Function
    End Class


    ''' <summary>
    ''' Implements the unstructured working‑correlation matrix for GEE.  
    ''' All pairwise correlations are estimated freely without constraints.
    ''' </summary>
    Public Class Unstructured
        Inherits GEEcovStruct

        Public Overrides Function tostring() As String
            Return "Unstructured"
        End Function

        ''' <summary>
        ''' Solves the covariance system by:
        ''' <list type="bullet">
        '''   <item><description>Constructing the full covariance matrix</description></item>
        '''   <item><description>Scaling by variance function</description></item>
        '''   <item><description>Attempting Cholesky factorization</description></item>
        '''   <item><description>Applying nearest‑positive‑definite correction if needed</description></item>
        ''' </list>
        ''' </summary>
        Public Overrides Sub covarianceMatrixSolve(expval() As Double, index As Integer, gee As GEE, stDev() As Double,
                                               wdmat(,) As Double, wresid() As Double,
                                               ByRef res_wdmat(,) As Double, ByRef res_wresid() As Double,
                                               ByRef Optional strTrace As String = Nothing)

            Dim vco(,) As Double = Nothing, iErr As Integer, bSuccess As Boolean

            Dim vmat(,) As Double = covarianceMatrix(expval, gee, index)
            Dim tmp(,) As Double = Matrix.MatrixArithmeticCore.OuterProduct(stDev, stDev)

            For i = 0 To vmat.GetUpperBound(0)
                For k = 0 To vmat.GetUpperBound(1)
                    vmat(i, k) = vmat(i, k) * tmp(i, k)
                Next
            Next

            Dim threshold As Double = 0.01
            'Factor the covariance matrix.  If the factorization fails, attempt to condition it into a factorizable matrix.
            For i = 0 To 20
                iErr = 0
                vco = Matrix.MatrixFactorizationCore.Cholesky(vmat, iErr, False)
                If iErr > 0 Then 'MatrixType not positive-definite. Compute pseudoinverse
                    strTrace = strTrace & " WARNING: CHOLESKY. bmat Not positive-definite. Calling CovNearest." & Environment.NewLine
                    strTrace = strTrace & " i=" & Convert.ToString(i, System.Globalization.CultureInfo.CurrentCulture) & " vmat=" &
                                GEECoreFormatting.ArrayToString(vmat) & " treshold=" & threshold.ToString(System.Globalization.CultureInfo.CurrentCulture) &
                                " bSuccess=" & bSuccess.ToString() & Environment.NewLine
                    bSuccess = False
                    vmat = StatFunc.CovNearest(vmat, threshold)
                    threshold *= 2
                Else
                    bSuccess = True
                    Exit For
                End If
            Next

            If Not bSuccess Then
                ' Last resort if we still cannot factor the covariance matrix.
                For i = 0 To vmat.GetUpperBound(0)
                    For k = 0 To vmat.GetUpperBound(1)
                        If i <> k Then vmat(i, k) = 0
                    Next
                Next
                CoreServices.Log($"WARNING: CovNearest was Not successful. Using vmat.  vmat={GEECoreFormatting.ArrayToString(vmat)}", AppInfrastructure.LogMsgType.Warn)
                strTrace &= $"WARNING: CovNearest was not successful. Using vmat.  vmat={GEECoreFormatting.ArrayToString(vmat)}"
                vco = Matrix.MatrixFactorizationCore.Cholesky(vmat, iErr, False)
            End If

            res_wdmat = Matrix.MatrixFactorizationCore.CholeskySolve(vco, wdmat)
            res_wresid = Matrix.MatrixFactorizationCore.CholeskySolve(vco, wresid)
        End Sub

        ''' <summary>
        ''' Updates the full unstructured correlation matrix using standardized
        ''' residual cross‑products aggregated across clusters.  
        ''' Applies SPSS‑style small‑sample corrections when <c>gee.UseP</c> is True.
        ''' </summary>
        Public Overrides Sub updateAssoc(gee As GEE, ByRef Optional strTrace As String = Nothing)
            Dim tmpTrace As String = String.Empty, wsum As Double
            Dim tmpTimeLi As List(Of Double()) = gee.TimeClustered
            Dim tmpCachedMeans As List(Of (Double(), Double(,))) = gee.CachedMeans
            Dim tmpEndogLi As List(Of Double()) = gee.EndogClustered

            Dim dict As Dictionary(Of Double, Integer) = gee.TimesDict 'dictionary - unique time values, counts
            Dim q As Integer = dict.Count
            Dim cov(q - 1, q - 1) As Double, csum(q - 1, q - 1) As Double 'it automaticialy initialize all elements to 0

            Dim scaleEst As Double = 0.0
            Dim lg0 As Double = 0.0
            For i = 0 To gee.NoGroup - 1
                Dim expval() As Double = tmpCachedMeans(i).Item1
                Dim endog() As Double = tmpEndogLi(i)
                Dim ix() As Double = tmpTimeLi(i)

                Dim resid(expval.GetUpperBound(0)) As Double, sdev(expval.GetUpperBound(0)) As Double
                For j = 0 To expval.GetUpperBound(0)
                    Dim v As Double = gee.Family.Variance(expval(j))
                    If v < 0.000000000001 Then v = 0.000000000001
                    sdev(j) = Math.Sqrt(v)
                Next


                resid = Matrix.MatrixArithmeticCore.Divide(Matrix.MatrixArithmeticCore.Subtract(endog, expval), sdev, tmpTrace)
                If tmpTrace <> String.Empty Then strTrace = strTrace & Environment.NewLine & tmpTrace
                Dim ssr As Double = SumSq(resid)
                Dim ii As Integer = 0
                For Each id1 In ix
                    Dim jj As Integer = 0
                    For Each id2 In ix
                        csum(dict.Item(id1), dict.Item(id2)) += 1
                        cov(dict.Item(id1), dict.Item(id2)) += (resid(ii) * resid(jj)) 'tmp(dict.Item(id1), dict.Item(id2))
                        jj += 1
                    Next id2
                    ii += 1
                Next id1
                wsum += ix.Length
                scaleEst += ssr
            Next


            If gee.UseP Then
                scaleEst = scaleEst / (wsum - gee.Nparams)
            Else
                'do not use the n-p correction for dispersion and correlation estimates, as in Liang and Zeger.
                'This can be useful when the number of observations is small, as subtracting p may yield correlations greater than 1.
                scaleEst /= wsum
            End If

            For i = 0 To csum.GetUpperBound(0)
                For j = 0 To csum.GetUpperBound(1)
                    If gee.UseP Then
                        If csum(i, j) >= gee.Nparams Then
                            csum(i, j) = (csum(i, j) - gee.Nparams) * scaleEst
                        Else 'this is based on SPSS alogorithms 22, pg 442/443 note
                            csum(i, j) *= scaleEst
                        End If
                    Else
                        csum(i, j) *= scaleEst 'do not use the n-p correction for dispersion and correlation estimates, as in Liang and Zeger. This can be useful when the number of observations is small, as subtracting p may yield correlations greater than 1.
                    End If
                Next
            Next
            cov = Matrix.MatrixArithmeticCore.Divide(cov, csum, tmpTrace)
            If tmpTrace <> String.Empty Then strTrace &= Environment.NewLine & tmpTrace

            For i = 0 To cov.GetUpperBound(0)
                For j = 0 To cov.GetUpperBound(1)
                    If i = j Then cov(i, j) = 1.0
                Next
            Next

            Me.pDepParams = cov
        End Sub

        ''' <summary>
        ''' Returns the appropriate submatrix of the full unstructured correlation
        ''' matrix for the current cluster, based on observed time points.
        ''' </summary>
        Public Overrides Function covarianceMatrix(endog_expval() As Double, gee As GEE, index As Integer) As Double(,)
            Dim out(,) As Double
            If pDepParams Is Nothing Then Me.pDepParams = Matrix.MatrixArithmeticCore.IdentityMatrix(gee.UniqueTimesDict.Count - 1)
            If gee.hasTime Then
                'TODO: need to test this on live data. I assume that the subset of the pDepParams matrix should be returned
                ' based on what times we have in the current cluster

                Dim time_li As List(Of Double()) = gee.TimeClustered
                Dim dict As Dictionary(Of Double, Integer) = gee.TimesDict 'dictionary - unique time values, counts
                ReDim out(time_li(index).GetUpperBound(0), time_li(index).GetUpperBound(0))

                Dim i As Integer = 0
                For Each idi In time_li(index)
                    Dim j As Integer = 0
                    For Each idj In time_li(index)
                        out(i, j) = pDepParams(dict.Item(idi), dict.Item(idj))
                        j += 1
                    Next idj
                    i += 1
                Next idi
            Else
                Dim endog_li As List(Of Double()) = gee.EndogClustered
                ReDim out(endog_li(index).GetUpperBound(0), endog_li(index).GetUpperBound(0))
                For i = 0 To endog_li(index).GetUpperBound(0)
                    For j = 0 To endog_li(index).GetUpperBound(0)
                        out(i, j) = pDepParams(i, j)
                    Next
                Next
            End If

            Return out
        End Function

    End Class
End Namespace
