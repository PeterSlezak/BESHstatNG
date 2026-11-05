using BESHStatNG;
using BESHStatNG.AppInfrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.OfficeAddin;

[TestClass]
public class Output28ContractTests
{
    [DataTestMethod]
    [DataRow(0)] [DataRow(1)] [DataRow(2)]
    public void OfficeContractKeepsTypedCellsRolesAndDestinationsAlongsideWindowsIntent(int destination)
    {
        var values = new object[,] {
            { "Title", "", "" }, { "Row", "P", "Value" },
            { "a", .05, true }, { "b", "0,05", 7d }, { "Footnote", "", "" } };
        var model = new ResultTableOutputModel(values, headerTopRows: 1, headerLeftColumns: 1,
            footerRows: 1, pvalueColumns: [1], titleRows: 1);
        var target = new SpreadsheetOutputTarget { Destination = (SpreadsheetOutputDestination)destination,
            WorksheetName = "Results", StartRow = 3, StartColumn = 4 };
        var settings = new PValuePresentationSettings { DecimalPlaces = 3 };
        var document = new OutputDocument([new OutputTableArtifact("table", model)]);
        var semantic = OutputSemanticPlanner.Create(document, settings, .05);
        var layout = OutputLayoutPlanner.Create(document, target, semanticPlan: semantic);
        var windows = ResultTablePresentationIntent.Capture(new ResultTableOutputBlock(3, 4, model), "table", 0);
        if (destination == 2)
        {
            var error = Assert.ThrowsException<ArgumentException>(() => OfficeOutputPlanAdapter.Prepare(document, semantic, layout));
            Assert.AreEqual("Unsupported Office output destination.", error.Message);
            Assert.AreEqual(3, windows.FirstBodyRow);
            return; // Existing Office limitation; no new Office workbook support is claimed.
        }
        var office = OfficeOutputPlanAdapter.Prepare(document, semantic, layout);
        Assert.AreEqual(1, office.SchemaVersion);
        Assert.AreEqual(destination, office.Target.Destination);
        Assert.AreEqual("R3C4:R7C6", office.UsedBounds.ToString());
        Assert.AreEqual(PValuePresentation.BuildExcelNumberFormat(settings), office.PvalueNumberFormat);
        foreach (var cell in office.Tables.Single().Cells)
            Assert.AreEqual(windows.RoleAt(cell.Row + 1, cell.Column + 1).ToString(), cell.Role);
        var numeric = office.Tables.Single().Cells.Single(c => c.Row == 2 && c.Column == 1);
        var text = office.Tables.Single().Cells.Single(c => c.Row == 3 && c.Column == 1);
        Assert.IsTrue(numeric.IsSignificant);
        Assert.IsFalse(text.IsSignificant, "Typed text remains distinct from legacy Windows coercion");
        values[2, 1] = .9; settings.DecimalPlaces = 9; target.Destination = SpreadsheetOutputDestination.NewWorkbook;
        Assert.IsTrue(numeric.IsSignificant);
        Assert.AreEqual(destination, office.Target.Destination);
        Assert.AreEqual(3, windows.FirstBodyRow);
    }
}
