"""Independent complete source reconstruction; no original/reference execution."""
from common import *
from source_views import body
from verify_c14_composition_v2 import tokens, phase, unwrap_cases
import re

PROOF=PRIVATE/'draft-proof-v2'
load=lambda path:json.loads(path.read_bytes())
contracts=load(PROOF/'whole-body-contracts-v1.json')['contracts']
portable=load(PROOF/'portable-api-contracts-v1.json')['contracts']
api={(r['source_record']['owner'].split('.')[-1],r['source_record']['name']):r for r in portable}
old=load(PREVIOUS/'current-ownership-v2.json')
prior={r['symbol_id']:r for r in old['members']}
mapping={r['after']:r['before'] for r in load(PROOF/'current-semantic-position-crosswalk-v1.json')['rows']}
preimages={r['path']:r for r in load(HERE/'source-transitions-final-v1.json')['files'] if r['before']}
moves=load(HERE/'implementation-source-moves-v1.json')['moves']
text_move=load(HERE/'implementation-input-source-move-v1.json')
rows=[];move_rows=[]


def report_unwrap(text):
    for value in ['out','outNoHdr']:
        text=text.replace('New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel('+value+'), UdfReportPreparation38.Raw)',value)
    text=text.replace('UdfReportProjection38.FromFault(UdfReportFault38.NotAvailable)','ExcelError.ExcelErrorNA')
    return text.replace('UdfReportProjection38.SafeNumber(','SafeExcelNumber(')


def check_case_choices(original,prepared):
    expected=[]
    for line in original.splitlines():
        if re.match(r'\s*Case\s+(?!Else)',line):expected.extend(re.findall(r'"(?:[^"\n]|"")*"',line.split(':',1)[0]))
    marker=re.search(r'LegacyUdfText38\.CaseToken\(.*?New String\(\)\s*\{([^}]+)\}',prepared)
    assert marker is not None
    actual=re.findall(r'"(?:[^"\n]|"")*"',marker[1])
    assert actual==expected, 'Case-token choice order or contents changed'


legacy={
 'ParseResidualType':('CoxUDFsOptions38','v','Dim s As String = AsString(v)'),
 'ParseGeeCovarianceName':('GEEUDFsOptions38','v','Dim s As String = AsString(v)'),
 'ParseGeeStandardErrorType':('GEEUDFsOptions38','v','Dim s As String = AsString(v)'),
 'ParseGeeResidualType':('GEEUDFsOptions38','v','Dim s As String = AsString(v)'),
 'GetCoxTieMethod':('UdfDataImportOptions38','arg','Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)'),
 'GetRegressionFamilyCode':('UdfDataImportOptions38','arg','Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)'),
 'GetRegressionLinkName':('UdfDataImportOptions38','arg','Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)'),
}

for c in contracts:
    n=c['current_record'];old_n=prior[mapping[c['current_id']]]
    old_raw=(ROOT/preimages[n['path']]['preimage']['path']).read_bytes() if n['path'] in preimages else (ROOT/n['path']).read_bytes()
    entry_text,entry_view=body(old_raw,old_n)
    if not c['new_support']:
        assert n['body_sha256']==old_n['body_sha256']
        mode='EXACT_RETAINED_C14_BODY_WITH_UNCHANGED_PREDECESSOR_READER_AUTHORITY'
    elif tokens(c['original_whole_body'])==tokens(entry_text):
        mode='COMPLETE_ORIGINAL_EQUALS_ENTRY_THEN_EXACT_V15_EXTRACTION_COMPOSITE'
    elif n['name'] in legacy:
        owner,arg,prefix=legacy[n['name']];p=api[owner,n['name']]
        original=phase(c['original_whole_body']);expected=original.replace(prefix,'Dim s As String = '+arg)
        expected=expected.replace('regression.GetCanonicalLinkFromDisplayName','Global.BESHStatNG.regression.GetCanonicalLinkFromDisplayName')
        check_case_choices(original,phase(p['whole_body']))
        assert tokens(expected)==tokens(unwrap_cases(phase(p['whole_body']))), 'Entire option parser differs '+n['name']
        extra=', defaultValue' if n['name']=='GetCoxTieMethod' else ', familyDisplayName' if n['name']=='GetRegressionLinkName' else ''
        expected_native='Return Global.BESHStatNG.ExcelCommon.'+owner+'.'+n['name']+'(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString('+arg+')'+extra+')'
        assert tokens(phase(entry_text))==tokens(expected_native)
        mode='COMPLETE_ORIGINAL_TEXT_CONVERSION_AND_TYPED_CASE_GRAMMAR_WITH_EXACT_LEGACY_TEXT_POLICY'
    elif n['name'] in ['BuildLabeledMatrixOutput','BuildResidualVectorOutput']:
        owner='GEEUDFsReports38' if n['name']=='BuildLabeledMatrixOutput' else 'UdfRegressionOutputReports38'
        p=api[owner,n['name']]
        assert tokens(phase(c['original_whole_body']))==tokens(report_unwrap(phase(p['whole_body'])))
        args='matrix, rowNames, colNames, includeHeader' if n['name']=='BuildLabeledMatrixOutput' else 'vec, header, includeHeader'
        assert tokens(phase(entry_text))==tokens('Return MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.'+owner+'.'+n['name']+'('+args+'))')
        mode='ENTIRE_ORIGINAL_MATRIX_VECTOR_GRAMMAR_PLUS_TYPED_NUMBER_FAULT_PROJECTION_AND_NATIVE_MATERIALIZATION'
    else:
        raise AssertionError('Unreconciled complete original/entry parent '+n['name'])
    reconstructed=entry_text
    used=[]
    for move in moves:
        if move['path']==n['path'] and move['original_segment'] in reconstructed:
            assert reconstructed.count(move['original_segment'])==1
            reconstructed=reconstructed.replace(move['original_segment'],move['native_call'],1)
            used.append(move['name'])
    if n['name']=='TryGetLooseNumericVector':
        assert reconstructed.count(text_move['original_segment'])==1
        reconstructed=reconstructed.replace(text_move['original_segment'],text_move['native_call'],1)
        used.append('TryGetLooseNumericTextPrepared')
    assert tokens(reconstructed)==tokens(c['current_whole_body']), 'Full current parent reconstruction differs '+n['name']
    rows.append(dict(original_id=c['original_id'],current_id=c['current_id'],name=n['name'],mode=mode,
                     moved_segments=used,entry_source_view=entry_view,full_original_entry_current_reconciled=True))

