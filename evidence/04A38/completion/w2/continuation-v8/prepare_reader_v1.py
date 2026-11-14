"""Version C7 independent reader for the exact C8 source/dependency packet."""
from common import *
inputs=json.loads((HERE/'proof-inputs-v1.json').read_bytes());callbacks=json.loads((HERE/'historical-callback-contracts-v1.json').read_bytes())['rows']
inputs['new_historical_ids']+= [r['original_id'] for r in callbacks]
inputs['historical_callback_ids']=[r['original_id'] for r in callbacks]
inputs['expected_contracts']=49;inputs['expected_callbacks']=10
save('proof-inputs-v2.json',inputs)
source=(PREVIOUS/'verify_checkpoint_template_v5.py').read_text(encoding='utf-8')
def change(before,after):
    global source
    assert before in source,before[:100];source=source.replace(before,after)
change('Independent C7','Independent C8')
change('from verify_composition_v1 import inspect as source_composition','from verify_c8_composition_v2 import inspect as source_composition')
change("C6='evidence/04A38/completion/w2/continuation-v6/'","C6='evidence/04A38/completion/w2/continuation-v7/'")
start=source.index('CHANGED=');end=source.index('\ndef need(',start)
sample=['SSIZE_TTEST_UNPAIRED','SSIZE_PROP_SINGLE','SSIZE_PROP_INDEP','SSIZE_LOGRANK','SSIZE_COX_BINARY','SSIZE_COX_CONTINUOUS','SSIZE_ICC','SSIZE_BLANDALTMAN']
portable=['Unpaired','SingleProportion','IndependentProportions','LogRank','CoxBinary','CoxContinuous','Icc','BlandAltman']
imports=['TryGetAlignedClusterIdColumnObject','TryBuildGeeDataFromUdfArgs','TryBuildGlmDataFromUdfArgs','TryBuildPredictorDataFromUdfArgs','TryBuildCoxDataFromUdfArgs']
changed={('BESHStatNG.WorksheetFunctions.SampleSizeUDFs',n) for n in sample}|{('BESHStatNG.UdfDataImport',n) for n in imports}
targets={('BESHStatNG.WorksheetFunctions.SampleSizeUDFs',n):('BESHStatNG.ExcelCommon.UdfSampleSizePreparation38','Prepare'+p) for n,p in zip(sample,portable)}
targets.update({('BESHStatNG.UdfDataImport',n):('BESHStatNG.ExcelCommon.UdfModelImportPreparation38',p) for n,p in zip(imports[1:],['InitializeResponsePredictorsPrepared','InitializeResponsePredictorsPrepared','InitializePredictorsPrepared','InitializeCoxPrepared'])})
targets[('BESHStatNG.UdfDataImport',imports[0])]=('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38','CopyTrimmedColumnPrepared')
source=source[:start]+'CHANGED='+repr(changed)+'\nNEW_TARGETS='+repr(targets)+'\n'+source[end:]
change("proof-inputs-v1.json","proof-inputs-v2.json")
change("need(len(transitions)==7 and len({r['path'] for r in transitions})==7", "need(len(transitions)==6 and len({r['path'] for r in transitions})==6")
change("for key in ['after','preimage','snapshot']:check(t[key])", "for key in ['after','snapshot']:check(t[key])\n        if t['preimage'] is not None:check(t['preimage'])")
change("need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Forged exact entry preimage')\n        projected[t['path']]=raw(t['preimage']['path'])", "if t['before'] is not None:\n            need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Forged exact entry preimage')\n            projected[t['path']]=raw(t['preimage']['path'])\n        else:need(t.get('entry_state')=='ABSENT' and t['path'] in {r['path'] for r in load(PREFIX+'implementation-plan-v1.json')['new_sources']},'Unapproved new source path')")
change('from verify_checkpoint_v5 import compute as c6_compute','from verify_c7_checkpoint_v1 import compute as c6_compute')
change('dict(total=704,closed=248,OPEN=456)','dict(total=704,closed=274,OPEN=430)')
change("len(current['members'])==724","len(current['members'])==726")
change('len(contracts)==59','len(contracts)==49')
change("(n['name']=='IsBinaryCell38' and c['new_support'])", "(n['name'] in ['ResolveSampleSizeHypothesisKind38','MaterializeSampleSizeResult38'] and c['new_support'])")
change("need(newhistory==set(inputs['new_historical_ids']) and len(newhistory)==26", """historical_callbacks=load(PREFIX+'historical-callback-contracts-v1.json')['rows']
    need(len(historical_callbacks)==1,'Historical callback multiplicity')
    for row in historical_callbacks:
        h=history_by_id[row['original_id']];n=members[row['current_id']];o=originals[h['git_original_id']]
        need(not h['proof'] and n['symbol_id'] in inherited and row['new_historical_support'] and not row['new_current_support'],'Invented callback population change')
        need(row['original_record']==o and row['current_record']==n and row['original_source']==snapshots[h['path']],'Historical callback identity/source mismatch')
        check(row['original_source']);check(row['current_source'])
        for side,node in [('original',o),('current',n)]:
            text,view=body(raw(row[side+'_source']['path']),node)
            need((text,view)==(row[side+'_whole_body'],row[side+'_source_view']),'Changed historical callback source')
            universe=original if side=='original' else current
            need(row[side+'_fragments']==[f for f in universe['fragments'] if f['member_id']==node['symbol_id']],'Historical callback branch/inline omitted')
        need(row['original_whole_body']==row['current_whole_body'],'Historical callback body drift')
        newhistory.add(row['original_id'])
    need(newhistory==set(inputs['new_historical_ids']) and len(newhistory)==24""")
