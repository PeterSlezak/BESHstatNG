Option Explicit On
Option Strict On

Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure

<TestClass>
Public Class SettingsAndPValueTests

    <TestMethod>
    Public Sub BeshStatNgSettings_Defaults_AreCorrect()

        Dim settings As New BeshStatNgSettings()

        Assert.AreEqual(3, settings.Version)
        Assert.AreEqual(0.05, settings.DefaultAlpha, 0.0)
        Assert.AreEqual(Integer.MinValue, settings.DefaultRandomSeed)

        Assert.IsNotNull(settings.Diagnostics)
        Assert.IsNotNull(settings.PValuePresentation)

        Assert.AreEqual(
            PValuePresentationSettings.DefaultDecimalPlaces,
            settings.PValuePresentation.DecimalPlaces)

        Assert.AreEqual(
            PValueSmallValueDisplay.LessThanThreshold,
            settings.PValuePresentation.SmallValueDisplay)

        Assert.IsTrue(settings.PValuePresentation.UseUpperBound)

    End Sub


    <TestMethod>
    Public Sub PValuePresentation_DefaultFormatting_PreservesExpectedDisplayContract()

        Dim settings As New PValuePresentationSettings()

        Dim value As Double = 0.000525773592093826

        Dim actual As String =
            PValuePresentation.FormatForDisplay(
                value,
                settings,
                CultureInfo.InvariantCulture)

        Assert.AreEqual("0.00052577", actual)

    End Sub


    <TestMethod>
    Public Sub PValuePresentation_VerySmallPValue_UsesLowerThreshold()

        Dim settings As New PValuePresentationSettings()

        Dim actual As String =
            PValuePresentation.FormatForDisplay(
                0.000000001,
                settings,
                CultureInfo.InvariantCulture)

        Assert.AreEqual("<0.00000001", actual)

    End Sub


    <TestMethod>
    Public Sub SettingsStore_RoundTrip_PreservesValues()

        Dim tempDirectory As String =
            Path.Combine(
                Path.GetTempPath(),
                "BESHStat_CoreTests_" & Guid.NewGuid().ToString("N"))

        Directory.CreateDirectory(tempDirectory)

        Try
            Dim store As New BeshStatNgSettingsStore(tempDirectory)

            Dim original As New BeshStatNgSettings With {
                .DefaultAlpha = 0.01,
                .DefaultRandomSeed = 12345
            }

            original.PValuePresentation.DecimalPlaces = 6
            original.PValuePresentation.SmallValueDisplay =
                PValueSmallValueDisplay.ScientificNotation
            original.PValuePresentation.UseUpperBound = False

            store.Save(original)

            Dim loaded As BeshStatNgSettings = store.Load()

            Assert.AreEqual(0.01, loaded.DefaultAlpha, 0.0)
            Assert.AreEqual(12345, loaded.DefaultRandomSeed)

            Assert.AreEqual(
                6,
                loaded.PValuePresentation.DecimalPlaces)

            Assert.AreEqual(
                PValueSmallValueDisplay.ScientificNotation,
                loaded.PValuePresentation.SmallValueDisplay)

            Assert.IsFalse(
                loaded.PValuePresentation.UseUpperBound)

        Finally
            If Directory.Exists(tempDirectory) Then
                Directory.Delete(tempDirectory, True)
            End If
        End Try

    End Sub

End Class