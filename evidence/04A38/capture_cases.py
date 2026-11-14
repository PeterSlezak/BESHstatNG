"""Named, independent pre-edit cases. Pilot outputs are NOT a final freeze."""
from pathlib import Path
import hashlib
import itertools
import json
import re
from preflight import ROOT,HERE,pin,save

CULTURES=['','sk-SK']
def key(case):return case['target']+'.'+hashlib.sha256(json.dumps(case,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24]
def main():
    entry=json.loads((HERE/'entry-members.json').read_bytes());out=HERE/'preedit/attempts/capture-v1';out.mkdir(exist_ok=False)
    cases=[]
    def add(**c):c['case_id']=key(c);cases.append(c)
    # All 268 actual public bodies, both cultures. Default/ordinary reachability
    # is audited after execution and must not be inferred from case count.
    for culture,e in itertools.product(CULTURES,entry['exports']):
        add(target_kind='export',target=e['effective_excel_name'],culture=culture,cohort='all_exports_ordinary',arguments={},expected_stage='PUBLIC_BODY; reached result/guard/error audited separately')
    primitive_states=['number:-0','number:NaN','number:+Inf','number:-Inf','number:-1','number:0']
    object_states=['null','empty','missing','error','blank','whitespace','true','text:1.25','number:NaN','number:+Inf','number:-Inf','matrix','zero_rows','ragged']
    for culture,e in itertools.product(CULTURES,entry['exports']):
        for arg in e['arguments']:
            if arg['default_value'] is not None:continue
            for state in primitive_states if arg['type']=='As Double' else object_states:
                add(target_kind='export',target=e['effective_excel_name'],culture=culture,cohort='required_argument_states',arguments={arg['name']:state},varied_argument=arg['name'],input_state=state,expected_stage='PUBLIC_BODY; early guards do not cover successful calculations')
    simple_states=['null','empty','missing','error','dbnull','blank','whitespace','true','false','text:1.25','text:1,25','text: ML-_ ','text:\uff2d\uff2c','number:-0','number:0','number:0.5','number:1','number:NaN','number:+Inf','number:-Inf','number:9223372036854775808','long:9007199254740993','decimal:1.2500']
    scalar={
      'BESHStatNG.WorksheetFunctions.ExcelArgPredicates':['IsMissingArg','IsBlankCell'],
      'BESHStatNG.WorksheetFunctions.ExcelArgReaders':['AsString','CellToTrimmedText'],
      'BESHStatNG.WorksheetFunctions.ExcelArgNumeric':['TryGetDouble','TryGetFiniteDouble','TryGetBinary01','TryGetWholeNumber','TryGetFiniteDoubleFlexible','TryGetStatus01Flexible'],
      'BESHStatNG.UdfScalarUtilities':['NormalizeText','NormalizeToken','NormalizeMixedModelOptionToken'],
      'BESHStatNG.UdfDataImport':['TryParseAlpha','GetHistogramBinRule'],
      'BESHStatNG.WorksheetFunctions.UdfCacheHelpers':['TryGetHandleKey']}
    for owner,names in scalar.items():
        for name,culture,state in itertools.product(names,CULTURES,simple_states):
            member=next(r for r in entry['members'] if r['containing_type']==owner and r['name']==name and r['kind']=='FunctionBlock')
            # First non-byref input parameter is authoritative syntax, not a name guess.
            first=re.search(r'\bFunction\s+\w+\s*\(\s*(\w+)\s+As',member['signature'])
            assert first,(owner,name)
            add(target_kind='helper',type=owner,target=name,culture=culture,cohort='scalar_and_native_marker_boundary',arguments={first.group(1):state},input_state=state,expected_stage='DIRECT_ORIGINAL_HELPER_BODY')
    parser_candidates=[r for r in entry['members'] if r['kind']=='FunctionBlock' and not r['exported'] and (r['name'].startswith('Parse') or r['name'].startswith('Normalize')) and re.search(r'\(\s*(?:ByVal\s+)?(?:arg|value|s|text)\s+As\s+(?:Object|String)',r['signature'],re.I)]
    parser_inventory=[]
    for member in parser_candidates:
        if '(Of ' in member['signature']:continue
        first=re.search(r'\(\s*(?:ByVal\s+)?(arg|value|s|text)\s+As\s+(Object|String)',member['signature'],re.I)
        # Every string-literal alias in the original branch, plus deliberately
        # invalid tokens. Output messages are harmless additional negative cases.
        literals=sorted(set(x.replace('""','"') for x in re.findall(r'"((?:[^"\n]|"")*)"',member['body_tokens'])))
        states=['null','blank','whitespace','text:unsupported-04A38']+['text:'+v for v in literals]
        if first.group(2).lower()=='string':states=[s for s in states if s not in ['empty','missing','error']]
        for culture,state in itertools.product(CULTURES,states):
            args={first.group(1):state}
            if 'analysisLabel As String' in member['signature']:args['analysisLabel']='text:MMRM'
            add(target_kind='helper',type=member['containing_type'],target=member['name'],culture=culture,cohort='original_option_literal_cross_product',arguments=args,input_state=state,expected_stage='DIRECT_ORIGINAL_OPTION_BRANCH')
        parser_inventory.append(dict(member_id=member['symbol_id'],type=member['containing_type'],name=member['name'],literal_inputs=literals,cultures=CULTURES))
    assert len(cases)==len({c['case_id'] for c in cases})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as f:
        for c in cases:f.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    (out/'Capture38.cs.txt').write_bytes((HERE/'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v1/specification.json',dict(status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED',rows=len(cases),source_commit='ba1636f5356ac761509c2cfa16fc66b92430935d',cases=pin((out/'cases.jsonl').relative_to(ROOT).as_posix()),
        generator=pin('evidence/04A38/capture_cases.py'),recorder=pin('evidence/04A38/helpers/Capture38.cs'),parsers=parser_inventory,
        dimensions='Named target/culture/argument/state, independently Cartesian; no id-modulo parameter selection.',
        native='Explicit wizard-false test double; native coercion getter/call both blocked and counted. No ExcelReference constructed. Shipping native/registration branches source-only.',
        nondeterminism_policy='Only runtime-created GUID identity entropy is replaced with a stable creation ordinal. Exact prefix, N format/length/case and all referential/cache relationships retained. No other output tolerance or normalization.',
        proof_limit='A returned error or setup exception never counts as a valid calculation. Full matrix/ownership/source/coverage audit must pass before product edits; this pilot alone cannot authorize implementation.'))
    print('Prepared',len(cases),'pre-edit pilot cases; no production changes or freeze claim')

if __name__=='__main__':main()
