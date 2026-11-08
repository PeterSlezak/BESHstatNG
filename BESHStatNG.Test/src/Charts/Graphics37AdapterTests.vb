Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Reflection.Emit
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Frozen37
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Graphics37AdapterTests
    Private Shared Function Calls(method As MethodBase) As Collections.Generic.List(Of MethodBase)
        Dim opcodes = GetType(OpCodes).GetFields(BindingFlags.Public Or BindingFlags.Static).Where(Function(f) f.FieldType Is GetType(OpCode)).Select(Function(f) DirectCast(f.GetValue(Nothing), OpCode)).ToDictionary(Function(o) o.Value)
        Dim bytes = method.GetMethodBody().GetILAsByteArray(), i = 0, result As New Collections.Generic.List(Of MethodBase)
        While i < bytes.Length
            Dim value As Short = CShort(bytes(i)) : i += 1
            If value = &HFE Then value = CShort(-512 + CInt(bytes(i))) : i += 1
            Dim op = opcodes(value), size = 0
            Select Case op.OperandType
                Case OperandType.InlineNone : size = 0
                Case OperandType.ShortInlineBrTarget, OperandType.ShortInlineI, OperandType.ShortInlineVar : size = 1
                Case OperandType.InlineVar : size = 2
                Case OperandType.InlineI8, OperandType.InlineR : size = 8
                Case OperandType.InlineSwitch : size = 4 + 4 * BitConverter.ToInt32(bytes, i)
                Case Else : size = 4
            End Select
            If op.OperandType = OperandType.InlineMethod Then result.Add(method.Module.ResolveMethod(BitConverter.ToInt32(bytes, i)))
            i += size
        End While
        Return result
    End Function
    <DataTestMethod><DataRow("LoadingPlot2D")><DataRow("LoadingPlot3D")><DataRow("ScorePlot2D")><DataRow("ScorePlot3D")><DataRow("Biplot")><DataRow("ScreePlot")>
    Public Sub EveryShippingPcaPathConsumesPortablePreparation(name As String)
        Dim method = GetType(graphics.PcaPlotExcel).GetMethod(name), called = Calls(method)
        Assert.IsTrue(called.Any(Function(m) m.DeclaringType Is GetType(MultivariateChartPresentation37)), name)
        If name <> "ScreePlot" Then Assert.IsTrue(called.Any(Function(m) m.DeclaringType Is GetType(MultivariatePlotGeometry37)), name)
        If name.Contains("3D") Then
            For Each operation In {"dataInputs", "axesLabelInputs", "showPlanePointInputs", "ScaleAxis", "settingsInputs", "SetDataLabels", "draw"}
                Assert.IsTrue(called.Any(Function(m) m.DeclaringType Is GetType(graphics.XYZscatter) AndAlso m.Name = operation), operation)
            Next
        End If
    End Sub
    <DataTestMethod><DataRow("ScreePlot")><DataRow("LoadingPlot2D")><DataRow("LoadingPlot3D")>
    Public Sub EveryShippingFactorPathConsumesExactPercentAndCaptionPreparation(name As String)
        Dim called = Calls(GetType(graphics.FactorAnalysisPlotExcel).GetMethod(name))
        Assert.IsTrue(called.Any(Function(m) m.DeclaringType Is GetType(MultivariatePlotGeometry37) AndAlso m.Name = If(name = "ScreePlot", "InitialFactorPercent", "FactorPercent")))
        Assert.IsTrue(called.Any(Function(m) m.DeclaringType Is GetType(MultivariateChartPresentation37)))
    End Sub
    <TestMethod>
    Public Sub BothShippingCaRoutesConsumePortableCoordinatesLabelsAndBounds()
        Dim contribution = Calls(GetType(graphics.CorrespondenceAnalysisPlotExcel).GetMethod("ContributionPlot"))
        Assert.IsTrue(contribution.Any(Function(m) m.DeclaringType Is GetType(MultivariatePlotGeometry37) AndAlso m.Name = "Contribution"))
        Assert.IsTrue(contribution.Any(Function(m) m.DeclaringType Is GetType(MultivariateChartPresentation37) AndAlso m.Name = "ContributionLabels"))
        Dim map = Calls(GetType(graphics.CorrespondenceAnalysisPlotExcel).GetMethod("CorrespondencePlot"))
        Assert.IsTrue(map.Any(Function(m) m.DeclaringType Is GetType(MultivariatePlotGeometry37) AndAlso m.Name = "CorrespondenceAxes"))
        For Each name In {"CorrespondenceSeries", "CorrespondenceLabel", "CorrespondenceCaption"}
            Assert.IsTrue(map.Any(Function(m) m.DeclaringType Is GetType(MultivariateChartPresentation37) AndAlso m.Name = name))
        Next
        Assert.AreEqual(2, map.Where(Function(m) m.DeclaringType Is GetType(MultivariateChartPresentation37) AndAlso m.Name = "CorrespondenceSeries").Count())
    End Sub
    <TestMethod>
    Public Sub DendrogramEntryPointsReachExistingLayoutAndPortablePresentation()
        Dim host = GetType(graphics.ClusterAnalysisPlotExcel), target = host.GetMethod("CreateExcelChart")
        Assert.IsTrue(Calls(host.GetMethod("Draw")).Contains(target))
        Dim result = Calls(host.GetMethod("CreateDendrogramChart"))
        Assert.IsTrue(result.Contains(target))
        Assert.IsTrue(result.Any(Function(m) m.Name = "CreateDendrogramLayout" AndAlso m.DeclaringType Is GetType(Multivariate.HierarchicalClusterResult)))
        Dim rendering = Calls(target)
        Assert.IsTrue(rendering.Any(Function(m) m.DeclaringType Is GetType(DendrogramChartGeometry37) AndAlso m.Name = "Bounds"))
        For Each name In {"BaseSeries", "Clusters", "CutSeries", "LeafSeries"}
            Assert.IsTrue(rendering.Any(Function(m) m.DeclaringType Is GetType(DendrogramChartPresentation37) AndAlso m.Name = name))
        Next
        Assert.AreEqual(9, target.GetParameters().Length)
        Assert.AreEqual("A1", target.GetParameters()(3).DefaultValue)
        Assert.AreEqual(480.0, target.GetParameters()(4).DefaultValue)
        Assert.AreEqual(320.0, target.GetParameters()(5).DefaultValue)
    End Sub
    <TestMethod>
    Public Sub NullAndDimensionGuardsExitBeforeNativeCreation()
        For Each host In New Type(){GetType(graphics.PcaPlotExcel), GetType(graphics.FactorAnalysisPlotExcel), GetType(graphics.CorrespondenceAnalysisPlotExcel)}
            For Each method In host.GetMethods(BindingFlags.Public Or BindingFlags.Static)
                Dim args = method.GetParameters().Select(Function(p) If(p.Position = 0, Nothing, If(p.IsOptional, p.DefaultValue, If(p.ParameterType Is GetType(Boolean), CType(False, Object), CType(0, Object))))).ToArray()
                Dim failure = Assert.ThrowsException(Of TargetInvocationException)(Function() method.Invoke(Nothing, args))
                Assert.AreEqual("model", DirectCast(failure.InnerException, ArgumentNullException).ParamName)
            Next
        Next
        graphics.PcaPlotExcel.LoadingPlot2D(Cases37.PcaModel(1))
        graphics.PcaPlotExcel.ScorePlot2D(Cases37.PcaModel(1))
        graphics.PcaPlotExcel.Biplot(Cases37.PcaModel(1), -1)
        graphics.FactorAnalysisPlotExcel.LoadingPlot3D(Cases37.FactorModel(2))
        GetType(graphics.CorrespondenceAnalysisPlotExcel).GetMethod("CorrespondencePlot").Invoke(Nothing, New Object(){Cases37.CaModel(1), Nothing})
    End Sub
End Class
