Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression

' -----------------------------------------------------------------------------
' Consolidated mixed-model test module.
' This file groups previously separate test classes so the MixedModels test
' folder stays below ten compile modules while preserving the existing tests.
' -----------------------------------------------------------------------------

' ===== BEGIN migrated from MMRMMulticovariateMissingReferenceTests.vb =====



' MMRM reference and missing-data tests using the augmented longitudinal Orthodont data set.
' The CSV is real-data-anchored: the response/age/sex/visit structure follows the
' Potthoff-Roy Orthodont data, with deterministic additional factors/covariates and
' missingness patterns added for mixed-model unit testing.
'
' Reference constants in this file were generated from an independent REML MMRM/GLS
' reference calculation using the same fixed-effects design and residual covariance
' structures. They are hard-coded so that the tests do not depend on R or any external
' script at run time.
<TestClass>
Public Class MMRMMulticovariateMissingReferenceTests

    Private Const DATA_FILE As String = "mixedmodel_longitudinal_multicovariate_missing.csv"

    Private Const TOL_BETA As Double = 0.0001
    Private Const TOL_SE As Double = 0.0001
    Private Const TOL_LOGLIK As Double = 0.001
    Private Const TOL_LSMEAN As Double = 0.0001
    Private Const TOL_CONTRAST As Double = 0.0001

    <TestMethod>
    Public Sub MulticovariateMMRM_RStructures_MatchHardCodedReferences()
        Dim dat As ModelData = LoadModelData(responseObservedOnly:=True)

        For Each ref As RStructureReference In ReferenceCases()
            Dim result As MixedModelResult = FitMMRM(dat, ref)

            AssertBasicResult(result,
                              expectedP:=7,
                              expectedN:=95,
                              expectedSubjects:=27,
                              label:=ref.Name)

            For j As Integer = 0 To ref.Beta.Length - 1
                AssertAlmostEqual(ref.Beta(j),
                                  result.Beta(j),
                                  TOL_BETA,
                                  ref.Name & " beta[" & j.ToString(CultureInfo.InvariantCulture) & "]")

                AssertAlmostEqual(ref.BetaSE(j),
                                  result.BetaSE(j),
                                  TOL_SE,
                                  ref.Name & " SE[" & j.ToString(CultureInfo.InvariantCulture) & "]")
            Next

            AssertAlmostEqual(ref.LogLik,
                              result.LogLik,
                              TOL_LOGLIK,
                              ref.Name & " REML log-likelihood")
        Next
    End Sub


    <TestMethod>
    Public Sub MulticovariateMMRM_IncompleteVisitPatterns_ArePresentAfterResponseFiltering()
        Dim allRows As List(Of Dictionary(Of String, String)) = LoadRows()
        Dim dat As ModelData = LoadModelData(responseObservedOnly:=True)

        Assert.AreEqual(99, allRows.Count, "Total rows in source CSV.")
        Assert.AreEqual(4, CountRowsWhere(allRows, Function(r) IsMissing(r("distance_mm"))), "Rows with missing response.")
        Assert.AreEqual(95, dat.Y.Length, "Rows used after excluding missing response.")
        Assert.AreEqual(27, CountDistinctSubjects(dat.SubjectId), "Number of subjects after response filtering.")

        Dim clusterSizes As Dictionary(Of Integer, Integer) = ClusterSizeCounts(dat.SubjectId)
        Assert.AreEqual(16, GetCount(clusterSizes, 4), "Subjects with four observed response visits.")
        Assert.AreEqual(9, GetCount(clusterSizes, 3), "Subjects with three observed response visits.")
        Assert.AreEqual(2, GetCount(clusterSizes, 2), "Subjects with two observed response visits.")

        Dim patternCounts As Dictionary(Of String, Integer) = ObservedVisitPatternCounts(dat.SubjectId, dat.Visit)
        Assert.AreEqual(16, GetCount(patternCounts, "V1V2V3V4"), "Complete observed-response pattern count.")
        Assert.AreEqual(3, GetCount(patternCounts, "V1V2V3"), "V1V2V3 observed-response pattern count.")
        Assert.AreEqual(2, GetCount(patternCounts, "V1V2V4"), "V1V2V4 observed-response pattern count.")
        Assert.AreEqual(2, GetCount(patternCounts, "V1V3V4"), "V1V3V4 observed-response pattern count.")
        Assert.AreEqual(2, GetCount(patternCounts, "V2V3V4"), "V2V3V4 observed-response pattern count.")
        Assert.AreEqual(1, GetCount(patternCounts, "V1V2"), "V1V2 observed-response pattern count.")
        Assert.AreEqual(1, GetCount(patternCounts, "V1V3"), "V1V3 observed-response pattern count.")
    End Sub


    <TestMethod>
    Public Sub MulticovariateMMRM_MissingCovariatePatterns_AreDetected()
        Dim rows As List(Of Dictionary(Of String, String)) = LoadRows()

        Assert.AreEqual(24, CountRowsWhere(rows, Function(r) r("any_covariate_missing") = "1"), "Rows with any covariate missing flag.")
        Assert.AreEqual(71, CountRowsWhere(rows, Function(r) r("analysis_include_complete_covariates") = "1"), "Rows complete for response and selected covariates.")
        Assert.AreEqual(71, CountRowsWhere(rows, Function(r) Not IsMissing(r("distance_mm")) AndAlso r("analysis_include_complete_covariates") = "1"), "Observed-response rows complete for covariate analysis.")

        Assert.AreEqual(8, CountRowsWhere(rows, Function(r) IsMissing(r("parent_height_cm"))), "Missing parent_height_cm count.")
        Assert.AreEqual(8, CountRowsWhere(rows, Function(r) IsMissing(r("baseline_bmi"))), "Missing baseline_bmi count.")
        Assert.AreEqual(4, CountRowsWhere(rows, Function(r) IsMissing(r("weight_kg"))), "Missing weight_kg count.")
        Assert.AreEqual(4, CountRowsWhere(rows, Function(r) IsMissing(r("adherence_pct"))), "Missing adherence_pct count.")
        Assert.AreEqual(2, CountRowsWhere(rows, Function(r) IsMissing(r("puberty_stage"))), "Missing puberty_stage count.")

        Dim completeRows As List(Of Dictionary(Of String, String)) =
            rows.FindAll(Function(r) Not IsMissing(r("distance_mm")) AndAlso r("analysis_include_complete_covariates") = "1")

        Assert.AreEqual(71, completeRows.Count, "Complete-case row count.")
        Assert.AreEqual(23, CountDistinctSubjects(completeRows), "Complete-case subject count.")
    End Sub


    <TestMethod>
    Public Sub MulticovariateMMRM_UN_PostEstimation_LSMeansAndContrastsMatchReferences()
        Dim dat As ModelData = LoadModelData(responseObservedOnly:=True)
        Dim ref As RStructureReference = ReferenceCases().Find(Function(x) x.Name = "Unstructured")
        Dim result As MixedModelResult = FitMMRM(dat, ref)

        Dim lsRefs As List(Of LinearEstimateReference) = UNLsMeanReferences()
        For Each one As LinearEstimateReference In lsRefs
            Dim l() As Double =
                MixedModelPostEstimation.AverageDesignRowForProfile(dat.X,
                                                                    dat.Visit,
                                                                    dat.TreatmentActive,
                                                                    one.Visit,
                                                                    one.Group,
                                                                    Nothing)

            Assert.IsNotNull(l, "Expected estimable LS-mean row for " & one.Label)

            Dim estimate As Double = MixedModelPostEstimation.LinearEstimate(l, result.Beta)
            Dim variance As Double = MixedModelPostEstimation.LinearCombinationVariance(l, result.VarBeta)
            Dim se As Double = Math.Sqrt(Math.Max(0.0, variance))

            AssertAlmostEqual(one.Estimate, estimate, TOL_LSMEAN, one.Label & " estimate")
            AssertAlmostEqual(one.StdError, se, TOL_LSMEAN, one.Label & " SE")
        Next

        Dim contrastRefs As List(Of LinearEstimateReference) = UNActiveMinusControlReferences()
        For Each one As LinearEstimateReference In contrastRefs
            Dim lControl() As Double =
                MixedModelPostEstimation.AverageDesignRowForProfile(dat.X,
                                                                    dat.Visit,
                                                                    dat.TreatmentActive,
                                                                    one.Visit,
                                                                    0.0,
                                                                    Nothing)

            Dim lActive() As Double =
                MixedModelPostEstimation.AverageDesignRowForProfile(dat.X,
                                                                    dat.Visit,
                                                                    dat.TreatmentActive,
                                                                    one.Visit,
                                                                    1.0,
                                                                    Nothing)

            Assert.IsNotNull(lControl, "Expected control LS-mean row for " & one.Label)
            Assert.IsNotNull(lActive, "Expected active LS-mean row for " & one.Label)

            Dim lDiff() As Double = Matrix.M_SUB(lActive, lControl)
            Dim estimate As Double = MixedModelPostEstimation.LinearEstimate(lDiff, result.Beta)
            Dim variance As Double = MixedModelPostEstimation.LinearCombinationVariance(lDiff, result.VarBeta)
            Dim se As Double = Math.Sqrt(Math.Max(0.0, variance))

            AssertAlmostEqual(one.Estimate, estimate, TOL_CONTRAST, one.Label & " estimate")
            AssertAlmostEqual(one.StdError, se, TOL_CONTRAST, one.Label & " SE")
        Next
    End Sub


    Private Shared Function FitMMRM(dat As ModelData,
                                    ref As RStructureReference) As MixedModelResult
        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=dat.Y,
                                           x:=dat.X,
                                           subjectId:=dat.SubjectId,
                                           z:=Nothing,
                                           visit:=dat.Visit,
                                           sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateMMRM(blockData,
                                            CreateRStruct(ref.Name),
                                            MixedModelFitMethod.REML)

        req.RequestLabel = "Multicovariate missing-data MMRM " & ref.Name
        req.ResponseVarName = "distance_mm"
        req.SubjectVarName = "subject_id"
        req.VisitVarName = "visit"
        req.FixedEffectNames = FixedEffectNames()
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
        req.Control = ReferenceControl()
        req.StartThetaR = CType(ref.StartTheta.Clone(), Double())

        Dim fit As New MMRM(req)
        Return fit.Fit()
    End Function


    Private Shared Function CreateRStruct(name As String) As MixedModelRStruct
        Select Case name
            Case "Identity"
                Return New IdentityR()
            Case "Diagonal Heterogeneous"
                Return New DiagonalHeterogeneousR()
            Case "Compound Symmetry"
                Return New CompoundSymmetryR()
            Case "AR(1)"
                Return New AR1R()
            Case "Unstructured"
                Return New UnstructuredR()
            Case Else
                Throw New ArgumentException("Unsupported test R structure: " & name)
        End Select
    End Function


    Private Shared Function ReferenceControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 300
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function ReferenceCases() As List(Of RStructureReference)
        Return New List(Of RStructureReference) From {
            New RStructureReference(
                name:="Identity",
                startTheta:=New Double() {1.5142690069388516},
                beta:=New Double() {22.1980921082, -2.0777163358, -0.4949719123, 1.5695414612, 1.1761742362, 0.6222021557, 0.1119491254},
                betaSE:=New Double() {0.6135025803, 0.4510218180, 0.7251478563, 0.5397744996, 0.5411225348, 0.1417068537, 0.1980454884},
                logLik:=-205.2652871791),
            New RStructureReference(
                name:="Diagonal Heterogeneous",
                startTheta:=New Double() {1.7042262218450015, 1.4285767851216464, 1.4354365230160766, 1.4497621896523536},
                beta:=New Double() {22.1396235546, -2.1049153548, -0.4396030136, 1.5729923829, 1.1951126205, 0.6349093733, 0.1005363228},
                betaSE:=New Double() {0.6358720639, 0.4483513790, 0.7632976531, 0.5364491547, 0.5367501375, 0.1451329161, 0.2026401365},
                logLik:=-204.9417234003),
            New RStructureReference(
                name:="Compound Symmetry",
                startTheta:=New Double() {1.6119723529784433, 0.6291627433102471},
                beta:=New Double() {22.1782536338, -2.0862966722, -0.7793301433, 1.7477785426, 1.3243007174, 0.5758940624, 0.2233415333},
                betaSE:=New Double() {0.8111918489, 0.7330857221, 0.8256272725, 0.8796279574, 0.8833450432, 0.1008705523, 0.1406419363},
                logLik:=-192.0944536497),
            New RStructureReference(
                name:="AR(1)",
                startTheta:=New Double() {1.5897216989092446, 0.5953963678753839},
                beta:=New Double() {22.3755238874, -2.1691809270, -0.7293192787, 1.5850679169, 1.1564322162, 0.6141400869, 0.1504710492},
                betaSE:=New Double() {0.7792211739, 0.6505460056, 0.8538041506, 0.7779739418, 0.7865231975, 0.1411578863, 0.1965268926},
                logLik:=-196.2398737568),
            New RStructureReference(
                name:="Unstructured",
                startTheta:=New Double() {0.8791697156684223, 1.2312552673676571, 0.4975075045219937, 1.3817814980687670, 0.2357143321994451, 0.5524592870139459, 1.0820320747663819, 1.1684924986178790, 0.8855391449998156, 0.3761405971440460},
                beta:=New Double() {21.8233980303, -1.8605849380, -0.6326622729, 1.7236210053, 1.4752119937, 0.5937075634, 0.2249988230},
                betaSE:=New Double() {0.8171610943, 0.7284883988, 0.8284791263, 0.8778882336, 0.8751760731, 0.1117356257, 0.1539057948},
                logLik:=-188.3710023182)
        }
    End Function


    Private Shared Function UNLsMeanReferences() As List(Of LinearEstimateReference)
        Return New List(Of LinearEstimateReference) From {
            New LinearEstimateReference("Visit 1 Control", 1.0, 0.0, 22.0920447462, 0.5945661722),
            New LinearEstimateReference("Visit 1 Active", 1.0, 1.0, 21.4703560385, 0.5753317161),
            New LinearEstimateReference("Visit 2 Control", 2.0, 0.0, 23.3407559971, 0.5225777286),
            New LinearEstimateReference("Visit 2 Active", 2.0, 1.0, 23.0671511735, 0.4980510361),
            New LinearEstimateReference("Visit 3 Control", 3.0, 0.0, 24.5507537614, 0.5396818129),
            New LinearEstimateReference("Visit 3 Active", 3.0, 1.0, 24.8476858648, 0.5085629249),
            New LinearEstimateReference("Visit 4 Control", 4.0, 0.0, 25.7734214360, 0.6412076000),
            New LinearEstimateReference("Visit 4 Active", 4.0, 1.0, 26.5490567632, 0.6019868805)
        }
    End Function


    Private Shared Function UNActiveMinusControlReferences() As List(Of LinearEstimateReference)
        Return New List(Of LinearEstimateReference) From {
            New LinearEstimateReference("Visit 1 Active - Control", 1.0, Double.NaN, -0.6216887076, 0.8270041652),
            New LinearEstimateReference("Visit 2 Active - Control", 2.0, Double.NaN, -0.2736048235, 0.7210441652),
            New LinearEstimateReference("Visit 3 Active - Control", 3.0, Double.NaN, 0.2969321034, 0.7424173017),
            New LinearEstimateReference("Visit 4 Active - Control", 4.0, Double.NaN, 0.7756353272, 0.8820516702)
        }
    End Function


    Private Shared Function FixedEffectNames() As String()
        Return New String() {"Intercept",
                             "sex_code",
                             "treatment_active",
                             "site_central",
                             "site_south",
                             "age_centered_8",
                             "treatment_active:age_centered_8"}
    End Function


    Private Shared Function LoadModelData(responseObservedOnly As Boolean) As ModelData
        Dim rows As List(Of Dictionary(Of String, String)) = LoadRows()
        If responseObservedOnly Then
            rows = rows.FindAll(Function(r) Not IsMissing(r("distance_mm")))
        End If

        Dim n As Integer = rows.Count
        Dim y(n - 1) As Double
        Dim subject(n - 1) As Object
        Dim visit(n - 1) As Double
        Dim treatment(n - 1) As Double
        Dim x(n - 1, 6) As Double

        For i As Integer = 0 To n - 1
            Dim r As Dictionary(Of String, String) = rows(i)

            Dim sexCode As Double = GetNumericOrRecode(r,
                                                       New String() {"sex_code"},
                                                       Function(row) If(String.Equals(row("sex"), "Female", StringComparison.OrdinalIgnoreCase), 1.0, 0.0))

            Dim active As Double = GetNumericOrRecode(r,
                                                      New String() {"treatment_active_code", "active_code", "treatment_arm_code"},
                                                      Function(row) If(String.Equals(row("treatment_arm"), "Active", StringComparison.OrdinalIgnoreCase), 1.0, 0.0))

            Dim siteCentral As Double = GetNumericOrRecode(r,
                                                           New String() {"clinic_central_code", "site_central_code", "central_code"},
                                                           Function(row) If(String.Equals(row("clinic_site"), "Central", StringComparison.OrdinalIgnoreCase), 1.0, 0.0))

            Dim siteSouth As Double = GetNumericOrRecode(r,
                                                         New String() {"clinic_south_code", "site_south_code", "south_code"},
                                                         Function(row) If(String.Equals(row("clinic_site"), "South", StringComparison.OrdinalIgnoreCase), 1.0, 0.0))

            Dim ageC As Double = ParseD(r("age_centered_8"))

            y(i) = ParseD(r("distance_mm"))
            subject(i) = r("subject_id")
            visit(i) = ParseD(r("visit"))
            treatment(i) = active

            x(i, 0) = 1.0
            x(i, 1) = sexCode
            x(i, 2) = active
            x(i, 3) = siteCentral
            x(i, 4) = siteSouth
            x(i, 5) = ageC
            x(i, 6) = active * ageC
        Next

        Return New ModelData With {
            .Y = y,
            .X = x,
            .SubjectId = subject,
            .Visit = visit,
            .TreatmentActive = treatment
        }
    End Function


    Private Shared Function LoadRows() As List(Of Dictionary(Of String, String))
        Dim path As String = GetTestDataPath(DATA_FILE)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException(DATA_FILE & " must contain a header and at least one data row.")
        End If

        Dim header() As String = SplitCsvSimple(lines(0))
        Dim rows As New List(Of Dictionary(Of String, String))()

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For

            Dim parts() As String = SplitCsvSimple(lines(i))
            Dim row As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            For j As Integer = 0 To header.Length - 1
                Dim value As String = String.Empty
                If j < parts.Length Then value = parts(j)
                row(header(j)) = value
            Next

            rows.Add(row)
        Next

        Return rows
    End Function


    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory

        Dim candidates As String() = {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "TestData", fileName),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\..\TestData", fileName))
        }

        For Each candidate As String In candidates
            If File.Exists(candidate) Then Return candidate
        Next

        Throw New FileNotFoundException("Could not locate test data file.", fileName)
    End Function


    Private Shared Function SplitCsvSimple(line As String) As String()
        Dim raw() As String = line.Split(","c)
        Dim out(raw.Length - 1) As String

        For i As Integer = 0 To raw.Length - 1
            out(i) = CleanCsvToken(raw(i))
        Next

        Return out
    End Function


    Private Shared Function CleanCsvToken(value As String) As String
        If value Is Nothing Then Return String.Empty

        Dim s As String = value.Trim()
        If s.Length >= 2 AndAlso s.StartsWith("""", StringComparison.Ordinal) AndAlso s.EndsWith("""", StringComparison.Ordinal) Then
            s = s.Substring(1, s.Length - 2).Replace("""""", """")
        End If

        Return s.Trim()
    End Function


    Private Shared Function ParseD(text As String) As Double
        Return Double.Parse(CleanCsvToken(text), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function IsMissing(value As String) As Boolean
        Return String.IsNullOrWhiteSpace(value)
    End Function


    Private Shared Function GetNumericOrRecode(row As Dictionary(Of String, String),
                                               candidateColumns() As String,
                                               fallback As Func(Of Dictionary(Of String, String), Double)) As Double
        For Each col As String In candidateColumns
            If row.ContainsKey(col) AndAlso Not IsMissing(row(col)) Then
                Return ParseD(row(col))
            End If
        Next

        Return fallback(row)
    End Function


    Private Shared Function CountRowsWhere(rows As List(Of Dictionary(Of String, String)),
                                           predicate As Func(Of Dictionary(Of String, String), Boolean)) As Integer
        Dim count As Integer = 0
        For Each row As Dictionary(Of String, String) In rows
            If predicate(row) Then count += 1
        Next
        Return count
    End Function


    Private Shared Function CountDistinctSubjects(subject() As Object) As Integer
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each s As Object In subject
            seen.Add(CStr(s))
        Next

        Return seen.Count
    End Function


    Private Shared Function CountDistinctSubjects(rows As List(Of Dictionary(Of String, String))) As Integer
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each r As Dictionary(Of String, String) In rows
            seen.Add(r("subject_id"))
        Next

        Return seen.Count
    End Function


    Private Shared Function ClusterSizeCounts(subject() As Object) As Dictionary(Of Integer, Integer)
        Dim bySubject As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For Each s As Object In subject
            Dim key As String = CStr(s)
            If Not bySubject.ContainsKey(key) Then bySubject(key) = 0
            bySubject(key) += 1
        Next

        Dim out As New Dictionary(Of Integer, Integer)()
        For Each kv As KeyValuePair(Of String, Integer) In bySubject
            If Not out.ContainsKey(kv.Value) Then out(kv.Value) = 0
            out(kv.Value) += 1
        Next

        Return out
    End Function


    Private Shared Function ObservedVisitPatternCounts(subject() As Object,
                                                       visit() As Double) As Dictionary(Of String, Integer)
        Dim bySubject As New Dictionary(Of String, List(Of Integer))(StringComparer.OrdinalIgnoreCase)

        For i As Integer = 0 To subject.Length - 1
            Dim key As String = CStr(subject(i))
            If Not bySubject.ContainsKey(key) Then bySubject(key) = New List(Of Integer)()
            bySubject(key).Add(CInt(visit(i)))
        Next

        Dim out As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For Each kv As KeyValuePair(Of String, List(Of Integer)) In bySubject
            kv.Value.Sort()
            Dim pattern As String = String.Empty
            For Each v As Integer In kv.Value
                pattern &= "V" & v.ToString(CultureInfo.InvariantCulture)
            Next

            If Not out.ContainsKey(pattern) Then out(pattern) = 0
            out(pattern) += 1
        Next

        Return out
    End Function


    Private Shared Function GetCount(Of T)(dict As Dictionary(Of T, Integer), key As T) As Integer
        If dict.ContainsKey(key) Then Return dict(key)
        Return 0
    End Function


    Private Shared Sub AssertBasicResult(result As MixedModelResult,
                                         expectedP As Integer,
                                         expectedN As Integer,
                                         expectedSubjects As Integer,
                                         label As String)
        Assert.IsNotNull(result, label & " result should not be Nothing.")
        Assert.AreEqual(expectedP, result.P, label & " fixed-effect dimension.")
        Assert.AreEqual(expectedN, result.Nobs, label & " observation count.")
        Assert.AreEqual(expectedSubjects, result.NoSubjects, label & " subject count.")
        Assert.IsNotNull(result.Beta, label & " beta vector.")
        Assert.IsNotNull(result.BetaSE, label & " beta SE vector.")
        Assert.AreEqual(expectedP, result.Beta.Length, label & " beta length.")
        Assert.AreEqual(expectedP, result.BetaSE.Length, label & " beta SE length.")
    End Sub


    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         tolerance As Double,
                                         label As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail(label & ": actual value is not finite. Expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If

        Dim diff As Double = Math.Abs(expected - actual)

        If diff > tolerance Then
            Assert.Fail(label & ": expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " &
                        actual.ToString("G17", CultureInfo.InvariantCulture) &
                        ", abs diff " &
                        diff.ToString("G17", CultureInfo.InvariantCulture) &
                        " > tolerance " &
                        tolerance.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If
    End Sub


    Private Class ModelData
        Public Y() As Double
        Public X(,) As Double
        Public SubjectId() As Object
        Public Visit() As Double
        Public TreatmentActive() As Double
    End Class


    Private Class RStructureReference
        Public ReadOnly Name As String
        Public ReadOnly StartTheta() As Double
        Public ReadOnly Beta() As Double
        Public ReadOnly BetaSE() As Double
        Public ReadOnly LogLik As Double

        Public Sub New(name As String,
                       startTheta() As Double,
                       beta() As Double,
                       betaSE() As Double,
                       logLik As Double)
            Me.Name = name
            Me.StartTheta = startTheta
            Me.Beta = beta
            Me.BetaSE = betaSE
            Me.LogLik = logLik
        End Sub
    End Class


    Private Class LinearEstimateReference
        Public ReadOnly Label As String
        Public ReadOnly Visit As Double
        Public ReadOnly Group As Double
        Public ReadOnly Estimate As Double
        Public ReadOnly StdError As Double

        Public Sub New(label As String,
                       visit As Double,
                       group As Double,
                       estimate As Double,
                       stdError As Double)
            Me.Label = label
            Me.Visit = visit
            Me.Group = group
            Me.Estimate = estimate
            Me.StdError = stdError
        End Sub
    End Class

End Class

' ===== END migrated from MMRMMulticovariateMissingReferenceTests.vb =====