change("==16,'Callback multiplicity differs'", "==10,'Callback multiplicity differs'")
change("C6+'metadata-full/families-v2/current-'", "C6+'metadata-all/packet-v1-context-v3/current-'")
needle="        root_keys=[]\n"
addition="""        for row in historical_callbacks:
            bound=next(r for r in row['configurations'] if r['configuration']==config)
            b,a=bound['original'],bound['current']
            for part in ['parent','target']:
                need(b[part]==original_methods[b[part]['method']] and a[part]==methods[a[part]['method']],'Fabricated historical callback compiled member')
                need(semantic(b[part])==semantic(a[part]),'Historical callback caller/instructions changed')
            need(b['target']['member_name']=='_Lambda$__94-0' and b['target']['method']==a['target']['method'] and bound['actual_method_aliases']==[],'Invented callback alias')
            need(bound['required_field_names']==['$I','$I94-0'],'Wrong callback cache context')
            for side,meta,defs in [(b,before,original_methods),(a,now,methods)]:
                context=next(t for t in meta['types'] if t['type']==side['target']['declaring_type'])
                need(side['context']==context,'Changed complete callback field/type context')
                ctors=[r for r in defs.values() if r['declaring_type']==context['type'] and r['member_name'] in ['.ctor','.cctor']]
                need(side['constructors']==ctors,'Dropped callback constructor')
                fields=[i for i in side['parent']['instructions'] if isinstance(i['operand'],str) and i['operand'].startswith('FIELD:')]
                need(side['fields']==fields,'Dropped real callback cache access')
            need(b['fields']==a['fields'] and [semantic(c) for c in b['constructors']]==[semantic(c) for c in a['constructors']],'Callback cache/initialization drift')
            bf={f['name']:f for f in b['context']['fields']};af={f['name']:f for f in a['context']['fields']}
            need(all(bf[k]==af[k] for k in bound['required_field_names']),'Callback relevant field drift')
        root_keys=[]
"""
change(needle,addition)
change('completion-w2-cont7/packet-v1','completion-w2-cont8/packet-v1')
change('retained_C6_methods=len(prior)-3,changed=3','retained_C7_methods=len(prior)-13,changed=13')
change("s['declaration'].split()[-1]=='AssumptionsUDFs'","s['declaration'].split()[-1]=='SampleSizeUDFs'")
change("'BESHStatNG.WorksheetFunctions.AssumptionsUDFs'", "'BESHStatNG.WorksheetFunctions.SampleSizeUDFs'")
change("need(t['fields']==[],'Mutable stateless module context changed')", "need({f['name'] for f in t['fields']}=={'HypothesisSuperiority','HypothesisNonInferiority','HypothesisEquivalence'} and all(f['is_literal'] and f['is_static'] for f in t['fields']),'Unexpected sample-size module state')")
change("dict(total=724,closed=len(closed),OPEN=724-len(closed))", "dict(total=726,closed=len(closed),OPEN=726-len(closed))")
change("C6+'compatibility-families-v2.json'", "C6+'compatibility-packet-v1.json'")
change("INDEPENDENT_C7_PARTIAL", "INDEPENDENT_C8_PARTIAL")
change('from verify_validation_v1 import inspect as validation','from verify_c8_validation_v1 import inspect as validation')
source=source.replace('C6 inherited','C7 inherited').replace('complete C6 CIL','complete C7 CIL').replace('C6 field','C7 field').replace('Authenticated C7 input','Authenticated C8 input').replace('Assumptions container','Sample-size container')
with (HERE/'verify_c8_checkpoint_template_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('reader-version-crosswalk-v1.json',dict(before=pin(PREVIOUS/'verify_checkpoint_template_v5.py'),after=pin(HERE/'verify_c8_checkpoint_template_v1.py'),inputs=pin(HERE/'proof-inputs-v2.json'),scope='Independently reconstruct all source/branch/inline/CIL dependencies and original/current populations, retain full C7 partial reader replay on exact preimages, explicitly prove queued historical callback without new current callback, and require complete sample-size children/literal field context. Always partial NOT_READY.',original_queries=0))
