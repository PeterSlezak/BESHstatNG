Option Explicit On
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions


    ''' <summary>
    ''' Worksheet functions that expose selected distribution utilities from the BESHStatNG library.
    ''' </summary>
    Public Module DistributionUDFs

        ' -------------------------------------------------------------------------------------------------------------
        ' Studentized range distribution
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Studentized range distribution CDF: returns <c>P(0 ≤ Q ≤ q)</c>.
        ''' </summary>
        ''' <param name="q">
        ''' Studentized range value (must be &gt; 0).
        ''' </param>
        ''' <param name="v">
        ''' Degrees of freedom (must be ≥ 1). Non-integer values are supported.
        ''' </param>
        ''' <param name="r">
        ''' Number of groups/samples (must be ≥ 2).
        ''' </param>
        ''' <returns>
        ''' The probability <c>P(0 ≤ Q ≤ q)</c>. If inputs are invalid or the internal routine reports failure,
        ''' returns <c>#NUM!</c>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This is a worksheet-friendly wrapper around the internal implementation
        ''' <c>distributions.Distributions.PRTRNG(q, v, r, iFault)</c> (Algorithm AS 190).
        ''' </para>
        ''' <para>
        ''' The Studentized range distribution is commonly used for Tukey-style multiple comparison
        ''' procedures (e.g., Tukey HSD) after ANOVA.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.DIST.PRTRNG(3.5, 20, 5)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.DIST.PRTRNG",
            Category:="BESHStatNG - Distributions",
            Description:="Studentized range CDF: returns P(0 ≤ Q ≤ q) for df=v and r groups (AS190).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/distributions/",
            IsThreadSafe:=True)>
        Public Function PRTRNG(
            <ExcelArgument(Name:="q", Description:="Studentized range value (q ≥ 0).")>
            ByVal q As Double,
            <ExcelArgument(Name:="v", Description:="Degrees of freedom (v > 0).")>
            ByVal v As Double,
            <ExcelArgument(Name:="r", Description:="Number of samples / groups (r ≥ 2).")>
            ByVal r As Double
        ) As Object

            ' Basic argument validation (avoid hard crashes / misleading output)
            If Double.IsNaN(q) OrElse Double.IsNaN(v) OrElse Double.IsNaN(r) Then Return ExcelError.ExcelErrorNum
            If q < 0 OrElse v <= 0 OrElse r < 2 Then Return ExcelError.ExcelErrorNum

            Dim iFault As Integer = 0
            Dim p As Double = distributions.Distributions.PRTRNG(q, v, r, iFault)

            If iFault <> 0 OrElse Double.IsNaN(p) OrElse Double.IsInfinity(p) Then
                Return ExcelError.ExcelErrorNum
            End If

            Return p
        End Function

        ''' <summary>
        ''' Studentized range distribution upper tail: returns <c>P(Q &gt; q)</c>.
        ''' </summary>
        ''' <param name="q">
        ''' Studentized range value (must be &gt; 0).
        ''' </param>
        ''' <param name="v">
        ''' Degrees of freedom (must be ≥ 1).
        ''' </param>
        ''' <param name="r">
        ''' Number of groups/samples (must be ≥ 2).
        ''' </param>
        ''' <returns>
        ''' The probability <c>P(Q &gt; q)</c>. If inputs are invalid, returns <c>#NUM!</c>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is computed as <c>1 - BESH.DIST.PRTRNG(q, v, r)</c> with safeguards for
        ''' floating-point rounding.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.DIST.PRTRNG.TAIL(3.5, 20, 5)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.DIST.PRTRNG.TAIL",
            Category:="BESHStatNG - Distributions",
            Description:="Studentized range upper-tail: returns P(Q > q) = 1 - BESH.DIST.PRTRNG(q,v,r).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/distributions/",
            IsThreadSafe:=True)>
        Public Function PRTRNG_TAIL(
            <ExcelArgument(Name:="q", Description:="Studentized range value (q ≥ 0).")>
            ByVal q As Double,
            <ExcelArgument(Name:="v", Description:="Degrees of freedom (v > 0).")>
            ByVal v As Double,
            <ExcelArgument(Name:="r", Description:="Number of samples / groups (r ≥ 2).")>
            ByVal r As Double
        ) As Object

            Dim cdfObj As Object = PRTRNG(q, v, r)
            If TypeOf cdfObj Is ExcelError Then Return cdfObj

            Dim cdf As Double = CDbl(cdfObj)
            Dim tail As Double = 1.0 - cdf

            ' Clamp to [0,1] to avoid negative tails due to numerical rounding
            If tail < 0 Then tail = 0.0
            If tail > 1 Then tail = 1.0

            Return tail
        End Function


        ''' <summary>
        ''' Computes the probability density function (PDF) of the F distribution.
        ''' </summary>
        ''' <param name="x">
        ''' Point at which to evaluate the density (must be ≥ 0).
        ''' </param>
        ''' <param name="df1">
        ''' Numerator degrees of freedom (must be &gt; 0). Non-integer values are supported.
        ''' </param>
        ''' <param name="df2">
        ''' Denominator degrees of freedom (must be &gt; 0). Non-integer values are supported.
        ''' </param>
        ''' <returns>
        ''' The value of the F distribution PDF at <paramref name="x"/>.
        ''' If inputs are invalid or the density is not finite, returns <c>#NUM!</c>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This is the worksheet-friendly wrapper around
        ''' <c>distributions.Distributions.F_PDF(x, df1, df2)</c>.
        ''' </para>
        ''' <para>
        ''' <b>Relation to Excel</b>:<br/>
        ''' <c>F.DIST(x, df1, df2, FALSE)</c> ↔ <c>BESH.DIST.F_PDF(x, df1, df2)</c>
        ''' </para>
        ''' <para>
        ''' <b>Relation to R</b>:<br/>
        ''' <c>df(x, df1, df2)</c> ↔ <c>BESH.DIST.F_PDF(x, df1, df2)</c>
        ''' </para>
        ''' <para>
        ''' <b>Behavior at x = 0</b>: the F density behaves like <c>x^(df1/2 - 1)</c> as <c>x → 0</c>.
        ''' Therefore:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description>If <c>df1 &gt; 2</c>, then <c>f(0) = 0</c>.</description></item>
        ''' <item><description>If <c>df1 = 2</c>, then <c>f(0) = 1</c>.</description></item>
        ''' <item><description>If <c>df1 &lt; 2</c>, the density diverges (infinite) and this UDF returns <c>#NUM!</c>.</description></item>
        ''' </list>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.DIST.F_PDF(1.25, 5, 10)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.DIST.F_PDF",
            Category:="BESHStatNG - Distributions",
            Description:="F distribution PDF (equivalent to F.DIST(x, df1, df2, FALSE)).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/distributions/")>
        Public Function F_PDF(
            <ExcelArgument(Name:="x", Description:="Point at which to evaluate the density (x ≥ 0).")> x As Double,
            <ExcelArgument(Name:="df1", Description:="Numerator degrees of freedom (df1 > 0).")> df1 As Double,
            <ExcelArgument(Name:="df2", Description:="Denominator degrees of freedom (df2 > 0).")> df2 As Double
        ) As Object

            If Double.IsNaN(x) OrElse Double.IsNaN(df1) OrElse Double.IsNaN(df2) Then
                Return ExcelError.ExcelErrorNum
            End If
            If x < 0 OrElse df1 <= 0 OrElse df2 <= 0 Then
                Return ExcelError.ExcelErrorNum
            End If

            ' Handle x = 0 explicitly (see remarks).
            If x = 0 Then
                If df1 > 2 Then Return 0.0
                If df1 = 2 Then Return 1.0
                Return ExcelError.ExcelErrorNum
            End If

            Dim p As Double = distributions.Distributions.F_PDF(x, df1, df2)

            If Double.IsNaN(p) OrElse Double.IsInfinity(p) Then
                Return ExcelError.ExcelErrorNum
            End If

            Return p
        End Function
    End Module

End Namespace