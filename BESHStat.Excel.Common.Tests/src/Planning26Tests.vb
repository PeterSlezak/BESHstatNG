Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Planning26Tests
    Public Shared Iterator Function Cases() As IEnumerable(Of Object())
        For Each culture In {"", "sk-SK"}
            For method = 0 To 12
                For profile = 0 To 7 : Yield New Object() {method, profile, culture} : Next
            Next
        Next
    End Function
    Public Shared Function Request(method As Integer, profile As Integer) As SampleSizePlanningRequest
        Dim values As Double() = Nothing
        Select Case method
            Case 0 : values = {0.75R, 0.4R, 0.3R, 1.0R}
            Case 1 : values = {1.25R, 1.0R, 0.0R, 0.4R}
            Case 2 : values = {0.75R, 1.0R, 0.0R, 0.4R}
            Case 3 : values = {0.6R, 0.8R, 2.0R}
            Case 4 : values = {10.0R, 5.0R, 1.96R}
            Case 5, 6 : values = {0.4R, 0.4R, 0.1R, 1.0R}
            Case 7 : values = {0.5R, 0.4R, 1.0R}
            Case 8 : values = {0.5R, 0.4R}
            Case 9 : values = {5.0R, 10.0R}
            Case 10, 11 : values = {0.0R, 5.0R, 10.0R, 1.0R}
            Case 12 : values = {5.0R, 10.0R, 1.0R}
        End Select
        Dim alpha = 0.05R, beta = If(method = 4, Double.NaN, 0.2R)
        Select Case profile
            Case 1 : values(0) *= 0.9R
            Case 2 : values(0) = -1
            Case 3 : values(0) = Double.NaN
            Case 4 : alpha = 0
            Case 5 : beta = 0
            Case 6 : values(0) = 0
            Case 7
                If method = 1 OrElse method = 2 Then
                    values(3) = Double.NaN : values(2) = 0
                ElseIf method = 3 Then
                    values(2) = 4
                ElseIf method = 9 OrElse method = 12 Then
                    values(0) = 5.001R
                End If
        End Select
        Dim observations = If(method = 3 AndAlso profile = 7, 4, 2)
        Return New SampleSizePlanningRequest(CType(method, SampleSizePlanningMethod), alpha, beta, values, observations, overallEventProvided:=profile <> 7, rSquaredProvided:=profile <> 7)
    End Function
    Public Shared Function Failure(action As Action) As Exception
        Try : action() : Return Nothing : Catch ex As Exception : Return ex : End Try
    End Function
    <DataTestMethod, DynamicData(NameOf(Cases), DynamicDataSourceType.Method)>
    Public Sub ExactOriginalCoreArgumentsAndParagraphs(method As Integer, profile As Integer, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim request = Planning26Tests.Request(method, profile), report As SampleSizePlanningReport = Nothing, original As Object = Nothing
            Dim failure = Planning26Tests.Failure(Sub() report = SampleSizePlanningPolicy.Calculate(request))
            Dim previous = Planning26Tests.Failure(Sub() original = Planning26LegacyCore.Calculate(request))
            Dim node As New XElement("comparison", New XAttribute("family", "planning"), New XAttribute("method", request.Method), New XAttribute("profile", profile), New XAttribute("culture", culture), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XElement("fixture_sha256", Spc23Fixture.Hash(request)))
            Assert.AreEqual(If(previous Is Nothing, "", previous.GetType().FullName), If(failure Is Nothing, "", failure.GetType().FullName))
            Assert.AreEqual(If(previous Is Nothing, "", previous.Message), If(failure Is Nothing, "", failure.Message))
            If failure IsNot Nothing Then
                node.Add(New XAttribute("classification", "PRESERVED_CORE_REJECTION"), New XElement("exception", New XAttribute("type", failure.GetType().FullName), failure.Message))
            Else
                Assert.AreEqual(Spc23Fixture.Hash(original), Spc23Fixture.Hash(report.CoreResult))
                Dim legacy = GetType(Planning26LegacyText).GetMethod(request.Method.ToString()).Invoke(Nothing, New Object() {request, report.CoreResult})
                Dim text = GetType(SampleSizeTextProjection).GetMethod(request.Method.ToString()).Invoke(Nothing, New Object() {request, report.CoreResult})
                Assert.AreEqual(CStr(legacy), CStr(text)) : Assert.AreEqual(CStr(legacy), report.Text)
                CollectionAssert.AreEqual(report.Text.Split({vbNewLine}, StringSplitOptions.None), report.Lines.ToArray())
                node.Add(New XAttribute("classification", "POSITIVE_PARITY"), New XElement("result_sha256", Spc23Fixture.Hash(report.CoreResult)), New XElement("text_sha256", Spc23Fixture.Hash(report.Text)), New XElement("text", report.Text), New XElement("line_count", report.Lines.Count))
            End If
            Emit(node)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    Public Shared Sub Emit(node As XElement)
        Console.WriteLine("04A26_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(node.ToString(SaveOptions.DisableFormatting))))
    End Sub
    <TestMethod>
    Public Sub RequestsCopyNumericIntentAndDoNotReinterpretOptionalZeroOrAbsentBeta()
        Dim values As Double() = {0.75R, 1, 0, Double.NaN}
        Dim request As New SampleSizePlanningRequest(SampleSizePlanningMethod.CoxEventCountBinaryCovariate, 0.05R, 0.2R, values, overallEventProvided:=False, rSquaredProvided:=False)
        values(0) = 99 : Dim copied = request.Inputs() : copied(0) = 88
        Assert.AreEqual(0.75R, request.Value(0)) : Assert.IsTrue(Double.IsNaN(request.Value(3)))
        Assert.AreEqual(0.0R, request.Value(2)) : Assert.IsFalse(request.RSquaredProvided)
        Assert.AreEqual(SampleSizeFamily.Cox, request.Description.Family)
        Assert.AreEqual("rSquaredWithOtherCovariates", request.Description.FieldRoles(2))
        Dim absent As New SampleSizePlanningRequest(SampleSizePlanningMethod.BlandAltmanLoASampleSize, 0.05R, Double.NaN, {10.0R, 5.0R, 1.96R})
        Assert.IsTrue(Double.IsNaN(absent.Beta))
        Dim first = SampleSizePlanningPolicy.Calculate(absent), second = SampleSizePlanningPolicy.Calculate(absent)
        Assert.AreEqual(first.Text, second.Text)
        Assert.AreNotSame(first, second)
    End Sub
    <TestMethod>
    Public Sub FamilyHypothesisDefaultsAndLegacyIccCountGuardRetainDistinctMeaning()
        For Each method As SampleSizePlanningMethod In [Enum].GetValues(GetType(SampleSizePlanningMethod))
            Dim defaults = SampleSizePlanningDefaults.Inputs(method)
            Dim copy = SampleSizePlanningDefaults.Inputs(method) : copy(0) = 999
            Assert.AreNotEqual(999.0R, SampleSizePlanningDefaults.Inputs(method)(0))
            Dim request As New SampleSizePlanningRequest(method, 0.05R, 0.2R, defaults, observationsPerSubject:=2)
            Assert.AreEqual(defaults.Length, request.Description.FieldRoles.Count)
        Next
        CollectionAssert.AreEqual(New Double() {0.5R, 0.4R, 1}, SampleSizePlanningDefaults.Inputs(SampleSizePlanningMethod.IndependentProportions))
        CollectionAssert.AreEqual(New Double() {0.4R, 0.5R, 1}, SampleSizePlanningDefaults.Inputs(SampleSizePlanningMethod.IndependentProportions, True))
        Assert.IsTrue(SampleSizePlanningDescriptions.Describe(SampleSizePlanningMethod.IccHypothesisTestSampleSize).AlphaIsOneSided)
        Assert.IsFalse(SampleSizePlanningDescriptions.Describe(SampleSizePlanningMethod.BlandAltmanLoASampleSize).BetaVisible)
        Dim value = 2.5R
        Assert.IsTrue(value < 2 OrElse value <> Math.Truncate(value), "Existing Windows guard rejects a nonintegral count before CInt")
        value = Double.PositiveInfinity
        Assert.IsFalse(value < 2 OrElse value <> Math.Truncate(value), "Candidate only: legacy guard admits Infinity")
        Assert.ThrowsException(Of OverflowException)(Sub()
                                                       Dim ignored = CInt(value)
                                                   End Sub)
    End Sub
End Class
