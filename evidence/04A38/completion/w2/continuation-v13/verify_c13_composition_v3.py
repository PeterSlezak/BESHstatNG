"""Independent whole-body composites and exact source boundaries, never an oracle."""
import json,re,sys,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
sys.path.append(str(HERE.parent/'continuation-v11'))
from source_views import body
from verify_composition_v2 import tokens,phase,unwrap
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'

def need(value,message):
    if not value:raise ValueError(message)

def unwrap_cases(text):
    """Remove only the recorded legacy string-selection policy wrapper."""
    marker='LegacyUdfText38.CaseToken('
    while marker in text:
        begin=text.index(marker);start=begin+len(marker);depth=1;quoted=False;comma=None;end=None;i=start
        while i<len(text):
            ch=text[i]
            if ch=='"':
                if quoted and i+1<len(text) and text[i+1]=='"':i+=2;continue
                quoted=not quoted
            elif not quoted:
                if ch=='(':depth+=1
                elif ch==')':
                    depth-=1
                    if depth==0:end=i;break
                elif ch==',' and depth==1 and comma is None:comma=i
            i+=1
        need(end is not None and comma is not None,'Malformed actual CaseToken policy call')
        text=text[:begin]+text[start:comma]+text[end+1:]
    return text

def inspect(root=ROOT,overrides=None):
    root=Path(root);overrides=overrides or {};raw=lambda p:overrides[p] if p in overrides else (root/p).read_bytes();load=lambda p:json.loads(raw(p))
    contracts=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];portable=load(PREFIX+'portable-api-contracts-v1.json')['contracts']
    apis={(p['source_record']['owner'],p['source_record']['name']):p for p in portable}
    current=load(PREFIX+'current-universe-packet-v1.json');before=load('evidence/04A38/completion/w2/continuation-v12/current-ownership-v2.json')
    old={(n['path'],n['name']):n for n in before['members'] if n['parent_member'] is None}
    transitions={r['path']:r for r in load(PREFIX+'source-transitions-final-v1.json')['files']}
    movements=load(PREFIX+'implementation-source-composition-v2.json')['phases'];moved={};rows=[]
    for movement in movements:
        label=movement['parent'];match=re.search(r'Function (\w+)\(',label);name=match[1] if match else label
        native=next(c for c in contracts if c['current_record']['path']==movement['path'] and c['current_record']['name']==name)
        entry=body(raw(transitions[movement['path']]['preimage']['path']),old[movement['path'],name])[0].replace('\r\n','\n')
        actual=body(raw(movement['path']),native['current_record'])[0]
        expected=entry.replace(movement['original_segment'],movement['actual_native_call'],1)
        need(expected!=entry and tokens(actual)==tokens(expected),'Native source changed outside exact prepared movement '+name)
        owner='BESHStatNG.ExcelCommon.'+('UdfPlotDataPreparation38' if movement['owner']=='prepared' else movement['owner'])
        api=movement['portable_signature'].split('(',1)[0];target=apis[owner,api]
        need(tokens(phase(target['whole_body']))==tokens(movement['portable_segment']),'Actual complete portable method body changed '+api)
        need(native['original_whole_body'] is None or tokens(entry)==tokens(native['original_whole_body']),'Entry prepared parent differed from archived original '+name)
        moved[name]=movement
        rows.append(dict(name=name,original_id=native['original_id'],current_id=native['current_id'],portable_id=target['source_record']['symbol_id'],
            mode='EXACT_COMPLETE_NATIVE_PREFIX_PREPARED_BODY_SUFFIX_COMPOSITE',actual_native_call=movement['actual_native_call'],bindings=movement['bindings'],
            original_segment=movement['original_segment'],actual_portable_segment=movement['portable_segment'],supported=native['proof_status']!='OPEN_BYREF_PARITY_DEPENDENCY'))
    for c in contracts:
        if not c['new_support']:continue
        n=c['current_record'];name=n['name']
        if name in moved:continue
        if c['original_whole_body'] is None:
            previous=old[n['path'],name];entry=body(raw(PREFIX+'source-before/'+n['path']+'.txt'),previous)[0]
            need(tokens(entry)==tokens(c['current_whole_body']),'Current-only parent body drift '+name)
            rows.append(dict(name=name,current_id=c['current_id'],mode='COMPLETE_CURRENT_ONLY_HOST_BODY_WITH_INDEPENDENT_ENTRY_SOURCE_AND_CIL'))
        elif tokens(c['current_whole_body'])==tokens(c['original_whole_body']):
            rows.append(dict(name=name,original_id=c['original_id'],current_id=c['current_id'],mode='EXACT_COMPLETE_ORIGINAL_CURRENT_PARENT'))
        elif name=='BuildDiscriminantMeansTable':
            target=apis['BESHStatNG.ExcelCommon.MultivariateAnalysisUDFsReports38',name]
            need(tokens(phase(c['original_whole_body']))==tokens(unwrap(phase(target['whole_body']))),'Original complete means layout not reconstructed')
            expected='Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.MultivariateAnalysisUDFsReports38.BuildDiscriminantMeansTable(model, useOriginalScale)), Object(,))'
            need(tokens(phase(c['current_whole_body']))==tokens(expected),'Wrong native means materializer/arguments')
            rows.append(dict(name=name,original_id=c['original_id'],current_id=c['current_id'],portable_id=target['source_record']['symbol_id'],mode='EXACT_RAW_PROJECTION_AND_NATIVE_MATERIALIZER_COMPOSITE'))
        elif name in ['GetHistogramBinRule','ParseRocDirection']:
            full=load(PREFIX+'dependency-full-common-v1.json')
            target=next(p for p in full['members'] if p['owner']=='BESHStatNG.ExcelCommon.UdfDataImportOptions38' and p['name']==name)
            target_body=body(raw(target['path']),target)[0];original_body=phase(c['original_whole_body'])
            argument='arg' if name=='GetHistogramBinRule' else 'direction'
            expected=original_body.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString('+argument+')',argument)
            need(tokens(expected)==tokens(re.sub(r'LegacyUdfText38.EqualText\(token, ("[^"]+")\)',r'token = \1',unwrap_cases(phase(target_body)))),'Complete legacy option grammar not reconstructed '+name)
            native='Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.'+name+'(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString('+argument+'))'
            need(tokens(phase(c['current_whole_body']))==tokens(native),'Host string coercion/default boundary changed '+name)
            rows.append(dict(name=name,original_id=c['original_id'],current_id=c['current_id'],portable_id=target['symbol_id'],mode='COMPLETE_NATIVE_TEXT_COERCION_AND_LEGACY_COMPARETEXT_POLICY_COMPOSITE',
                qualifications='Actual unchanged LegacyUdfText38 source/CIL implements the original VB text comparison. Type/message/precedence and host catch remain; extra existing Common stack frames are explicit.'))
        else:raise ValueError('Unreconciled whole parent '+name)
    need(len(moved)==8,'Missing production prepared movement')
    blocked={c['current_record']['name'] for c in contracts if c['proof_status']=='OPEN_BYREF_PARITY_DEPENDENCY'}
    need(blocked=={'GuessPositiveClass','TryGetRocInput','ROC_POINTS','ROC_STATS'},'Unresolved dependency silently closed')
    return dict(status='C13_COMPLETE_SUPPORTED_SOURCE_COMPOSITES_WITH_EXPLICIT_OPEN_BYREF_DEPENDENCY',rows=rows,blocked_parents=sorted(blocked),original_queries=0,
        qualification='Static full source/compiled boundary proof; does not assert every dynamic/native arm executed. Ordinary frozen compatibility does not close the recorded ByRef alias difference.')

if __name__=='__main__':print(json.dumps(inspect()))
