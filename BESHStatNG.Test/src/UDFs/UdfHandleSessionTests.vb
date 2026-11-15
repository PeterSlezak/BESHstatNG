Option Explicit On
Option Strict On
Imports System
Imports System.Collections
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class UdfHandleSessionTests
    Private Shared Function Column(kind As String) As Object(,)
        Dim a(35, 0) As Object
        For i = 0 To 35
            Select Case kind
                Case "groups" : a(i, 0) = If(i < 18, "B", "A")
                Case "subject", "clusterId" : a(i, 0) = "S" & (i \ 4).ToString(CultureInfo.InvariantCulture)
                Case "visit" : a(i, 0) = CDbl(i Mod 4 + 1)
                Case "time" : a(i, 0) = CDbl(1 + i * 7 Mod 31)
                Case "status" : a(i, 0) = If(i Mod 4 = 0, 0.0, 1.0)
                Case "count" : a(i, 0) = CDbl(i * 5 Mod 9)
                Case "ordinal" : a(i, 0) = CDbl(i Mod 3 + 1)
                Case "treatment" : a(i, 0) = CDbl(i Mod 2)
                Case "existingScore" : a(i, 0) = 0.1 + i * 0.02
                Case "id" : a(i, 0) = "id" & i.ToString(CultureInfo.InvariantCulture)
                Case Else : a(i, 0) = 3.0 + i * 0.4 + Math.Sin((i + 1) * 0.7)
            End Select
        Next
        Return a
    End Function
    Private Shared Function Matrix(p As Integer, Optional categorical As Boolean = False) As Object(,)
        Dim a(35, p - 1) As Object
        For i = 0 To 35
            For j = 0 To p - 1
                a(i, j) = If(categorical, CType("C" & j.ToString() & ":" & (i Mod (j + 2)).ToString(), Object), CType(1.0 + 0.3 * i + Math.Sin((i + 1) * (j + 1) * 0.7) + ((i + 2 * j) Mod 5) * 0.11, Object))
            Next
        Next
        Return a
    End Function
    Private Shared Sub SetArg(m As MethodInfo, a() As Object, name As String, value As Object)
        Dim index = Array.FindIndex(m.GetParameters(), Function(p) p.Name = name)
        If index >= 0 Then a(index) = value
    End Sub
    Private Shared Function Arguments(m As MethodInfo, family As String) As Object()
        Dim a(m.GetParameters().Length - 1) As Object
        For i = 0 To a.Length - 1
            Dim p = m.GetParameters()(i)
            If p.IsOptional Then Continue For
            Select Case p.Name
                Case "x", "newX", "xCount", "covariates" : a(i) = Matrix(If(m.DeclaringType Is GetType(MultivariateAnalysisUDFs), 3, 2), family = "MCA")
                Case "y" : a(i) = Column(If(family = "ORDLOGIT" OrElse family = "MNLOGIT", "ordinal", If(family = "GLMNB" OrElse family = "ZIP", "count", "continuous")))
                Case "time", "status", "subject", "clusterId", "visit", "groups" : a(i) = Column(p.Name)
                Case "table" : a(i) = New Object(,) {{8.0, 5.0, 4.0}, {4.0, 9.0, 3.0}, {5.0, 2.0, 7.0}}
            End Select
        Next
        If family = "GLM" OrElse family = "GEE" Then SetArg(m, a, "family", "Gaussian") : SetArg(m, a, "link", "Identity")
        If family = "GEE" Then SetArg(m, a, "covariance", "Independence")
        SetArg(m, a, "randomSeed", 173)
        If family = "PCA" OrElse family = "FA" Then SetArg(m, a, "retentionMethod", "fixed") : SetArg(m, a, "retentionValue", 2)
        If family = "FA" Then SetArg(m, a, "extractionMethod", "principalcomponents") : SetArg(m, a, "rotationMethod", "none")
        If family = "MCA" Then SetArg(m, a, "hasHeader", False)
        If family = "KMEANS" Then SetArg(m, a, "numberOfClusters", 3)
        If family = "PSM" Then
            For Each name In {"id", "treatment", "outcome", "existingScore"} : SetArg(m, a, name, Column(name)) : Next
            SetArg(m, a, "covariates", Matrix(3)) : SetArg(m, a, "scoreMethod", "existing") : SetArg(m, a, "matchingOptions", "seed=173;caliperscale=none")
        End If
        If family = "LMM" OrElse family = "MMRM" Then
            Dim visits = If(family = "LMM", 5, 3), n = 18 * visits
            Dim y(n - 1, 0) As Object, x(n - 1, If(family = "LMM", 2, 4)) As Object, z(n - 1, 0) As Object, subject(n - 1, 0) As Object, visit(n - 1, 0) As Object
            Dim r = 0
            For sid = 1 To 18
                For v = 1 To visits
                    Dim trt = If(sid Mod 2 = 0, 1.0, 0.0), days = CDbl(v - 3), difficulty = -0.2 + 0.15 * (v - 1) + 0.04 * (sid Mod 4)
                    subject(r, 0) = "S" & sid.ToString("00", CultureInfo.InvariantCulture) : visit(r, 0) = CDbl(v)
                    If family = "LMM" Then
                        y(r, 0) = 12 + 0.9 * days + 1.1 * trt + 0.55 * difficulty + 0.28 * days * trt + 0.03 * (sid - 9) + 0.35 * (sid Mod 6 - 2) + 0.04 * (sid Mod 5 - 2) * days + 0.035 * ((sid + v - 1) Mod 5 - 2)
                        x(r, 0) = days : x(r, 1) = trt : x(r, 2) = difficulty : z(r, 0) = days
                    Else
                        Dim v2 = If(v = 2, 1.0, 0.0), v3 = If(v = 3, 1.0, 0.0)
                        y(r, 0) = 20 + 1.4 * trt + 0.8 * v + 0.35 * trt * v + 0.15 * (sid Mod 5 - 2) + 0.03 * ((sid + v) Mod 4 - 1)
                        x(r, 0) = trt : x(r, 1) = v2 : x(r, 2) = v3 : x(r, 3) = trt * v2 : x(r, 4) = trt * v3
                    End If
                    r += 1
                Next
            Next
            SetArg(m, a, "y", y) : SetArg(m, a, "x", x) : SetArg(m, a, "subject", subject) : SetArg(m, a, "visit", visit) : SetArg(m, a, "maxIter", 200)
            If family = "LMM" Then
                SetArg(m, a, "z", z) : SetArg(m, a, "xVarNames", "Days,Treatment,Difficulty") : SetArg(m, a, "zVarNames", "Days")
                SetArg(m, a, "residualCovariance", "ID") : SetArg(m, a, "randomCovariance", "RI+S") : SetArg(m, a, "inference", "ResidualDF")
            Else
                SetArg(m, a, "varNames", "treatment_active,visit2,visit3,treatment_active:visit2,treatment_active:visit3") : SetArg(m, a, "covariance", "CS") : SetArg(m, a, "inference", "KR")
            End If
        End If
        Return a
    End Function

    <DataTestMethod>
    <DataRow("PCA", "MultivariateAnalysisUDFs", "_pcaCache", "PCA_FIT", "PCA_SCORES", "PCA_DROP")>
    <DataRow("FA", "MultivariateAnalysisUDFs", "_faCache", "FA_FIT", "FA_SCORES", "FA_DROP")>
    <DataRow("KMEANS", "MultivariateAnalysisUDFs", "_kmeansCache", "KMEANS_FIT", "KMEANS_ASSIGNMENTS", "KMEANS_DROP")>
    <DataRow("HCLUST", "MultivariateAnalysisUDFs", "_hclustCache", "HCLUST_FIT", "HCLUST_MEMBERSHIP", "HCLUST_DROP")>
    <DataRow("CA", "MultivariateAnalysisUDFs", "_caCache", "CA_FIT", "CA_ROWS", "CA_DROP")>
    <DataRow("MCA", "MultivariateAnalysisUDFs", "_mcaCache", "MCA_FIT", "MCA_CATEGORIES", "MCA_DROP")>
    <DataRow("DA", "MultivariateAnalysisUDFs", "_daCache", "DA_FIT", "DA_CASEWISE", "DA_DROP")>
    <DataRow("PSM", "PropensityScoreUDFs", "_psmCache", "PS_FIT", "PS_SCORES", "PS_CLEANUP")>
    <DataRow("COX", "CoxUDFs", "_coxCache", "COX_FIT", "COX_TESTS", "COX_DROP")>
    <DataRow("GEE", "GEEUDFs", "_geeCache", "GEE_FIT", "GEE_VCOV", "GEE_DROP")>
    <DataRow("GLMNB", "GlmNbUDFs", "_glmNbCache", "GLMNB_FIT", "GLMNB_TESTS", "GLMNB_DROP")>
    <DataRow("GLM", "GlmUDFs", "_glmCache", "GLM_FIT", "GLM_TESTS", "GLM_DROP")>
    <DataRow("LM", "LinearModelUDFs", "_lmCache", "LM_FIT", "LM_TESTS", "LM_DROP")>
    <DataRow("LMM", "LmmUDFs", "_lmmCache", "LMM_FIT", "LMM_COEF", "LMM_DROP")>
    <DataRow("MMRM", "MixedModelUDFs", "_mmrmCache", "MMRM_FIT", "MMRM_COEF", "MMRM_DROP")>
    <DataRow("ORDLOGIT", "OrdinalLogitUDFs", "_ordCache", "ORDLOGIT_FIT", "ORDLOGIT_TESTS", "ORDLOGIT_DROP")>
    <DataRow("MNLOGIT", "MultinomialLogitUDFs", "_mnCache", "MNLOGIT_FIT", "MNLOGIT_TESTS", "MNLOGIT_DROP")>
    <DataRow("ZIP", "ZipUDFs", "_zipCache", "ZIP_FIT", "ZIP_TESTS", "ZIP_DROP")>
    Public Sub EveryFamilyRealFitRetainsObjectAssociationAndDropIsolation(family As String, moduleName As String, field As String, fitName As String, queryName As String, dropName As String)
        Dim old = CultureInfo.CurrentCulture
        Dim owner = GetType(MultivariateAnalysisUDFs).Assembly.GetType("BESHStatNG.WorksheetFunctions." & moduleName, True, True)
        Dim cache = DirectCast(owner.GetField(field, BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing), IDictionary)
        For Each culture In {"", "sk-SK"}
            Dim a As String = Nothing, b As String = Nothing
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim fit = owner.GetMethod(fitName), query = owner.GetMethod(queryName), drop = owner.GetMethod(dropName)
                a = TryCast(fit.Invoke(Nothing, Arguments(fit, family)), String) : b = TryCast(fit.Invoke(Nothing, Arguments(fit, family)), String)
                Assert.IsNotNull(a) : Assert.IsNotNull(b) : Assert.IsTrue(a.StartsWith(family & ":", StringComparison.Ordinal)) : Assert.AreNotEqual(a, b)
                Dim first = cache(a), second = cache(b)
                Assert.IsNotNull(first) : Assert.IsNotNull(second) : Assert.AreNotSame(first, second)
                Dim qa = Arguments(query, family) : SetArg(query, qa, "handle", " " & a.ToUpperInvariant() & " ")
                Dim output = TryCast(query.Invoke(Nothing, qa), Object(,)) : Assert.IsNotNull(output)
                output(0, 0) = "caller mutation"
                Assert.AreNotEqual("caller mutation", DirectCast(query.Invoke(Nothing, qa), Object(,))(0, 0))
                SetArg(query, qa, "handle", "OTHER:" & a.Split(":"c)(1))
                Assert.AreEqual(ExcelError.ExcelErrorNA, query.Invoke(Nothing, qa))
                Assert.AreEqual(True, drop.Invoke(Nothing, New Object() {" " & a.ToUpperInvariant() & " "}))
                SetArg(query, qa, "handle", a) : Assert.AreEqual(ExcelError.ExcelErrorNA, query.Invoke(Nothing, qa))
                Assert.AreEqual(False, drop.Invoke(Nothing, New Object() {a}))
                Assert.AreSame(second, cache(b))
                SetArg(query, qa, "handle", b) : Assert.IsInstanceOfType(query.Invoke(Nothing, qa), GetType(Object(,)))
            Finally
                If a IsNot Nothing Then cache.Remove(a)
                If b IsNot Nothing Then cache.Remove(b)
                CultureInfo.CurrentCulture = old
            End Try
        Next
    End Sub

    <TestMethod>
    Public Sub LegacyArbitraryStoredKeyIsNotRejectedByNewGrammarDescription()
        Dim owner = GetType(MultivariateAnalysisUDFs), fit = owner.GetMethod("PCA_FIT"), key As String = Nothing
        Dim cache = DirectCast(owner.GetField("_pcaCache", BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing), IDictionary)
        Const arbitrary = "controlled opaque key without generated syntax"
        Try
            key = CStr(fit.Invoke(Nothing, Arguments(fit, "PCA"))) : cache.Add(arbitrary, cache(key))
            Assert.IsInstanceOfType(MultivariateAnalysisUDFs.PCA_SCORES(arbitrary), GetType(Object(,)))
        Finally
            cache.Remove(arbitrary)
            If key IsNot Nothing Then cache.Remove(key)
        End Try
    End Sub

    <TestMethod>
    Public Sub ParallelLookupDropAllowsCapturedReferenceWithoutResurrection()
        Dim owner = GetType(MultivariateAnalysisUDFs), fit = owner.GetMethod("PCA_FIT"), key As String = Nothing
        Dim cache = DirectCast(owner.GetField("_pcaCache", BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing), IDictionary)
        Try
            key = CStr(fit.Invoke(Nothing, Arguments(fit, "PCA")))
            Dim expected = cache(key), read As Object = Nothing
            Dim helper = owner.GetMethod("TryGetPcaHandle", BindingFlags.NonPublic Or BindingFlags.Static)
            Using gate As New Barrier(2)
                Dim tasks() As Task = {Task.Run(Sub()
                                                   Assert.IsTrue(gate.SignalAndWait(5000))
                                                   Dim args() As Object = {key, Nothing}
                                                   If CBool(helper.Invoke(Nothing, args)) Then read = args(1)
                                               End Sub), Task.Run(Sub()
                                                                      Assert.IsTrue(gate.SignalAndWait(5000))
                                                                      MultivariateAnalysisUDFs.PCA_DROP(key)
                                                                  End Sub)}
                Assert.IsTrue(Task.WaitAll(tasks, 10000))
            End Using
            Assert.IsTrue(read Is Nothing OrElse Object.ReferenceEquals(expected, read))
            Assert.IsFalse(cache.Contains(key))
        Finally
            If key IsNot Nothing Then cache.Remove(key)
        End Try
    End Sub
End Class
