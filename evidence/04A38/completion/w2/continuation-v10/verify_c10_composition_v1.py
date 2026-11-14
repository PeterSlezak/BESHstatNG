"""Independent authentic full-parent and complete portable-phase reconstruction."""
import json,re,hashlib,textwrap
from pathlib import Path
from source_views import body
ROOT=Path(__file__).resolve().parents[5];PREFIX=Path(__file__).resolve().parent.relative_to(ROOT).as_posix()+'/'
def need(v,message):
    if not v:raise ValueError(message)
def tokens(text):
    result=[]
    for match in re.finditer(r'"(?:[^"\n]|"")*"|\x27[^\n]*|[^\s]',text):
        t=match[0]
        if not t.startswith("'"):result.append(t if t.startswith('"') else t.lower())
    return ''.join(result)
def phase(text):
    text=text.replace('\r\n','\n');header=re.search(r'\)\s+As [^\n]+\n',text);need(header is not None,'Missing typed phase header');return text[header.end():text.rfind('End Function')].strip()
ARGUMENTS={
'McNemarBody':'tab, alphaValue, AddressOf IsFinite','OddsRatioBody':'tab, alphaValue, AddressOf IsFinite','RiskRatioBody':'tab, alphaValue, AddressOf IsFinite','TrendBody':'tab, rows, cols, AddressOf IsFinite','OrdinalAssociationBody':'tab, alphaValue, AddressOf IsFinite','MantelHaenszelBody':'mat, alphaValue, AddressOf IsFinite','SingleProportionBody':'x, n, alphaValue, AddressOf IsFinite','IndependentProportionsBody':'x1, n1, x2, n2, alphaValue, AddressOf IsFinite','PairedProportionsBody':'n, only1, only2, both, alphaValue, AddressOf IsFinite','ProportionsNonInferiorityBody':'x0, n0, x1, n1, marginValue, alphaValue','ProportionsEquivalenceBody':'x0, n0, x1, n1, lowerValue, upperValue, alphaValue',
'BuildPcaSummary':'h.Model, h.RowIds, h.VariableNames, h.MatrixType, h.RetentionMethod, h.RetentionValue, hdr','BuildPcaEigen':'eigen, pct, cum, h.Model, hdr','BuildFactorSummary':'h.Model, h.VariableNames, h.MatrixType, h.ExtractionMethod, h.RotationMethod, h.ScoreMethod, hdr','BuildFactorEigen':'h.Model, includeHeader, AddressOf Global.BESHStatNG.Matrix.MatrixArithmeticCore.Trace, AddressOf GetOptionalBool'}
def inspect(root=ROOT,overrides=None):
    root=Path(root);overrides=overrides or {};raw=lambda p:overrides[p] if p in overrides else (root/p).read_bytes();load=lambda p:json.loads(raw(p))
    current=load(PREFIX+'current-universe-packet-v2.json');portable=load(PREFIX+'portable-universe-v1.json');previous=load('evidence/04A38/completion/w2/continuation-v9/current-universe-packet-v1.json');entries=load(PREFIX+'packet-A-source-composition-v1.json')['rows']+load(PREFIX+'packet-BC-source-composition-v1.json')['rows'];trans={r['path']:r for r in load(PREFIX+'source-transitions-final-v1.json')['files']};by={n['symbol_id']:n for n in previous['members']};now={n['signature_key']:n for n in current['members'] if n['parent_member'] is None};apis={(n['owner'].split('.')[-1],n['name']):n for n in portable['members']};results=[]
    need(len(entries)==len(ARGUMENTS)==15,'Missing/duplicate source phase')
    for entry in entries:
        old=by[entry['entry_current_id']];n=now[old['signature_key']];before=body(raw(trans[n['path']]['preimage']['path']),old)[0].replace('\r\n','\n');after=body(raw(n['path']),n)[0].replace('\r\n','\n');original=entry['source_phase'];need(before.count(original)==1,'Phase not inside the actual authenticated parent');begin=before.index(original);end=begin+len(original);is_ct=entry['api'] in list(ARGUMENTS)[:11];owner='UdfContingencyPreparation38' if is_ct else 'UdfMultivariateReportPreparation38';api=apis[owner,entry['api']];arguments=ARGUMENTS[entry['api']]
        call='                Dim '+('body' if is_ct else 'out')+' As Object(,) = Global.BESHStatNG.ExcelCommon.'+owner+'.'+entry['api']+'('+arguments+')\n'
        if is_ct and 'Return ExcelError.ExcelErrorNum' in original:call+='                If body Is Nothing Then Return ExcelError.ExcelErrorNum\n'
        call+='\n';need(after==before[:begin]+call+before[end:],'Parent changed outside the exact portable phase or argument order')
        expected=textwrap.dedent(original).rstrip()
        if is_ct:
            expected=expected.replace('Return ExcelError.ExcelErrorNum','Return Nothing').replace('TransposeIntMatrix(tab)','Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TransposeIntMatrix(tab)').replace('SafeCiLabel(','UdfOutputTablesReports38.SafeCiLabel(').replace('SafeCiText(','UdfOutputTablesReports38.SafeCiText(')+'\nReturn body'
        else:
            bindings={'h.Model':'model'}
            if entry['api']=='BuildPcaSummary':bindings.update({'h.MatrixType':'matrixLabel','h.RowIds':'rowIds','h.VariableNames':'variableNames','h.RetentionMethod':'retentionLabel','h.RetentionValue':'retentionValue'})
            if entry['api']=='BuildFactorSummary':bindings.update({'h.VariableNames':'variableNames','h.MatrixType':'matrixLabel','h.ExtractionMethod':'extractionLabel','h.RotationMethod':'rotationLabel','h.ScoreMethod':'scoreLabel'})
            if entry['api']=='BuildFactorEigen':bindings.update({'Global.BESHStatNG.Matrix.MatrixArithmeticCore.Trace(':'matrixTrace(','GetOptionalBool(includeHeader, True)':'readHeader(includeHeader, True)','ColumnSumsOfSquares(':'Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ColumnSumsOfSquares(','PercentOfTotal(':'Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.PercentOfTotal(','Cumulative(':'Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.Cumulative('})
            for a,b in bindings.items():expected=expected.replace(a,b)
            expected+='\nReturn out'
        actual=phase(body(raw(api['path']),api)[0]);need(tokens(actual)==tokens(expected),'Complete portable phase differs under actual source argument bindings '+entry['name']);results.append(dict(original_id=entry['original_id'],entry_parent=old['symbol_id'],current_parent=n['symbol_id'],portable_api=api['symbol_id'],proof='FULL_AUTHENTIC_PARENT_AND_PORTABLE_PHASE_RECONSTRUCTION',source_phase_sha256=hashlib.sha256(original.encode()).hexdigest(),consumer_arguments=arguments))
    return dict(status='FULL_SOURCE_COMPOSITION_PASS_NOT_READY',rows=results,original_queries=0,qualification='Complete source correspondence, not dynamic execution of every branch. Named finite/Trace/header delegates require the independent actual CIL/field/constructor and object-copy timing proof.')
if __name__=='__main__':print(json.dumps(inspect()))
