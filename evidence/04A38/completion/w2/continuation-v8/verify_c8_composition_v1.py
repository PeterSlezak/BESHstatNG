"""Reconstruct complete authentic parents from actual portable/native sources."""
import json,re,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
def need(value,message):
    if not value:raise ValueError(message)
def tokens(value):return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',value) if not x.startswith("'")]
def member(source,name,kind='Function'):
    source=source.replace('\r\n','\n');m=re.search(r'^([ \t]*)(?:Public|Friend|Private)(?: Shared)? '+kind+' '+name+r'\b',source,re.M)
    need(m is not None,'Missing declaration '+name);end=source.index('\n'+m[1]+'End '+kind,m.end())+len('\n'+m[1]+'End '+kind)
    return source[m.start():end]
def block(source,kind='Function'):
    lines=source.replace('\r\n','\n').splitlines();start=next(i for i,l in enumerate(lines) if (') As ' in l if kind=='Function' else l.rstrip().endswith(')')))
    return '\n'.join(lines[start+1:-1])
def inspect(root=ROOT,overrides=None):
    root=Path(root);overrides=overrides or {};prefix=HERE.relative_to(ROOT).as_posix()+'/'
    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()
    def text(path):return raw(path).decode('utf-8-sig').replace('\r\n','\n')
    contracts={c['current_record']['name']:c for c in json.loads(raw(prefix+'whole-body-contracts-v1.json'))['contracts']}
    planning=text('BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb');layout=text('BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb')
    native=text('BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb');imports=text('BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb')
    rows=[]
    def same(name,a,b):
        need(tokens(a)==tokens(b),'Full reconstruction differs '+name)
        rows.append(dict(name=name,tokens=len(tokens(a)),token_sha256=hashlib.sha256(json.dumps(tokens(a)).encode()).hexdigest()))
    for r in json.loads(raw(prefix+'sample-size-extraction-v1.json'))['rows']:
        c=contracts[r['native']];current=c['current_whole_body'];same(r['native']+' genuine Windows consumer',member(native,r['native']),r['current_native'])
        moved=block(member(planning,r['portable']))
        for reader in r['readers']:moved=re.sub(r'\b'+reader['prepared']+r'\(',reader['original']+'(',moved)
        moved=moved.replace('Dim hypothesis As UdfSampleSizeHypothesis38','Dim hypothesis As String').replace('If hypothesis = UdfSampleSizeHypothesis38.Invalid Then','If hypothesis Is Nothing Then')
        for old,new in [('Superiority','HypothesisSuperiority'),('NonInferiority','HypothesisNonInferiority'),('Equivalence','HypothesisEquivalence')]:moved=moved.replace('UdfSampleSizeHypothesis38.'+old,new)
        moved=moved.replace('Return Failed(errorKind, UdfSampleSizeError38.Value)','Return ExcelError.ExcelErrorValue').replace('Return Failed(errorKind, UdfSampleSizeError38.Numeric)','Return ExcelError.ExcelErrorNum').replace('Return SampleSizeUDFsReports38.Make','Return Make')
        same(r['native']+' complete ordered original Try phase',r['original_try_body'],moved)
        begin=current.index('Try\n')+len('Try\n');end=current.index('Catch ex As Exception',begin)
        same(r['native']+' full authentic original parent including header/catch',c['original_whole_body'],current[:begin]+moved+'\n            '+current[end:])
    for r in json.loads(raw(prefix+'import-layout-extraction-v1.json'))['rows']:
        c=contracts[r['native']];current=member(imports,r['native']);same(r['native']+' genuine source after-image',current,r['current'])
        for splice in r['splices']:
            helper=splice['portable'];bindings=splice.get('bindings',{})
            if helper.startswith('UdfNumericImportPreparation38.'):
                moved=block(member(text('BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'),helper.split('.')[-1],'Sub'),'Sub')
                bindings=dict(source='arr',startRow='startRow',rows='expectedRows',column='col')
            else:moved=block(member(layout,helper,'Sub'),'Sub')
            if helper.startswith('Initialize'):
                moved=moved.replace('ReDim raw(rowCount - 1, totalCols - 1)','Dim raw(rowCount - 1, totalCols - 1) As Object').replace('ReDim names(totalCols - 1)','Dim names(totalCols - 1) As String')
            for old,new in bindings.items():moved=re.sub(r'\b'+old+r'\b',lambda m:new,moved)
            same(r['native']+' actual '+helper+' prepared phase',splice['original'],moved)
            need(current.count(splice['replacement'])==1,'Missing actual original-order splice '+r['native']);current=current.replace(splice['replacement'],splice['original'])
        same(r['native']+' complete authentic original parent',c['original_whole_body'],current)
    reports=text('BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb')
    names=['MakeTwoGroupTable','MakeEquivalenceTwoGroupTable','MakeIndependentProportionsTable','MakeEquivalenceIndependentProportionsTable','MakeLogRankPlanningTable','MakeCoxPlanningTable','MakeIccPlanningTable','MakeBlandAltmanPlanningTable']
    for name in names:
        c=contracts[name];moved=block(member(reports,name)).replace('Return New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.Raw)','Return out')
        same(name+' full authentic original report',block(c['original_whole_body']),moved)
    # Keep the exact existing restricted text policy. The new bridge performs
    # the original native Text Select; Common dispatches solely on its enum.
    bridge=member(native,'ResolveSampleSizeHypothesisKind38')
    need('Dim hypothesis As String = ResolveSampleSizeHypothesisType(value)' in bridge,'Changed lazy host option provider')
    for label in ['Superiority','NonInferiority','Equivalence']:
        need('Case Hypothesis'+label in bridge and 'Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.'+label in bridge,'Lost canonical enum branch')
    need('If hypothesis Is Nothing Then Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Invalid' in bridge and 'Case Else' in bridge,'Lost invalid option outcomes')
    return dict(status='COMPLETE_SOURCE_RECONSTRUCTIONS_PASS_PARTIAL_NOT_READY',rows=rows,scope='Full authentic source parents plus real compiled delegates/consumers and all transitive dependencies. Existing LegacyUdfText source-domain restrictions remain; no global Unicode/text equivalence, scientific oracle or native execution is claimed.',original_queries=0)
if __name__=='__main__':print(json.dumps(inspect()))
