from common43 import *
import re,copy,collections
def validate(data):
    assert data['kind']=='CURRENT_MAIN_PRE_EDIT_04A43' and len(data['graphs'])==36
    ops={r['id']:r for r in data['operations']};alltokens=set();mapping={}
    for g in data['graphs']:
        assert g['uniqueGeneratedTokens'] and g['distinctCachedObjects'] and g['queryABindsOwnObject'] and g['queryBBindsOwnObject'] and g['dropOnlyA']
        assert g['workbookScope']==g['sessionEpoch']=='NOT_ESTABLISHED' and g['liveExcel']=='NOT_RUN'
        assert len(g['tokens'])==(3 if g['family']=='PSM' else 2)
        assert len({r['valueOrdinal'] for r in g['tokens']})==len(g['tokens'])
        for t in g['tokens']:
            key=t['token'];assert re.fullmatch(re.escape(g['family'])+r':[0-9a-f]{32}',key) and key not in alltokens;alltokens.add(key)
            assert t['valueOrdinal']==t['boundValueOrdinal'];mapping[key]=g['context']+'/'+t['symbol']
        tokens={t['symbol']:t['token'] for t in g['tokens']}
        for symbol in ['A','B']:
            row=ops[g['context']+'/QUERY_'+symbol];assert row['input']['values'][0]['value']==tokens[symbol] and row['output']['type']=='System.Object[,]'
        first=ops[g['context']+'/QUERY_A']['output'];repeat=ops[g['context']+'/QUERY_A_AFTER_MUTATION']['output'];assert first==repeat,'RESULT_ARRAY_COPY_OR_REPEAT_CHANGED'
        assert ops[g['context']+'/DROP_A']['output']==dict(type='System.Boolean',value='True')
        assert ops[g['context']+'/DROP_A_AGAIN']['output']==dict(type='System.Boolean',value='False')
        assert ops[g['context']+'/QUERY_A_AFTER_DROP']['output']['type'].endswith('ExcelError')
    return mapping
def canonical(data):
    mapping=validate(data)
    def walk(v):
        if isinstance(v,list):return [walk(x) for x in v]
        if isinstance(v,dict):return {k:walk(x) for k,x in v.items()}
        if isinstance(v,str):
            for token,symbol in mapping.items():v=re.sub(re.escape(token),lambda _:symbol,v,flags=re.I)
        return v
    result=walk(data);ops={r['id']:r for r in result['operations']}
    for f in load(HERE/'fixtures/volatile-fields-v1.json')['fields']:
        values=ops[f['operation']]['output']['values'];cell=values[f['value_index']];assert cell['type']==f['value_type']
        if f['kind']=='LABELED_VALUE_CELL':
            assert values[f['label_index']]['value']==f['label'];values[f['value_index']]=dict(type=cell['type'],runtime_only='<'+f['label']+'>')
        else:
            text=cell['value'];assert re.fullmatch(r'Computational time:?\s+[-+0-9.,Ee ]+ seconds\.',text)
            cell['value']=re.sub(r'(Computational time:?\s+)[-+0-9.,Ee ]+( seconds\.)',r'\1<ELAPSED_NUMBER>\2',text)
    return result
def validate_races(d):
    assert len(d['rows'])==864 and len({r['id'] for r in d['rows']})==864
    assert d['parallelFitDistinctTokens'] and d['parallelReadDistinctArrays'] and d['noOutputLeakage'] and d['liveExcel']=='NOT_RUN'
    for r in d['rows']:
        assert r['completedWithinBound'] and r['correctReference'] and -1<=r['c1']<=2 and -1<=r['c2']<=2
        if r['mode']=='GET_DROP':assert r['removed'] and r['remaining']==1
        else:assert r['remaining']==0
    return dict(rows=864,identities=sha(json.dumps(sorted(r['id'] for r in d['rows'])).encode()),observed_outcomes=dict(collections.Counter(str((r['mode'],r['found'],r['removed'],r['c1'],r['c2'])) for r in d['rows'])),same_fixed_schedule_claimed=False)
