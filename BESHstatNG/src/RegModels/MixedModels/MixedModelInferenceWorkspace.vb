Option Explicit On
Option Strict On

Namespace regression

    ''' <summary>
    ''' Fixed-effect inference method requested for mixed-model linear estimates.
    ''' </summary>
    Public Enum MixedModelInferenceKind
        WaldNormal = 0
        ResidualDF = 1
        BetweenWithin = 2
        Satterthwaite = 3

        ''' <summary>
        ''' Reserved for a full Kenward-Roger implementation.
        ''' Do not report KR unless the adjusted coefficient covariance matrix and
        ''' KR denominator degrees of freedom were both successfully computed.
        ''' </summary>
        KenwardRoger = 4
    End Enum


    ''' <summary>
    ''' Represents one linear estimate or contrast L * beta.
    ''' </summary>
    Public Class MixedModelLinearHypothesis

        ''' <summary>
        ''' Contrast/design row L, aligned with the fixed-effect coefficient vector.
        ''' </summary>
        Public Property L As Double()

        ''' <summary>
        ''' Optional display label for the row.
        ''' </summary>
        Public Property Label As String = String.Empty

        Public Sub New()
        End Sub

        Public Sub New(label As String, lRow() As Double)
            Me.Label = If(label, String.Empty)
            Me.L = lRow
        End Sub

        Public Function Validate(expectedLength As Integer) As Boolean
            Return L IsNot Nothing AndAlso L.Length = expectedLength
        End Function

    End Class


    ''' <summary>
    ''' Result of one fixed-effect linear estimate.
    ''' </summary>
    Public Class MixedModelLinearInferenceResult

        Public Property Label As String = String.Empty
        Public Property Estimate As Double = Double.NaN
        Public Property StdError As Double = Double.NaN
        Public Property DF As Double = Double.NaN
        Public Property Statistic As Double = Double.NaN
        Public Property PValue As Double = Double.NaN
        Public Property LowerCI As Double = Double.NaN
        Public Property UpperCI As Double = Double.NaN
        Public Property StatisticLabel As String = "z"
        Public Property PValueLabel As String = "Pr(>|z|)"
        Public Property InferenceKind As MixedModelInferenceKind = MixedModelInferenceKind.WaldNormal
        Public Property DiagnosticMessage As String = String.Empty

    End Class


    ''' <summary>
    ''' Universal derivative workspace for mixed-model fixed-effect inference.
    ''' This class is deliberately independent of whether the model is MMRM or LMM.
    ''' </summary>
    Public Class MixedModelInferenceWorkspace

        ''' <summary>
        ''' Number of fixed effects.
        ''' </summary>
        Public Property P As Integer

        ''' <summary>
        ''' Number of covariance parameters.
        ''' </summary>
        Public Property K As Integer

        ''' <summary>
        ''' Fixed-effect covariance matrix Phi = Var(beta).
        ''' For KR this can be the unadjusted covariance, while adjusted covariance
        ''' is stored in AdjustedVarBeta.
        ''' </summary>
        Public Property VarBeta As Double(,)

        ''' <summary>
        ''' Optional Kenward-Roger adjusted coefficient covariance matrix.
        ''' </summary>
        Public Property AdjustedVarBeta As Double(,)

        ''' <summary>
        ''' Approximate covariance of covariance parameters theta.
        ''' </summary>
        Public Property ThetaCovariance As Double(,)

        ''' <summary>
        ''' First derivative of Var(beta) with respect to theta.
        ''' Dimensions: theta index, beta row, beta column.
        ''' This is enough for Satterthwaite row-specific DF.
        ''' </summary>
        Public Property VarBetaGradient As Double(,,)

        ''' <summary>
        ''' First derivative P_h matrices needed by KR.
        ''' Dimensions: theta index, beta row, beta column.
        ''' </summary>
        Public Property KR_P As Double(,,)

        ''' <summary>
        ''' Second-order Q_hj matrices needed by KR.
        ''' Dimensions: theta h, theta j, beta row, beta column.
        ''' </summary>
        Public Property KR_Q As Double(,,,)

        ''' <summary>
        ''' Second-derivative R_hj matrices needed by second-order KR.
        ''' Dimensions: theta h, theta j, beta row, beta column.
        ''' </summary>
        Public Property KR_R As Double(,,,)

        ''' <summary>
        ''' True if KR_P and KR_Q are available.
        ''' </summary>
        Public Function HasLinearKRIngredients() As Boolean
            Return KR_P IsNot Nothing AndAlso KR_Q IsNot Nothing AndAlso
                   ThetaCovariance IsNot Nothing AndAlso VarBeta IsNot Nothing
        End Function

        ''' <summary>
        ''' True if KR_P, KR_Q and KR_R are available.
        ''' </summary>
        Public Function HasSecondOrderKRIngredients() As Boolean
            Return HasLinearKRIngredients() AndAlso KR_R IsNot Nothing
        End Function

        Public Function ValidateBasic() As Boolean
            If P <= 0 Then Return False
            If VarBeta Is Nothing Then Return False
            If VarBeta.GetLength(0) <> P OrElse VarBeta.GetLength(1) <> P Then Return False
            Return True
        End Function

    End Class


    ''' <summary>
    ''' Universal math helpers for mixed-model fixed-effect inference.
    ''' These helpers do not know whether the model is MMRM or LMM.
    ''' </summary>
    Public Module MixedModelInferenceMath

        Public Function LinearEstimate(l() As Double, beta() As Double) As Double
            If l Is Nothing OrElse beta Is Nothing OrElse l.Length <> beta.Length Then Return Double.NaN

            Dim s As Double = 0.0
            For i As Integer = 0 To l.Length - 1
                s += l(i) * beta(i)
            Next

            Return s
        End Function


        Public Function LinearVariance(l() As Double, varBeta(,) As Double) As Double
            If l Is Nothing OrElse varBeta Is Nothing Then Return Double.NaN
            If varBeta.GetLength(0) <> l.Length OrElse varBeta.GetLength(1) <> l.Length Then Return Double.NaN

            Dim s As Double = 0.0
            For r As Integer = 0 To l.Length - 1
                For c As Integer = 0 To l.Length - 1
                    s += l(r) * varBeta(r, c) * l(c)
                Next
            Next

            Return s
        End Function


        Public Function QuadraticForm(v() As Double, a(,) As Double) As Double
            If v Is Nothing OrElse a Is Nothing Then Return Double.NaN
            If a.GetLength(0) <> v.Length OrElse a.GetLength(1) <> v.Length Then Return Double.NaN

            Dim s As Double = 0.0
            For r As Integer = 0 To v.Length - 1
                For c As Integer = 0 To v.Length - 1
                    s += v(r) * a(r, c) * v(c)
                Next
            Next

            Return s
        End Function


        ''' <summary>
        ''' Computes row-specific Satterthwaite DF for L beta using a universal workspace.
        ''' </summary>
        Public Function TrySatterthwaiteDF(l() As Double,
                                           workspace As MixedModelInferenceWorkspace,
                                           ByRef df As Double) As Boolean
            df = Double.NaN

            If l Is Nothing OrElse workspace Is Nothing Then Return False
            If Not workspace.ValidateBasic() Then Return False
            If workspace.ThetaCovariance Is Nothing OrElse workspace.VarBetaGradient Is Nothing Then Return False
            If l.Length <> workspace.P Then Return False

            Dim v As Double = LinearVariance(l, workspace.VarBeta)
            If Not AppInfrastructure.IsFinite(v) OrElse v <= 0.0 Then Return False

            Dim k As Integer = workspace.ThetaCovariance.GetLength(0)
            If workspace.ThetaCovariance.GetLength(1) <> k Then Return False
            If workspace.VarBetaGradient.GetLength(0) <> k Then Return False
            If workspace.VarBetaGradient.GetLength(1) <> workspace.P Then Return False
            If workspace.VarBetaGradient.GetLength(2) <> workspace.P Then Return False

            Dim g(k - 1) As Double

            For h As Integer = 0 To k - 1
                Dim s As Double = 0.0

                For r As Integer = 0 To workspace.P - 1
                    For c As Integer = 0 To workspace.P - 1
                        s += l(r) * workspace.VarBetaGradient(h, r, c) * l(c)
                    Next
                Next

                If Not AppInfrastructure.IsFinite(s) Then s = 0.0
                g(h) = s
            Next

            Dim varV As Double = QuadraticForm(g, workspace.ThetaCovariance)
            If Not AppInfrastructure.IsFinite(varV) OrElse varV <= 0.0 Then Return False

            df = 2.0 * v * v / varV
            If Not AppInfrastructure.IsFinite(df) OrElse df <= 0.0 Then Return False

            df = Math.Max(1.0, Math.Min(1000000.0, df))
            Return True
        End Function


        ''' <summary>
        ''' Computes a linear Kenward-Roger adjusted covariance matrix if KR ingredients are available.
        ''' This intentionally does not pretend to do full second-order KR when R_hj is missing.
        ''' </summary>
        Public Function TryLinearKenwardRogerAdjustedVarBeta(workspace As MixedModelInferenceWorkspace,
                                                             ByRef adjustedVarBeta(,) As Double,
                                                             Optional ByRef diagnostic As String = Nothing) As Boolean
            adjustedVarBeta = Nothing
            diagnostic = String.Empty

            If workspace Is Nothing OrElse Not workspace.ValidateBasic() Then
                diagnostic = "Invalid inference workspace."
                Return False
            End If

            If Not workspace.HasLinearKRIngredients() Then
                diagnostic = "KR ingredients are incomplete. Need VarBeta, ThetaCovariance, P_h and Q_hj matrices."
                Return False
            End If

            Dim p As Integer = workspace.P
            Dim k As Integer = workspace.K
            Dim phi(,) As Double = workspace.VarBeta
            Dim w(,) As Double = workspace.ThetaCovariance

            Dim middle(p - 1, p - 1) As Double

            For h As Integer = 0 To k - 1
                For j As Integer = 0 To k - 1
                    Dim whj As Double = w(h, j)
                    If whj = 0.0 Then Continue For

                    Dim qhj(,) As Double = Slice4D(workspace.KR_Q, h, j, p)
                    Dim ph(,) As Double = Slice3D(workspace.KR_P, h, p)
                    Dim pj(,) As Double = Slice3D(workspace.KR_P, j, p)

                    Dim phPhiPj(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(ph, phi), pj)

                    For r As Integer = 0 To p - 1
                        For c As Integer = 0 To p - 1
                            ' Linear KR: ignore R_hj second-derivative term.
                            middle(r, c) += whj * (qhj(r, c) - phPhiPj(r, c))
                        Next
                    Next
                Next
            Next

            Dim add(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(phi, middle), phi)

            ReDim adjustedVarBeta(p - 1, p - 1)
            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    adjustedVarBeta(r, c) = phi(r, c) + 2.0 * add(r, c)
                Next
            Next

            MixedModelEngine.SymmetrizeInPlace(adjustedVarBeta)
            Return True
        End Function


        Private Function Slice3D(a(,,) As Double, h As Integer, p As Integer) As Double(,)
            Dim out(p - 1, p - 1) As Double
            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    out(r, c) = a(h, r, c)
                Next
            Next
            Return out
        End Function


        Private Function Slice4D(a(,,,) As Double, h As Integer, j As Integer, p As Integer) As Double(,)
            Dim out(p - 1, p - 1) As Double
            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    out(r, c) = a(h, j, r, c)
                Next
            Next
            Return out
        End Function

    End Module

End Namespace
