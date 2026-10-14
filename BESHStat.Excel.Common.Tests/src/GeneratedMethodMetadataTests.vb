Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Batch 00.02B: generated public metadata vs the two executable descriptors.</summary>
<TestClass>
Public Class GeneratedMethodMetadataTests

    <TestMethod>
    Public Sub GeneratedMethods_DescribeAllAuthoredFeatures_ButOnlyTwoHaveRuntimeIds()
        Assert.AreEqual(76, GeneratedMethodMetadata.All.Count)
        Assert.AreEqual(76, GeneratedMethodMetadata.All.Select(Function(m) m.Key).Distinct(StringComparer.Ordinal).Count())
        Assert.AreEqual(2, GeneratedMethodMetadata.All.Count(Function(m) m.AnalysisId IsNot Nothing))
        For Each method As AnalysisMethodMetadata In GeneratedMethodMetadata.All
            Assert.IsFalse(String.IsNullOrWhiteSpace(method.DisplayName))
            Assert.IsFalse(String.IsNullOrWhiteSpace(method.Category))
            Assert.IsFalse(String.IsNullOrWhiteSpace(method.Description))
            StringAssert.StartsWith(method.HelpUrl, "https://www.beshstat.eu/beshstatng/help/latest/methods/")
            Assert.IsFalse(method.Description.Contains("**"), method.Key)
            Assert.AreSame(method, GeneratedMethodMetadata.GetByKey(method.Key))
        Next
        Assert.AreEqual(2, AnalysisCatalog.All.Count, "Documentation-only methods must never become executable.")
    End Sub

    <TestMethod>
    Public Sub CatalogDescriptors_UseGeneratedAuthoredDisplayFields_WithoutChangingExecutionContracts()
        For Each descriptor As IAnalysisDescriptor In AnalysisCatalog.All
            Dim method As AnalysisMethodMetadata = GeneratedMethodMetadata.GetByAnalysisId(descriptor.Id)
            Assert.AreEqual(method.DisplayName, descriptor.DisplayName)
            Assert.AreEqual(method.Category, descriptor.Category)
            Assert.AreEqual(method.Description, descriptor.Description)
            Assert.AreEqual(method.HelpUrl, descriptor.HelpUrl)
            Assert.IsTrue(descriptor.LegacyKind.HasValue)
            Assert.AreSame(descriptor, AnalysisCatalog.GetByLegacyKind(descriptor.LegacyKind.Value))
            Assert.AreNotSame(descriptor.CreateDefaultOptions(), descriptor.CreateDefaultOptions())
        Next
        Assert.AreEqual("One-Way ANOVA", AnalysisCatalog.GetById(AnalysisIds.OneWayAnova).DisplayName)
        Assert.AreEqual("Kruskal-Wallis Test", AnalysisCatalog.GetById(AnalysisIds.KruskalWallis).DisplayName)
    End Sub

    <TestMethod>
    Public Sub Metadata_LookupsAreOrdinal_ReadOnly_AndRejectUnknownIds()
        Dim item As AnalysisMethodMetadata = Nothing
        Assert.IsFalse(GeneratedMethodMetadata.TryGetByAnalysisId(Nothing, item))
        Assert.IsNull(item)
        Assert.IsFalse(GeneratedMethodMetadata.TryGetByAnalysisId("ANOVA.ONEWAY", item))
        Assert.IsFalse(GeneratedMethodMetadata.TryGetByAnalysisId("not.registered", item))
        Assert.IsTrue(GeneratedMethodMetadata.TryGetByAnalysisId(AnalysisIds.OneWayAnova, item))
        Assert.AreEqual("OneWayANOVA", item.Key)
        Assert.ThrowsException(Of KeyNotFoundException)(Sub() GeneratedMethodMetadata.GetByAnalysisId("not.registered"))
        Assert.ThrowsException(Of KeyNotFoundException)(Sub() GeneratedMethodMetadata.GetByKey("unknown"))
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(GeneratedMethodMetadata.All, IList(Of AnalysisMethodMetadata)).Clear())
    End Sub
End Class
