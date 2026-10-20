Option Explicit On
Option Strict On

Namespace ExcelCommon
    ''' <summary>Synthetic opt-in demonstration only; never appended to statistical results.</summary>
    Public NotInheritable Class CompositeOutputExample
        Public Shared Function Create(decision As OutputTargetDecision) As CompositeOutputPlan
            Dim values As Object(,) = {{"Composite demo", Nothing, Nothing, Nothing, Nothing, Nothing},
                {"Group", "N", "Mean", "Flag", "Literal", "p"},
                {"Synthetic", 3, 1.5, True, "=1+1", 0.01},
                {"Embedded footer (already in table)", Nothing, Nothing, Nothing, Nothing, Nothing}}
            Dim main As New OutputTableArtifact("main", New Global.BESHStatNG.ResultTableOutputModel(values,
                headerTopRows:=1, footerRows:=1, titleRows:=1, pvalueColumns:={6}))
            Dim warning As New OutputDiagnosticArtifact("warning", OutputDiagnosticSeverity.Warning,
                "demo.warning", "Synthetic warning; no analysis was run.", "demo.input")
            Dim plot As New OutputPlotArtifact("plot", "demo-distribution", "Synthetic distribution",
                "Chart preview", "No statistical chart data or native chart is supplied.")
            Dim second As New OutputTableArtifact("second", New Global.BESHStatNG.ResultTableOutputModel(
                New Object(,) {{"Additional table", Nothing, Nothing}, {"Synthetic", 2, 0.5}}))
            Dim document As New OutputDocument({main, warning, plot, second})
            Return CompositeOutputPlanner.Create("composite-demo-v1", document, {
                CompositeOutputItem.FromArtifact(main), CompositeOutputItem.FromArtifact(warning, 1, 6),
                CompositeOutputItem.FromArtifact(plot, 8, 6),
                CompositeOutputItem.Note("note", "Additional note: synthetic demonstration only.", 1, 6, "main"),
                CompositeOutputItem.Link("link", "Example report", "https://example.org/report", 1, 6, "main"),
                CompositeOutputItem.FromArtifact(second)}, decision)
        End Function
    End Class
End Namespace
