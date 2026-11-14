"""Whole-source reconstruction of each extracted/pre-existing report phase."""
import json,re,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def need(value,message):
    if not value:raise ValueError(message)
def tokens(value):
    return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',value) if not x.startswith("'")]
def member(source,name,kind='Function'):
    source=source.replace('\r\n','\n')
    match=re.search(r'^([ \t]*)(?:Public|Friend|Private)(?: Shared)? '+kind+' '+name+r'\b',source,re.M)
    need(match is not None,'Missing complete declaration '+name)
    end=source.index('\n'+match[1]+'End '+kind,match.end())+len('\n'+match[1]+'End '+kind)
    return source[match.start():end]
def block(source,kind='Function'):
    lines=source.replace('\r\n','\n').splitlines()
    start=next(i for i,line in enumerate(lines) if (') As ' in line if kind=='Function' else line.rstrip().endswith(')')))
    return '\n'.join(lines[start+1:-1])
def inspect(root=ROOT,overrides=None):
    root=Path(root);overrides=overrides or {};prefix=HERE.relative_to(ROOT).as_posix()+'/'
    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()
    def text(path):return raw(path).decode('utf-8-sig').replace('\r\n','\n')
    contracts={r['current_record']['name']:r for r in json.loads(raw(prefix+'whole-body-contracts-v1.json'))['contracts']}
    numeric=text('BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb');core=text('BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb')
    report=text('BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb');spill=text('BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb')
    rows=[]
    def same(name,left,right):
        need(tokens(left)==tokens(right),'Complete source reconstruction differs '+name)
        rows.append(dict(name=name,tokens=len(tokens(left)),reconstructed_token_sha256=hashlib.sha256(json.dumps(tokens(left)).encode()).hexdigest()))
    for parent,helper,call,mapping in [
        ('TryGetTrimmedColumnObject','CopyTrimmedColumnPrepared','Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedColumnPrepared(arr, startRow, rows, col)',{'source':'arr','column':'col'}),
        ('TryGetTrimmedNumericMatrixObject','CopyTrimmedMatrixPrepared','Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedMatrixPrepared(arr, startRow, rows, cols, mat)',{'source':'arr','matrix':'mat','columns':'cols'})]:
        moved=block(member(numeric,helper,'Sub'),'Sub')
        for before,after in mapping.items():moved=re.sub(r'\b'+before+r'\b',after,moved)
        c=contracts[parent];need(c['current_whole_body'].count(call)==1,'Missing actual prepared call '+parent)
        same(parent+' full original body after prepared-phase substitution',c['original_whole_body'],c['current_whole_body'].replace(call,moved))
    moved=block(member(core,'TryBuildBoxCovariances'));moved=moved[:moved.rindex('Return True')]
    moved=moved.replace('ReDim covCube(g - 1, p - 1, p - 1)','Dim covCube(g - 1, p - 1, p - 1) As Double').replace('ReDim sampleSizes(g - 1)','Dim sampleSizes(g - 1) As Integer').replace('Return False','Return ExcelError.ExcelErrorNum').replace('Global.BESHStatNG.Matrix.MatrixStatisticsCore.SampleCovariance(mat)','Matrix.MatCovar(mat)')
    c=contracts['BOX_M'];pattern=r'Dim covCube\(,,\) As Double = Nothing\s+Dim sampleSizes\(\) As Integer = Nothing\s+If Not Global\.BESHStatNG\.WorksheetCompatibility\.UdfNumericPreparation38\.TryBuildBoxCovariances\(groupMats, covCube, sampleSizes\) Then Return ExcelError\.ExcelErrorNum'
    reconstructed,count=re.subn(pattern,lambda _:moved,c['current_whole_body']);need(count==1,'Missing complete Box-M prepared boundary')
    same('BOX_M entire original body; exact MatCovar facade target separately verified',c['original_whole_body'],reconstructed)
    for name in ['BuildGrubbsTable','BuildRosnerTable']:
        moved=block(member(report,name)).replace('Return New UdfReportProjection38(t.ToOutputModel(), UdfReportPreparation38.NormalizeBlanks)','Return PrepareResultTableForUdf(t)')
        same(name+' entire original report body',block(contracts[name]['original_whole_body']),moved)
    moved=block(member(report,'BuildSimpleNoteTable')).replace('Return New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.Raw)','Return out')
    same('BuildSimpleNoteTable entire raw string-only original body',block(contracts['BuildSimpleNoteTable']['original_whole_body']),moved)
    moved=block(member(report,'MakeMetricValueTable')).replace('Return New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.Raw)','Return out')
    same('MakeMetricValueTable entire original integer report body',block(contracts['MakeMetricValueTable']['original_whole_body']),moved)
    moved=block(member(spill,'TwoValueTable')).replace('Global.BESHStatNG.ResultTable()','ResultTable').replace('New UdfSourceCellReference38(0)','value1').replace('New UdfSourceCellReference38(1)','value2').replace('firstLabel','label1').replace('secondLabel','label2').replace('Return New UdfReportProjection38(table.ToOutputModel(), UdfReportPreparation38.NormalizeBlanks)','Return PrepareResultTableForUdf(table)')
    moved=re.sub(r'\btable\b','t',moved)
    same('BuildTwoValueTable complete source-domain primitive-value reconstruction',block(contracts['BuildTwoValueTable']['original_whole_body']),moved)
    c=contracts['PrepareExistingObjectTableForUdf'];moved=c['current_whole_body']
    layout='Dim layout = Global.BESHStatNG.ExcelCommon.UdfSpillLayout38.ExistingTable(totalRows, totalCols, includeHeader)'
    need(moved.count(layout)==1,'Missing ExistingTable phase')
    moved=moved.replace(layout,'').replace('layout.RowCount - 1','totalRows - 2').replace('layout.ColumnCount - 1','totalCols - 1').replace('layout.SourceStartRow','1')
    same('PrepareExistingObjectTableForUdf complete guarded layout reconstruction',c['original_whole_body'],moved)
    layout_source=text('BESHStat.Excel.Common/src/Results/UdfSpillLayout38.vb')
    need(tokens(block(member(layout_source,'ExistingTable')))==tokens('Return New UdfSpillSlice38(rows, columns, If(includeHeader OrElse rows <= 1, 0, 1))'),'ExistingTable selector differs')
    return dict(status='NINE_COMPLETE_ORIGINAL_SOURCE_RECONSTRUCTIONS_PASS',rows=rows,scope='Exact original/current token compositions plus complete actual CIL/type/field dependencies. Private two-value callers supply primitive Doubles; no global arbitrary-boxed-value equivalence. Host arithmetic, culture, error/catch and logging remain native.',original_queries=0)
if __name__=='__main__':print(json.dumps(inspect()))