allowed_aliases={
 'BuildCoxSummaryTable':[('h.VarNames','varNames'),('h.Result','result'),('h.Robust','robust'),('ExpForDisplay(','expForDisplay(')],
 'BuildCoxTestsTable':[('h.Result','result'),('ExcelError.ExcelErrorNA','unavailable')],
 'BuildCoxResidualTable':[('h.VarNames','varNames'),('ResidualLabel(rt)','CoxUDFsMetadata38.ResidualLabel(rt)')],
 'AppendCoxBaselineRows':[('kv.Key','key')],
 'BuildGeeTestsPrefix':[('Dim r As Integer = 0','r = 0')],
 'CreateGeePredictionTable':[('Dim r0 As Integer = 0','r0 = 0')],
 'MaterializeGeeRows':[('ExcelError.ExcelErrorNA','unavailable')],
}
for move in moves:
    aliases=[(r['before'],r['after']) for r in move['aliases']]
    assert aliases==allowed_aliases.get(move['name'],[])
    expected=move['original_segment']
    for before,after in aliases:expected=expected.replace(before,after)
    assert tokens(expected)==tokens(move['portable_segment'])
    expected=move['portable_segment']+'\n'+move['portable_suffix']
    if move['name'] in ['CreateCoxBaselineTable','AppendCoxBaselineRows']:
        expected,count=re.subn(r'tp = ("(?:survival|cumhaz|hazard)")',r'LegacyUdfText38.EqualText(tp, \1)',expected)
        assert count==3
    owner='UdfCoxInputPreparation38' if move['destination']=='input' else 'UdfCoxGeeReportPreparation38'
    p=api[owner,move['name']]
    assert tokens(phase(p['whole_body']))==tokens(expected), 'Full moved portable body differs '+move['name']
    move_rows.append(dict(name=move['name'],source=move['path'],portable=p['source_record']['symbol_id'],
        aliases=move['aliases'],text_policy='EXPLICIT_EXISTING_LEGACY_TEXT_COMPARE' if move['name'] in ['CreateCoxBaselineTable','AppendCoxBaselineRows'] else None))
p=api['UdfModelImportPreparation38','TryGetLooseNumericTextPrepared']
assert tokens(phase(p['whole_body']))==tokens(text_move['original_segment'].replace('ChrW(9)','Convert.ToChar(9)'))
move_rows.append(dict(name='TryGetLooseNumericTextPrepared',source=text_move['path'],portable=p['source_record']['symbol_id'],
    aliases=text_move['aliases'],byref='Outer native reset/missing branch remains before typed dispatch; exact inner allocation/finite rejection/failure resets preserved.'))

# Bind complete files as well as parents; unrelated branches or statements cannot hide outside the selected segments.
for path in sorted({r['path'] for r in moves}|{text_move['path']}):
    before=(ROOT/preimages[path]['preimage']['path']).read_bytes().decode('utf-8-sig').replace('\r\n','\n')
    expected=before
    for move in moves:
        if move['path']==path:
            assert expected.count(move['original_segment'])==1
            expected=expected.replace(move['original_segment'],move['native_call'],1)
    if path==text_move['path']:
        assert expected.count(text_move['original_segment'])==1
        expected=expected.replace(text_move['original_segment'],text_move['native_call'],1)
    assert tokens(expected)==tokens((ROOT/path).read_text(encoding='utf-8-sig')), 'Complete source file reconstruction differs '+path

save('source-composition-proof-v1.json',dict(status='COMPLETE_ORIGINAL_ENTRY_AND_CORRECTED_CURRENT_SOURCE_COMPOSITES_PROVED',
    rows=rows,moves=move_rows,whole_native_files=sorted({r['path'] for r in moves}|{text_move['path']}),
    original_queries=0,qualifications=['Native cache/lifecycle/import/error/culture/alpha/pvalue/exception context remains explicitly in each complete body.',
        'New Cox summary delegate is the unchanged actual native ExpForDisplay method; source/CIL caller/return/exception and immutable private-handle getter ownership are separately bound.',
        'Report fault/number projections reuse the unchanged current typed projection and Windows materializer; no scientific engine/default repair.',
        'Cox baseline source/CIL comparison exposed and corrected the real v15 text-policy defect; all first attempts and owner-authorized replacement campaign authority remain.']))
print(json.dumps(dict(complete_parents=len(rows),moved_bodies=len(move_rows),unreconciled=0,original_queries=0)))
