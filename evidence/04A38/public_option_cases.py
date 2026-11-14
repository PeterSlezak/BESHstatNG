"""Source/compiled-edge-derived public option reachability before product edits.

Original private-helper outputs select representative INPUT aliases only. Every
expected public result must be obtained by executing the original public body.
The first valid 24,732-case freeze is preserved, never refreshed or overwritten.
"""
from collections import defaultdict
import csv
import hashlib
import itertools
import json
import re
from preflight import ROOT,HERE,BASE,git,pin,save


def main():
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    assert git('rev-parse','HEAD').decode().strip()==BASE
    out=HERE/'preedit/attempts/public-options-v1';out.mkdir(exist_ok=False)
    census=json.loads((HERE/'entry-members.json').read_bytes())
    compiled=json.loads((HERE/'preedit/compiled-Debug.json').read_bytes())
    functions={r['containing_type']+'.'+r['name']:r for r in census['members'] if r['kind']=='FunctionBlock' and r['name']!='PrepareResultTableForUdf'}
    source_by_id={r['symbol_id']:r for r in census['members']}
    compiled_by_name={r['declaring_type']+'.'+r['name']:r for r in compiled['methods']}
    original=HERE/'preedit/attempts/capture-v10'
    helper_inputs={c['case_id']:c for c in map(json.loads,(original/'cases.jsonl').read_text(encoding='utf-8').splitlines())}
    representatives=defaultdict(dict)
    csv.field_size_limit(1<<28)
    with (original/'capture-Debug-original-v1.tsv').open(encoding='utf-8',newline='') as stream:
        for row in csv.DictReader(stream,delimiter='\t'):
            c=helper_inputs[row['case_id']]
            if c['cohort'] not in ['all_named_option_inputs','original_option_literal_cross_product'] or c['culture']!='':continue
            state=c['input_state']
            if not state.startswith('text:') or state in ['text:invalid-04A38','text:\uff2d\uff2c']:continue
            method=functions.get(c['type']+'.'+c['target'])
            if not method:continue
            output=row['result']
            if 'ByRef ' in method['signature']:
                output+='|'+row['byref_arguments'].partition('|')[2]
            key=(row['outcome'],output,row['error'])
            previous=representatives[c['type']+'.'+c['target']].get(key)
            choice=dict(state=state,original_case_id=c['case_id'],outcome_group=hashlib.sha256(repr(key).encode()).hexdigest())
            if previous is None or (len(state),state)<(len(previous['state']),previous['state']):representatives[c['type']+'.'+c['target']][key]=choice
    cases=[];paths=[]
    raw_arguments={'x','y','data','table','groups','time','status','probabilities','marker','covariates','xCount','xZero','newX','newCountX','newZeroX','newOffset','weights','offset','subject','visit','clusterId','formula','spec','at','handle','includeHeader'}
    def add(c):
        c=dict(c);c['input_factory_version']='v10';c['case_id']=c['target']+'.'+hashlib.sha256(json.dumps(c,sort_keys=True,separators=(',',':')).encode()).hexdigest()[:24]
        cases.append(c)
    for export in census['exports']:
        name=export['effective_excel_name'];member=source_by_id[export['source_symbol_id']]
        compiled_member=compiled_by_name[export['containing_type']+'.'+export['implementing_method']]
        body=member['body_tokens'][len(member['header_tokens']):]
        for parameter in export['arguments']:
            arg=parameter['name']
            if arg in raw_arguments:continue
            calls=[c for c in member['calls'] if re.search(r'\(\s*'+re.escape(arg)+r'\s*(?:,|\))',c['text']) and re.search(r'Parse|Normalize|GetOptionalBool|GetCoxTieMethod|GetRegressionFamilyCode|GetRegressionLinkName|GetFormulaAddressingMode|GetHistogramBinRule',c['expression'])]
            if not calls:continue
            states={};matched_parsers=[]
            for call in calls:
                parser_name=call['expression'].split('.')[-1]
                if '(' in parser_name:continue
                edges=[edge['target'].split('(')[0] for edge in compiled_member['calls'] if edge['target'].split('(')[0].endswith('.'+parser_name)]
                for edge in sorted(set(edges)):
                    matched_parsers.append(edge)
                    for choice in representatives.get(edge,{}).values():states.setdefault(choice['state'],[]).append(choice)
                if parser_name=='GetOptionalBool':states.update({'true':[],'false':[]})
                if 'Alpha' in parser_name or parser_name in ['ParseOptionalNullableDouble','ParseOptionalSeed']:
                    states.update({s:[] for s in ['number:-0','number:0.1','number:1','number:NaN','number:+Inf','number:-Inf']})
                if parser_name.startswith('Normalize'):
                    for part in body.split(' Case ')[1:]:
                        prefix=re.split(r'\b(?:Return|Dim|For|If|[A-Za-z_]\w*\s*=)\b',part,maxsplit=1)[0]
                        for token in re.findall(r'"((?:[^"\n]|"")*)"',prefix):states['text:'+token.replace('""','"')]=[]
            if not states:continue
            states.setdefault('text:invalid-04A38',[])
            paths.append(dict(export=name,argument=arg,source_calls=calls,compiled_parser_edges=sorted(set(matched_parsers)),states=states))
            for culture,(state,origins) in itertools.product(['','sk-SK'],sorted(states.items())):
                arguments={arg:state}
                names={a['name'] for a in export['arguments']}
                # Explicit deterministic synthetic fit inputs. Ordinary defaults
                # remain frozen separately in the original-v1 corpus.
                if 'randomSeed' in names and arg!='randomSeed':arguments['randomSeed']='number:1337'
                if 'bootstrapReplicates' in names and arg!='bootstrapReplicates':arguments['bootstrapReplicates']='number:250'
                if name.startswith('BESH.AGREE.BLANDALTMAN') and 'subjectIds' in names:
                    arguments['subjectIds']='vector:'+','.join(str(i//4+1) for i in range(36))
                if name.endswith(('.PCA_FIT','.FA_FIT')) and arg=='retentionMethod':arguments['retentionValue']='number:0.9' if any(t in state.lower() for t in ['variance','percent','cumul']) else 'number:2'
                add(dict(target_kind='export',target=name,culture=culture,cohort='public_option_consumer_bridge',arguments=arguments,varied_argument=arg,input_state=state,source_parameter=parameter,
                    original_helper_input_provenance=origins,compiled_parser_edges=sorted(set(matched_parsers)),
                    expected_stage='ORIGINAL_PUBLIC_BODY_TO_OPTION_POLICY_AND_SELECTED_OUTPUT; guards/errors counted separately, never successful alternative-output coverage'))
    assert len(cases)==len({c['case_id'] for c in cases})
    with (out/'cases.jsonl').open('x',encoding='utf-8',newline='\n') as stream:
        for c in cases:stream.write(json.dumps(c,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n')
    recorder=HERE/'preedit/frozen-v2/Capture38.cs'
    (out/'Capture38.cs.txt').write_bytes(recorder.read_bytes())
    save('preedit/attempts/public-options-v1/specification.json',dict(status='PRE_EDIT_OPTION_BRIDGE_CANDIDATE_NOT_SEALED',rows=len(cases),parameter_paths=len(paths),paths=paths,
        source_commit=BASE,cases=pin((out/'cases.jsonl').relative_to(ROOT).as_posix()),recorder=pin(recorder.relative_to(ROOT).as_posix()),generator=pin('evidence/04A38/public_option_cases.py'),
        chronology='BEFORE_FIRST_04A38_PRODUCT_EDIT; first valid original-v1 reference preserved unchanged',
        purpose='Private parser aliases alone do not establish public selected-output consumption. This adds independent public path invocations from source and resolved production CIL edges.',
        expected_results='NOT_COPIED_FROM_HELPER_OR_CURRENT_CODE; execute each original public body',native='NOT_RUN',scientific='NOT_GOLDEN',
        remaining_gate='Reachability and positive/negative option branches must be audited; this candidate does not close implementation or authorize a late reference.'))
    print('Prepared',len(cases),'public option bridge cases on',len(paths),'parameter paths; no product edit')


if __name__=='__main__':main()
