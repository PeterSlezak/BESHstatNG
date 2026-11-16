Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Web.Script.Serialization
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Test
Module GeeObserver47
 Sub Main(args As String())
  Dim rows As New List(Of Object)
  For Each culture In {"", "en-US", "sk-SK"}
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
   For Each binary In {False, True}
    Dim raw = Gee12Fixture.Raw(If(binary, "Binomial|Logit|Independence|Robust|time", "Gaussian|Identity|Independence|Robust|time"))
    Dim request = Gee12PolicyTests.Request("Gaussian|Identity|Independence|Robust|time", raw)
    Dim fit = request.CreateCoreModel() : fit.Fit(False)
    Dim report = BESHStatNG.WindowsGeeWorkflowAdapter.Report(request, fit.wrapResults(), fit.PredictedResponses, fit.AllResiduals, fit.ObservedResponses, fit.ObservationWeights)
    Dim options As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True, .ThresholdTable = True}
    Dim exceptionType As String = Nothing, exceptionMessage As String = Nothing, tables = 0
    Try
     tables = BESHStatNG.WindowsGeeWorkflowAdapter.DeferredPostFitFamilyReport(report, options, Double.NaN).LegacyTables().Count
    Catch ex As ArgumentException
     exceptionType = ex.GetType().FullName : exceptionMessage = ex.Message
    End Try
    If binary AndAlso (exceptionType IsNot Nothing OrElse tables = 0) Then Throw New InvalidOperationException("Binary mismatch branch changed")
    If Not binary AndAlso (exceptionType <> "System.ArgumentException" OrElse exceptionMessage <> "Observed outcomes must be coded as 0/1.") Then Throw New InvalidOperationException("Nonbinary shipping error changed")
    If GeeReportPolicy.Classification(report, options).LegacyTables().Count <> 0 Then Throw New InvalidOperationException("Fitted-family Common boundary changed")
    rows.Add(New With {.culture = culture, .binaryResponses = binary, .shippingException = exceptionType, .message = exceptionMessage, .legacyTables = tables, .commonTables = 0})
   Next
  Next
  IO.File.WriteAllText(args(0), New JavaScriptSerializer().Serialize(rows))
 End Sub
End Module
