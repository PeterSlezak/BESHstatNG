using System.Globalization;
using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Immutable physical source plus a parser-only projection. No synthetic Excel address.</summary>
public sealed class WorksheetColumnCatalog
{
    private readonly SpreadsheetFrameCell[][] rows;
    private readonly SpecificationColumn[] columns;
    private readonly SpreadsheetRangeData metadata;
    public string DocumentIdentity { get; }
    public string WorksheetId => metadata.WorksheetId;
    public string WorksheetName => metadata.WorksheetName;
    public string Address => metadata.Address;
    public IReadOnlyList<SpecificationColumn> Columns => Array.AsReadOnly(columns);
    public WorksheetColumnCatalog(string documentIdentity, SpreadsheetRangeData physical)
    {
        if (physical.StartRow != 1 || physical.ColumnCount is < 1 or > 128 || physical.RowCount < 1 ||
            (long)physical.RowCount * physical.ColumnCount > 10000 || physical.Rows.Count != physical.RowCount ||
            physical.Rows.Any(r => r is null || r.Count != physical.ColumnCount)) throw new InvalidDataException();
        DocumentIdentity = documentIdentity;
        metadata = new() { WorkbookId=physical.WorkbookId, WorkbookName=physical.WorkbookName,
            WorksheetId=physical.WorksheetId, WorksheetName=physical.WorksheetName, Address=physical.Address,
            AddressDialect=physical.AddressDialect, StartRow=physical.StartRow, StartColumn=physical.StartColumn,
            RowCount=physical.RowCount, ColumnCount=physical.ColumnCount, SelectionId=physical.SelectionId,
            SelectionRevision=physical.SelectionRevision, DateSystem=physical.DateSystem, DateSerialOrigin=physical.DateSerialOrigin };
        rows = physical.Rows.Select((r,y) => r.Select((c,x) => new SpreadsheetFrameCell(c,y+1,x+1,y+1,physical.StartColumn+x)).ToArray()).ToArray();
        var found = new List<SpecificationColumn>();
        for (int x=0;x<physical.ColumnCount;x++)
        {
            if (rows.All(r => r[x].Kind == SpreadsheetCellKind.Blank)) continue;
            var header=rows[0][x]; var letter=BESHStatNG.RegressionVariableCatalog.NumberToLetters(physical.StartColumn+x);
            var text=header.Kind switch {
                SpreadsheetCellKind.Text => header.TextValue,
                SpreadsheetCellKind.Number => header.NumberValue.ToString(CultureInfo.CurrentCulture),
                SpreadsheetCellKind.Bool => header.BooleanValue.ToString(), _ => ""
            };
            var usable=!string.IsNullOrWhiteSpace(text);
            var name=header.Kind==SpreadsheetCellKind.Text && usable ? text.Trim() : "Column " + (found.Count+1);
            found.Add(new(found.Count,name,physical.StartColumn+x) { WorksheetId=physical.WorksheetId,
                DocumentIdentity=documentIdentity, HeaderText=text, HasHeader=usable,
                DisplayText=usable ? text+" | Var"+letter : "Var"+letter,
                SelectedColumn=x+1, Header=header });
        }
        columns=found.ToArray();
    }
    public IReadOnlyList<SpecificationColumn> ForHeader(SpreadsheetHeaderMode header) => Array.AsReadOnly(columns.Select(c =>
        header==SpreadsheetHeaderMode.None ? c with {Name="Column "+(c.Index+1)} : c).ToArray());
    public SpreadsheetRangeData PhysicalCopy()
    {
        return new() { WorkbookId=metadata.WorkbookId, WorkbookName=metadata.WorkbookName, WorksheetId=WorksheetId,
            WorksheetName=WorksheetName, Address=Address, AddressDialect=metadata.AddressDialect,
            StartRow=1, StartColumn=metadata.StartColumn, RowCount=rows.Length, ColumnCount=metadata.ColumnCount,
            SelectionId=metadata.SelectionId, SelectionRevision=metadata.SelectionRevision,
            DateSystem=metadata.DateSystem, DateSerialOrigin=metadata.DateSerialOrigin,
            Rows=rows.Select(r=>r.Select(c=>c.ToCellValue()).ToList()).ToList() };
    }
    public SpreadsheetRangeData ParserCopy() => new() {
        // Zero/empty absolute column/address deliberately mean unknown, never a fictional A:C range.
        WorkbookId=metadata.WorkbookId, WorkbookName=metadata.WorkbookName, WorksheetId=WorksheetId, WorksheetName=WorksheetName,
        SelectionId=metadata.SelectionId, SelectionRevision=metadata.SelectionRevision, StartRow=1, StartColumn=0,
        RowCount=rows.Length, ColumnCount=columns.Length, DateSystem=metadata.DateSystem, DateSerialOrigin=metadata.DateSerialOrigin,
        Rows=rows.Select(r=>columns.Select(c=>r[c.SelectedColumn-1].ToCellValue()).ToList()).ToList()
    };
    public bool SameSource(WorksheetColumnCatalog other) => DocumentIdentity==other.DocumentIdentity &&
        WorksheetId==other.WorksheetId && WorksheetName==other.WorksheetName && Address==other.Address &&
        rows.Length==other.rows.Length && metadata.ColumnCount==other.metadata.ColumnCount &&
        rows.SelectMany(r=>r).Zip(other.rows.SelectMany(r=>r)).All(p=>
            p.First.Kind==p.Second.Kind && BitConverter.DoubleToInt64Bits(p.First.NumberValue)==BitConverter.DoubleToInt64Bits(p.Second.NumberValue) &&
            p.First.TextValue==p.Second.TextValue && p.First.BooleanValue==p.Second.BooleanValue && p.First.NumberFormat==p.Second.NumberFormat && p.First.NativeErrorCode==p.Second.NativeErrorCode);

