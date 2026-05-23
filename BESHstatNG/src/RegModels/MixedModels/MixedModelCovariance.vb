Option Explicit On
Option Strict On

Imports System
Imports System.Text
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Shared covariance-matrix routines for the Gaussian mixed-model engine.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module is the numerical bridge between the model specification layer
    ''' (<see cref="MixedModelFitRequest"/>, <see cref="MixedModelGStruct"/>,
    ''' <see cref="MixedModelRStruct"/>) and the eventual likelihood evaluator
    ''' (<c>MixedModelEngine</c>).  It deliberately contains only matrix/block
    ''' operations and does not know anything about Excel ranges, UI dialogs, or
    ''' formula parsing.
    ''' </para>
    ''' <para>
    ''' For a subject/cluster block <c>i</c>, the Gaussian mixed-model covariance is
    ''' </para>
    ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
    ''' <para>
    ''' where <c>Z_i</c> is the random-effects design for the subject, <c>G</c> is the
    ''' random-effects covariance matrix, and <c>R_i</c> is the residual/within-subject
    ''' covariance matrix.  Ordinary LMM uses both parts; MMRM uses the same engine
    ''' with no random-effects contribution, so that <c>V_i = R_i</c>.
    ''' </para>
    ''' <para>
    ''' Implementation decisions:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>All covariance work is subject-block based; this matches the likelihood needed for LMM and MMRM.</description></item>
    ''' <item><description>Positive-definite solves are delegated to the existing project Cholesky routines in <c>Matrix.vb</c>.</description></item>
    ''' <item><description>No Moore-Penrose fallback is used here.  During likelihood optimization, non-SPD covariance proposals should be rejected or penalized, not silently pseudo-inverted.</description></item>
    ''' <item><description>Optional diagonal jitter is available only through explicit helper methods, mainly for diagnostics and controlled optimizer retry logic.</description></item>
    ''' <item><description>Each public operation accepts an optional in-memory trace buffer while also writing through <see cref="CoreServices.logger"/>.  This mirrors the GLM/GEE logging style and allows the future engine/UI to expose detailed diagnostics to the user.</description></item>
    ''' </list>
    ''' </remarks>
    Public Module MixedModelCovariance

        Private Const DefaultJitterStart As Double = 0.0000000001
        Private Const DefaultJitterMultiplier As Double = 10.0
        Private Const MaxSymmetryWarningAbsDiff As Double = 0.00000001

        ''' <summary>
        ''' Builds the marginal covariance matrix <c>V_i</c> for one subject block.
        ''' </summary>
        ''' <param name="block">Subject block containing <c>y_i</c>, <c>X_i</c>, optional <c>Z_i</c>, and visit metadata.</param>
        ''' <param name="data">Global blocked dataset metadata used by visit-based R-side structures.</param>
        ''' <param name="gStruct">Random-effects covariance structure.  Use <see cref="NoRandomEffects"/> for MMRM.</param>
        ''' <param name="rStruct">Residual/within-subject covariance structure.</param>
        ''' <param name="thetaG">Internal-scale G-side parameter vector.</param>
        ''' <param name="thetaR">Internal-scale R-side parameter vector.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns>A symmetric square matrix with dimension <c>block.Nobs × block.Nobs</c>.</returns>
        ''' <remarks>
        ''' <para>
        ''' The assembled matrix is
        ''' </para>
        ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
        ''' <para>
        ''' If <paramref name="gStruct"/> is <c>Nothing</c> or is a degenerate
        ''' no-random-effects structure, the G-side contribution is skipped.  This is
        ''' the intended path for MMRM.
        ''' </para>
        ''' </remarks>
        Public Function BuildVi(block As MixedModelSubjectBlock,
                                data As MixedModelBlockData,
                                gStruct As MixedModelGStruct,
                                rStruct As MixedModelRStruct,
                                thetaG() As Double,
                                thetaR() As Double,
                                Optional ByRef strTrace As String = Nothing) As Double(,)

            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If rStruct Is Nothing Then Throw New ArgumentNullException(NameOf(rStruct))

            LogTrace($"MixedModelCovariance.BuildVi start subject='{block.SubjectKey}', n={block.Nobs}, q={block.Q}, R='{rStruct.ToString()}', G='{If(gStruct Is Nothing, "Nothing", gStruct.ToString())}'", strTrace)

            Dim ri(,) As Double = rStruct.BuildRi(thetaR, block, data, strTrace)
            ValidateSquareMatrix(ri, block.Nobs, "R_i")

            Dim vi(,) As Double = Matrix.CloneMatrix(ri)

            Dim addGSide As Boolean = False
            If gStruct IsNot Nothing AndAlso Not gStruct.IsDegenerateZeroG() Then
                addGSide = True
            End If

            If addGSide Then
                If Not block.HasRandomEffectsDesign() Then
                    Dim msg As String = $"MixedModelCovariance.BuildVi cannot add G-side contribution for subject '{block.SubjectKey}' because Z_i is missing."
                    LogWarn(msg, strTrace)
                    Throw New ApplicationException(msg)
                End If

                Dim gMat(,) As Double = gStruct.BuildG(thetaG, block.Q, strTrace)
                If gMat Is Nothing Then
                    LogWarn($"MixedModelCovariance.BuildVi G-structure '{gStruct.ToString()}' returned Nothing; only R_i will be used for subject '{block.SubjectKey}'.", strTrace)
                Else
                    ValidateSquareMatrix(gMat, block.Q, "G")
                    Dim zigzit(,) As Double = BuildZiGZiT(block, gMat, strTrace)
                    vi = Matrix.M_ADD(vi, zigzit)
                    LogTrace($"MixedModelCovariance.BuildVi added Z_i G Z_i' contribution for subject='{block.SubjectKey}'.", strTrace)
                End If
            Else
                LogTrace($"MixedModelCovariance.BuildVi no G-side contribution for subject='{block.SubjectKey}' (MMRM/no random effects path).", strTrace)
            End If

            WarnIfNotSymmetric(vi, "V_i", strTrace)
            regression.MixedModelEngine.SymmetrizeInPlace(vi)

            LogTrace($"MixedModelCovariance.BuildVi completed subject='{block.SubjectKey}', dim={vi.GetLength(0)}.", strTrace)
            Return vi
        End Function

        ''' <summary>
        ''' Computes the G-side marginal covariance contribution <c>Z_i G Z_i'</c>.
        ''' </summary>
        ''' <param name="block">Subject block containing the random-effects design <c>Z_i</c>.</param>
        ''' <param name="gMat">Random-effects covariance matrix <c>G</c>.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns>A square matrix with dimension <c>block.Nobs × block.Nobs</c>.</returns>
        Public Function BuildZiGZiT(block As MixedModelSubjectBlock,
                                    gMat(,) As Double,
                                    Optional ByRef strTrace As String = Nothing) As Double(,)
            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If gMat Is Nothing Then Throw New ArgumentNullException(NameOf(gMat))
            If Not block.HasRandomEffectsDesign() Then Throw New ApplicationException("Cannot compute Z_i G Z_i' because the subject block has no Z matrix.")

            Dim z(,) As Double = block.Z
            Dim n As Integer = block.Nobs
            Dim q As Integer = block.Q
            ValidateSquareMatrix(gMat, q, "G")

            Dim zg(,) As Double = Matrix.MatrixMult(z, gMat)
            Dim zt(,) As Double = Matrix.trans(z)
            Dim out(,) As Double = Matrix.MatrixMult(zg, zt)

            ValidateSquareMatrix(out, n, "Z_i G Z_i'")
            WarnIfNotSymmetric(out, "Z_i G Z_i'", strTrace)
            MixedModelEngine.SymmetrizeInPlace(out)
            LogTrace($"MixedModelCovariance.BuildZiGZiT subject='{block.SubjectKey}', n={n}, q={q}.", strTrace)
            Return out
        End Function

        ''' <summary>
        ''' Attempts a strict Cholesky decomposition of a symmetric positive-definite covariance matrix.
        ''' </summary>
        ''' <param name="vMat">Input covariance matrix.</param>
        ''' <param name="chol">Output lower-triangular Cholesky factor if the decomposition succeeds.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns><c>True</c> if the matrix is positive definite; otherwise <c>False</c>.</returns>
        ''' <remarks>
        ''' <para>
        ''' This helper intentionally calls the project Cholesky routine with <c>bErrorRaise := False</c>
        ''' and does not fall back to a pseudo-inverse.  Invalid covariance proposals are common during
        ''' numerical optimization; the likelihood evaluator should catch a <c>False</c> return and assign
        ''' a large objective value.
        ''' </para>
        ''' </remarks>
        Public Function TryCholesky(vMat(,) As Double,
                                    ByRef chol(,) As Double,
                                    Optional ByRef strTrace As String = Nothing) As Boolean
            ValidateSquareMatrix(vMat, -1, "V")
            Dim iErr As Integer = 0
            chol = Global.BESHStatNG.Matrix.Matrix.Cholesky(Matrix.CloneMatrix(vMat), iErr, False)
            If iErr <> 0 Then
                LogTrace($"MixedModelCovariance.TryCholesky failed with iErr={iErr}; dim={vMat.GetLength(0)}.", strTrace)
                Return False
            End If
            LogTrace($"MixedModelCovariance.TryCholesky succeeded; dim={vMat.GetLength(0)}.", strTrace)
            Return True
        End Function

        ''' <summary>
        ''' Computes a Cholesky factor and throws a logged exception if the matrix is not positive definite.
        ''' </summary>
        Public Function CholeskyStrict(vMat(,) As Double,
                                       Optional matrixLabel As String = "V",
                                       Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim chol(,) As Double = Nothing
            If Not TryCholesky(vMat, chol, strTrace) Then
                Dim msg As String = $"{matrixLabel} is not symmetric positive definite in MixedModelCovariance.CholeskyStrict."
                LogWarn(msg, strTrace)
                Throw New ApplicationException(msg)
            End If
            Return chol
        End Function

        ''' <summary>
        ''' Attempts Cholesky factorization after adding a small diagonal jitter, increasing the jitter if needed.
        ''' </summary>
        ''' <param name="vMat">Input covariance matrix.</param>
        ''' <param name="chol">Output Cholesky factor.</param>
        ''' <param name="usedJitter">Actual jitter added to the diagonal when the factorization succeeded.</param>
        ''' <param name="maxAttempts">Maximum number of jitter attempts.</param>
        ''' <param name="jitterStart">Initial diagonal jitter.</param>
        ''' <param name="jitterMultiplier">Multiplier applied to jitter after each failure.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns><c>True</c> if Cholesky succeeds, otherwise <c>False</c>.</returns>
        ''' <remarks>
        ''' This method is intended for controlled diagnostics or last-resort optimizer retry logic.  It should
        ''' not be used silently in the main likelihood path unless the result trace clearly reports that jitter
        ''' was required.
        ''' </remarks>
        Public Function TryCholeskyWithJitter(vMat(,) As Double,
                                              ByRef chol(,) As Double,
                                              ByRef usedJitter As Double,
                                              Optional maxAttempts As Integer = 6,
                                              Optional jitterStart As Double = DefaultJitterStart,
                                              Optional jitterMultiplier As Double = DefaultJitterMultiplier,
                                              Optional ByRef strTrace As String = Nothing) As Boolean
            If TryCholesky(vMat, chol, strTrace) Then
                usedJitter = 0.0
                Return True
            End If

            Dim jitter As Double = Math.Max(jitterStart, 0.0)
            For attempt As Integer = 1 To Math.Max(maxAttempts, 0)
                Dim tmp(,) As Double = AddDiagonalJitter(vMat, jitter)
                If TryCholesky(tmp, chol, strTrace) Then
                    usedJitter = jitter
                    LogWarn($"MixedModelCovariance.TryCholeskyWithJitter succeeded after adding diagonal jitter={jitter}; attempt={attempt}.", strTrace)
                    Return True
                End If
                jitter *= jitterMultiplier
            Next

            usedJitter = Double.NaN
            LogWarn("MixedModelCovariance.TryCholeskyWithJitter failed after all attempts.", strTrace)
            Return False
        End Function

        ''' <summary>
        ''' Adds a constant to the diagonal of a square matrix and returns a new matrix.
        ''' </summary>
        Public Function AddDiagonalJitter(mat(,) As Double, jitter As Double) As Double(,)
            ValidateSquareMatrix(mat, -1, "matrix")
            Dim out(,) As Double = Matrix.CloneMatrix(mat)
            For i As Integer = 0 To out.GetUpperBound(0)
                out(i, i) += jitter
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes <c>log |A|</c> from a lower-triangular Cholesky factor <c>L</c>, where <c>A = L L'</c>.
        ''' </summary>
        ''' <param name="chol">Lower-triangular Cholesky factor.</param>
        ''' <returns>The log determinant, <c>2 * Sum(log(diag(L)))</c>.</returns>
        Public Function LogDetFromCholesky(chol(,) As Double) As Double
            ValidateSquareMatrix(chol, -1, "Cholesky factor")
            Dim out As Double = 0.0
            For i As Integer = 0 To chol.GetUpperBound(0)
                If chol(i, i) <= 0.0 OrElse Double.IsNaN(chol(i, i)) OrElse Double.IsInfinity(chol(i, i)) Then
                    Throw New ApplicationException($"Invalid Cholesky diagonal at index {i}: {chol(i, i)}")
                End If
                out += 2.0 * Math.Log(chol(i, i))
            Next
            Return out
        End Function

        ''' <summary>
        ''' Solves <c>A X = B</c> for a symmetric positive-definite matrix <c>A</c> using Cholesky decomposition.
        ''' </summary>
        ''' <param name="vMat">Symmetric positive-definite left-hand side matrix <c>A</c>.</param>
        ''' <param name="rhs">Right-hand-side matrix <c>B</c>.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns>Solution matrix <c>X</c>.</returns>
        Public Function SolveSPD(vMat(,) As Double,
                                 rhs(,) As Double,
                                 Optional ByRef strTrace As String = Nothing) As Double(,)
            If rhs Is Nothing Then Throw New ArgumentNullException(NameOf(rhs))
            ValidateSquareMatrix(vMat, rhs.GetLength(0), "V")
            Dim chol(,) As Double = CholeskyStrict(vMat, "V", strTrace)
            Dim out(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(chol, rhs)
            LogTrace($"MixedModelCovariance.SolveSPD solved dim={vMat.GetLength(0)} with rhsCols={rhs.GetLength(1)}.", strTrace)
            Return out
        End Function

        ''' <summary>
        ''' Solves <c>A x = b</c> for a symmetric positive-definite matrix <c>A</c> using Cholesky decomposition.
        ''' </summary>
        Public Function SolveSPDVector(vMat(,) As Double,
                                       rhs() As Double,
                                       Optional ByRef strTrace As String = Nothing) As Double()
            If rhs Is Nothing Then Throw New ArgumentNullException(NameOf(rhs))
            ValidateSquareMatrix(vMat, rhs.Length, "V")
            Dim chol(,) As Double = CholeskyStrict(vMat, "V", strTrace)
            Dim out() As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(chol, rhs)
            LogTrace($"MixedModelCovariance.SolveSPDVector solved dim={vMat.GetLength(0)}.", strTrace)
            Return out
        End Function

        ''' <summary>
        ''' Computes the inverse of an SPD matrix from a strict Cholesky factorization.
        ''' </summary>
        ''' <remarks>
        ''' This helper is useful for diagnostics, BLUP calculations, covariance-parameter checks, and
        ''' small Excel-scale subject blocks.  The main likelihood evaluator should prefer solving linear
        ''' systems instead of explicitly forming inverses whenever possible.
        ''' </remarks>
        Public Function InverseSPD(vMat(,) As Double,
                                   Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim chol(,) As Double = CholeskyStrict(vMat, "V", strTrace)
            Dim out(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholInv(chol)
            LogTrace($"MixedModelCovariance.InverseSPD inverted dim={vMat.GetLength(0)}.", strTrace)
            Return out
        End Function

        ''' <summary>
        ''' Computes <c>r' A^-1 r</c> using a supplied Cholesky factor of <c>A</c>.
        ''' </summary>
        Public Function QuadraticFormFromCholesky(chol(,) As Double,
                                                  residual() As Double,
                                                  Optional ByRef strTrace As String = Nothing) As Double
            If residual Is Nothing Then Throw New ArgumentNullException(NameOf(residual))
            ValidateSquareMatrix(chol, residual.Length, "Cholesky factor")
            Dim solved() As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(chol, residual)
            Dim out As Double = Matrix.DotProduct(residual, solved)
            LogTrace($"MixedModelCovariance.QuadraticFormFromCholesky q={out}.", strTrace)
            Return out
        End Function

        ''' <summary>
        ''' Computes <c>r' A^-1 r</c> by first factoring <paramref name="vMat"/>.
        ''' </summary>
        Public Function QuadraticForm(vMat(,) As Double,
                                      residual() As Double,
                                      Optional ByRef strTrace As String = Nothing) As Double
            Dim chol(,) As Double = CholeskyStrict(vMat, "V", strTrace)
            Return QuadraticFormFromCholesky(chol, residual, strTrace)
        End Function

        ''' <summary>
        ''' Computes the marginal fitted mean vector <c>X_i beta</c> for one subject block.
        ''' </summary>
        Public Function ComputeMarginalMean(block As MixedModelSubjectBlock,
                                            betaHat() As Double) As Double()
            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If betaHat Is Nothing Then Throw New ArgumentNullException(NameOf(betaHat))
            If betaHat.Length <> block.P Then Throw New ApplicationException($"betaHat length ({betaHat.Length}) must equal block.P ({block.P}).")
            Return Matrix.MatrixVectorMultiply(block.X, betaHat)
        End Function

        ''' <summary>
        ''' Computes raw residuals <c>y_i - X_i beta</c> for one subject block.
        ''' </summary>
        Public Function ComputeMarginalResidual(block As MixedModelSubjectBlock,
                                                betaHat() As Double) As Double()
            Dim mean() As Double = ComputeMarginalMean(block, betaHat)
            Dim y() As Double = block.Y
            Dim out(y.Length - 1) As Double
            For i As Integer = 0 To y.Length - 1
                out(i) = y(i) - mean(i)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes the empirical Bayes/BLUP estimate of a subject random-effects vector.
        ''' </summary>
        ''' <param name="block">Subject block.</param>
        ''' <param name="betaHat">Estimated fixed-effects coefficient vector.</param>
        ''' <param name="gMat">Random-effects covariance matrix <c>G</c>.</param>
        ''' <param name="vInv">Inverse of the marginal block covariance <c>V_i^-1</c>.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns>
        ''' <c>b_hat_i = G Z_i' V_i^-1 (y_i - X_i beta_hat)</c>.  Returns <c>Nothing</c> if there is no random-effects design or no G matrix.
        ''' </returns>
        Public Function ComputeBLUP(block As MixedModelSubjectBlock,
                                    betaHat() As Double,
                                    gMat(,) As Double,
                                    vInv(,) As Double,
                                    Optional ByRef strTrace As String = Nothing) As Double()
            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If betaHat Is Nothing Then Throw New ArgumentNullException(NameOf(betaHat))
            If gMat Is Nothing OrElse Not block.HasRandomEffectsDesign() Then
                LogTrace($"MixedModelCovariance.ComputeBLUP subject='{block.SubjectKey}' skipped because there is no G/Z contribution.", strTrace)
                Return Nothing
            End If

            ValidateSquareMatrix(gMat, block.Q, "G")
            ValidateSquareMatrix(vInv, block.Nobs, "V inverse")

            Dim resid() As Double = ComputeMarginalResidual(block, betaHat)
            Dim z(,) As Double = block.Z
            Dim zt(,) As Double = Matrix.trans(z)
            Dim ztVinv(,) As Double = Matrix.MatrixMult(zt, vInv)
            Dim ztVinvResid() As Double = Matrix.MatrixVectorMultiply(ztVinv, resid)
            Dim out() As Double = Matrix.MatrixVectorMultiply(gMat, ztVinvResid)

            LogTrace($"MixedModelCovariance.ComputeBLUP subject='{block.SubjectKey}', q={out.Length}.", strTrace)
            Return out
        End Function

        ''' <summary>
        ''' Accumulates the block contributions needed for profiled fixed-effects estimation.
        ''' </summary>
        ''' <param name="block">Subject block.</param>
        ''' <param name="cholV">Lower-triangular Cholesky factor of <c>V_i</c>.</param>
        ''' <param name="xtVinvX">Accumulated <c>X' V^-1 X</c>; initialized by caller.</param>
        ''' <param name="xtVinvY">Accumulated <c>X' V^-1 y</c>; initialized by caller.</param>
        ''' <param name="yVinvY">Accumulated scalar <c>y' V^-1 y</c>.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <remarks>
        ''' This routine is designed for the future <c>MixedModelEngine</c>.  For each subject it solves
        ''' <c>V_i^-1 X_i</c> and <c>V_i^-1 y_i</c>, then accumulates the sufficient cross-products used to
        ''' profile out <c>beta</c> in the ML/REML objective.
        ''' </remarks>
        Public Sub AccumulateProfileCrossProducts(block As MixedModelSubjectBlock,
                                                  cholV(,) As Double,
                                                  ByRef xtVinvX(,) As Double,
                                                  ByRef xtVinvY() As Double,
                                                  ByRef yVinvY As Double,
                                                  Optional ByRef strTrace As String = Nothing)
            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If cholV Is Nothing Then Throw New ArgumentNullException(NameOf(cholV))
            ValidateSquareMatrix(cholV, block.Nobs, "Cholesky factor")

            If xtVinvX Is Nothing Then
                ReDim xtVinvX(block.P - 1, block.P - 1)
            End If
            If xtVinvY Is Nothing Then
                ReDim xtVinvY(block.P - 1)
            End If
            If xtVinvX.GetLength(0) <> block.P OrElse xtVinvX.GetLength(1) <> block.P Then Throw New ApplicationException("xtVinvX has incompatible dimensions.")
            If xtVinvY.Length <> block.P Then Throw New ApplicationException("xtVinvY has incompatible length.")

            Dim x(,) As Double = block.X
            Dim y() As Double = block.Y
            Dim vinvX(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(cholV, x)
            Dim vinvY() As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(cholV, y)

            Dim xt(,) As Double = Matrix.trans(x)
            Dim blockXtVinvX(,) As Double = Matrix.MatrixMult(xt, vinvX)
            Dim blockXtVinvY() As Double = Matrix.MatrixVectorMultiply(xt, vinvY)

            For r As Integer = 0 To block.P - 1
                xtVinvY(r) += blockXtVinvY(r)
                For c As Integer = 0 To block.P - 1
                    xtVinvX(r, c) += blockXtVinvX(r, c)
                Next
            Next

            yVinvY += Matrix.DotProduct(y, vinvY)
            LogTrace($"MixedModelCovariance.AccumulateProfileCrossProducts subject='{block.SubjectKey}', p={block.P}, n={block.Nobs}.", strTrace)
        End Sub

        ''' <summary>
        ''' Returns the maximum absolute asymmetry <c>max |A_ij - A_ji|</c> for a square matrix.
        ''' </summary>
        Public Function MaxAbsAsymmetry(mat(,) As Double) As Double
            ValidateSquareMatrix(mat, -1, "matrix")
            Dim maxDiff As Double = 0.0
            For i As Integer = 0 To mat.GetUpperBound(0)
                For j As Integer = i + 1 To mat.GetUpperBound(1)
                    Dim d As Double = Math.Abs(mat(i, j) - mat(j, i))
                    If d > maxDiff Then maxDiff = d
                Next
            Next
            Return maxDiff
        End Function

        ''' <summary>
        ''' Returns a symmetrized copy <c>(A + A') / 2</c>.
        ''' </summary>
        Public Function SymmetrizedCopy(mat(,) As Double) As Double(,)
            Dim out(,) As Double = Matrix.CloneMatrix(mat)
            MixedModelEngine.SymmetrizeInPlace(out)
            Return out
        End Function

        Private Sub ValidateSquareMatrix(mat(,) As Double, expectedDim As Integer, label As String)
            If mat Is Nothing Then Throw New ArgumentNullException(label)
            If mat.GetLength(0) <> mat.GetLength(1) Then Throw New ApplicationException(label & " must be square.")
            If expectedDim >= 0 AndAlso mat.GetLength(0) <> expectedDim Then
                Throw New ApplicationException($"{label} dimension mismatch. Expected {expectedDim}, found {mat.GetLength(0)}.")
            End If
        End Sub

        Private Sub WarnIfNotSymmetric(mat(,) As Double, matrixLabel As String, Optional ByRef strTrace As String = Nothing)
            Dim d As Double = MaxAbsAsymmetry(mat)
            If d > MaxSymmetryWarningAbsDiff Then
                LogWarn($"{matrixLabel} was not exactly symmetric before symmetrization; maxAbsDiff={d}.", strTrace)
            End If
        End Sub

        Private Sub AppendTraceLine(message As String, Optional ByRef strTrace As String = Nothing)
            If message Is Nothing Then Exit Sub
            If strTrace Is Nothing OrElse strTrace = String.Empty Then
                strTrace = message
            Else
                strTrace &= vbNewLine & message
            End If
        End Sub

        Private Sub LogTrace(message As String, Optional ByRef strTrace As String = Nothing)
            ' Important: Nothing means "do not collect low-level trace for this call".
            ' This is used during optimizer objective evaluations to avoid huge logs.
            If strTrace Is Nothing Then Exit Sub
            AppendTraceLine(message, strTrace)
            CoreServices.Logger.Trace(message)
        End Sub

        Private Sub LogDebug(message As String, Optional ByRef strTrace As String = Nothing)
            AppendTraceLine(message, strTrace)
            CoreServices.Logger.Debug(message)
        End Sub

        Private Sub LogWarn(message As String, Optional ByRef strTrace As String = Nothing)
            AppendTraceLine(message, strTrace)
            CoreServices.Logger.Warn(message)
        End Sub

    End Module

End Namespace
