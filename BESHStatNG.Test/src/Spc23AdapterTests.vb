Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Spc23AdapterTests
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer(id As Integer)
        Dim input = Spc23Fixture.InputFor(id)
        input.Context = Spc23LegacyPreparation.Map(input.FirstRow, input.FirstRow + 149, input.Grouped, input.Ids)
        Spc23Fixture.FillGrids(input, 0)
        Dim result = SpcMultivariate.Fit(Spc23Fixture.Prepare(input, True))
        Dim method = GetType(BESHStatNG.Ui22MultivariateControlCharts).GetMethod("BuildChartRenderingResult", BindingFlags.Instance Or BindingFlags.NonPublic)
        Assert.IsNotNull(method)
        'Inspect the compiled shipping call site without constructing a WinForms object.
        Dim bytes = method.GetMethodBody().GetILAsByteArray(), called As New Collections.Generic.List(Of String)
        For offset = 0 To bytes.Length - 5
            If bytes(offset) <> &H28 AndAlso bytes(offset) <> &H6F Then Continue For
            Try
                Dim target = method.Module.ResolveMethod(BitConverter.ToInt32(bytes, offset + 1))
                called.Add(target.DeclaringType.FullName & "." & target.Name)
            Catch ex As ArgumentException
                'Non-instruction byte positions cannot resolve as metadata tokens.
            End Try
        Next
        Assert.IsTrue(called.Contains(GetType(SpcMultivariateResultAxisPolicy).FullName & ".Project"))
        Assert.IsTrue(called.Contains(GetType(SpcMultivariateResultAxisPolicy).FullName & ".RenderingInput"))
        Dim nativeInput = SpcMultivariateResultAxisPolicy.RenderingInput(result.Panels(0))
        Assert.AreEqual(Spc23Fixture.Hash(Spc23LegacyPreparation.RenderingInput(result.Panels(0))), Spc23Fixture.Hash(nativeInput))
        Dim wrapped As New SpcFitResult(New SpcFitRequest(result.Request.ChartType, nativeInput), result.Panels, result.Warnings)
        Assert.AreEqual(result.Request.ChartType, wrapped.ChartType)
        Assert.AreEqual(result.Panels.Length, wrapped.Panels.Length)
        For i = 0 To result.Panels.Length - 1
            Assert.AreEqual(Spc23Fixture.Hash(result.Panels(i).Points), Spc23Fixture.Hash(wrapped.Panels(i).Points))
            Assert.AreEqual(Spc23Fixture.Hash(result.Panels(i).Signals), Spc23Fixture.Hash(wrapped.Panels(i).Signals))
        Next
        Dim projected = SpcUnivariateChartProjectionPolicy.Project(wrapped, True)
        Assert.AreEqual(wrapped.Panels(0).Points.Length, projected.Axis.Count)
        Assert.AreEqual(result.Panels(0).Points.Where(Function(p) p.IsSignalled AndAlso p.HasFiniteValue).Count(), projected.Panels(0).Channels(CInt(SpcChartChannel.Signal)).Where(Function(v) v.HasValue).Count())
    End Sub
End Class