    public SpreadsheetValidationIssue Translate(SpreadsheetValidationIssue issue)
    {
        var c=issue.SelectedColumn>0 && issue.SelectedColumn<=columns.Length ? columns[issue.SelectedColumn-1] : null;
        return new(issue.Code,issue.Severity,issue.Message,issue.FieldPath,issue.SelectedRow,c?.SelectedColumn??0,
            issue.WorksheetRow,c?.WorksheetColumn??0);
    }
    private SpreadsheetNumericObservation Numeric(SpreadsheetNumericObservation n) => new(n.Value,n.SelectedRow,
        columns[n.SelectedColumn-1].SelectedColumn,n.WorksheetRow,columns[n.SelectedColumn-1].WorksheetColumn);
    private SpreadsheetRepeatedObservation Repeated(SpreadsheetRepeatedObservation r) => new(r.Subject,r.Visit,r.VisitTime,r.VisitDate,Numeric(r.Response),r.TreatmentGroup);
    public SpreadsheetAnalysisInput? Translate(SpreadsheetAnalysisInput? specification) => specification switch {
        SpreadsheetModelInput m => new SpreadsheetModelInput(m.ResponseName,m.Response,m.RawPredictorMatrix,m.DesignMatrix,
            m.PredictorNames,m.RawPredictorNames,m.RequiredPredictorNames,m.SourceRows,
            m.CategoricalColumns.Select(c=>new SpreadsheetModelCategoricalColumn(c.ColumnName,columns[c.SelectedColumn-1].SelectedColumn,c.Levels)),
            m.OriginalFormula,m.NormalizedFormula,m.IncludesIntercept,m.MatrixRank,m.WorkbookId,m.WorksheetId,m.SelectionRevision),
        SpreadsheetRepeatedSubjectInput r => new SpreadsheetRepeatedSubjectInput(r.Observations.Select(Repeated)),
        SpreadsheetMixedModelInput m => new SpreadsheetMixedModelInput(m.Rows.Select(r=>new SpreadsheetMixedModelObservation(Repeated(r.Repeated),
            r.Covariates.Select(c=>new SpreadsheetFrameCell(c.ToCellValue(),c.SelectedRow,columns[c.SelectedColumn-1].SelectedColumn,c.WorksheetRow,columns[c.SelectedColumn-1].WorksheetColumn)),r.RandomGroups)),m.CovariateNames,m.RandomGroupNames),
        SpreadsheetCategoricalVectorInput c => new SpreadsheetCategoricalVectorInput(c.Name,c.Observations.Select(o=>new SpreadsheetCategoricalObservation(o.Level,o.SelectedRow,
            columns[o.SelectedColumn-1].SelectedColumn,o.WorksheetRow,columns[o.SelectedColumn-1].WorksheetColumn)),c.Levels),
        _ => specification
    };
}
