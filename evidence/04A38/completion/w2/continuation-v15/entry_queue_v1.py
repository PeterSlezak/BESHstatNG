"""Exact selected historical identities, full source bodies and actual compiled dependencies."""
from common import *
from collections import Counter
from source_views import body
from proof_kernel_v8 import bind, definitions, closure, NESTED_CACHE_DECLARATIONS

assert json.loads((HERE / 'entry-authentication-v2.json').read_bytes())['status'] == 'ENTRY_GATES_PASS'
load = lambda name: json.loads((PREVIOUS / name).read_bytes())
h = load('historical-ownership-v2.json')['members']
c = load('current-ownership-v2.json')
u = load('current-universe-packet-v1.json')
o = json.loads((W2.parent / 'w1/original-universe.json').read_bytes())
q = load('remaining-work-queue-v1.json')
assert len(h) == 704 and sum(bool(r['proof']) for r in h) == 496
assert len(c['members']) == 726 and sum(not r['review_status'].startswith('OPEN') for r in c['members']) == 529
distinct = [r for r in c['support'] if not r['already_in_primary_population']]
assert len(distinct) == 303 and sum(r['fully_reconciled'] for r in distinct) == 256
assert len(q['members']) == 208 and [r['original_id'] for r in q['members']] == [r['canonical_original_id'] for r in h if not r['proof']]
assert len(q['current_OPEN']) == 197 and Counter(r['current_id'] for r in q['current_OPEN']) == Counter(r['symbol_id'] for r in c['members'] if r['review_status'].startswith('OPEN'))
assert len(q['support_OPEN']) == 47 and Counter(r['support_id'] for r in q['support_OPEN']) == Counter(r['support_id'] for r in distinct if not r['fully_reconciled'])

expected = [
 ('COX_FIT','CoxUDFs','FunctionBlock','a535679032a7494a@9076:8281'),
 ('COX_SUMMARY','CoxUDFs','FunctionBlock','c6110fb5d713d2a7@19822:3163'),
 ('COX_TESTS','CoxUDFs','FunctionBlock','ad75f19ae60b35a7@25443:6607'),
 ('COX_RESID','CoxUDFs','FunctionBlock','1655eab4f5f55eb9@34391:2148'),
 ('lambda','CoxUDFs','SingleLineFunctionLambdaExpression','8cbe8cb0546eb818@35559:13'),
 ('COX_BASELINE','CoxUDFs','FunctionBlock','bdac655a3540d7c9@38763:3009'),
 ('COX_PRED','CoxUDFs','FunctionBlock','937fb993c2574869@45248:6931'),
 ('EnsureResiduals','CoxUDFs','FunctionBlock','422299bdff8977f2@54074:742'),
 ('ParseResidualType','CoxUDFs','FunctionBlock','01ddf637104dda0c@54828:606'),
 ('ProfileValues','GEEUDFs.GeeLsmEstimateComponent','PropertyStatement','bbdb4ba41e294a2d@4444:74'),
 ('TimeSpecified','GEEUDFs.GeeLsmEstimateAtProfile','PropertyStatement','87095b051d9953b6@4596:48'),
 ('TimeValue','GEEUDFs.GeeLsmEstimateAtProfile','PropertyStatement','77dd07dc1da288c3@4657:48'),
 ('ProfileValues','GEEUDFs.GeeLsmEstimateAtProfile','PropertyStatement','bbdb4ba41e294a2d@4718:74'),
 ('GEE_FIT','GEEUDFs','FunctionBlock','b0b51f0d3be6a84d@13536:14623'),
 ('GEE_SUMMARY','GEEUDFs','FunctionBlock','4c695208f3f008cb@29729:3348'),
 ('GEE_TESTS','GEEUDFs','FunctionBlock','96f9db514d78b1c7@34706:2376'),
 ('GEE_WCORR','GEEUDFs','FunctionBlock','a577394c9fa4b0ea@38941:1362'),
 ('GEE_VCOV','GEEUDFs','FunctionBlock','b71885b41926c9c5@42615:1904'),
 ('GEE_RESID','GEEUDFs','FunctionBlock','c9a51aa546ee02b1@46209:3650'),
 ('GEE_PRED','GEEUDFs','FunctionBlock','32d25dd8acc9407e@52384:3957'),
 ('GEE_CLASS','GEEUDFs','FunctionBlock','a70d8d43eafb18fc@58710:2155'),
 ('GEE_THRESH','GEEUDFs','FunctionBlock','1d46b154e1747674@62621:2235'),
 ('GEE_CALIB','GEEUDFs','FunctionBlock','1becb826a00fc43d@66710:2448'),
 ('GEE_BRIER','GEEUDFs','FunctionBlock','b8b96b9670571bc6@70478:1845'),
]
ids = ['BESHstatNG/src/ExcelUDFs/Regression/' + ('GeeUDFs' if owner.startswith('GEE') else 'CoxUDFs') + '.vb|BESHStatNG.WorksheetFunctions.' + owner + '|' + kind + '|' + tail for _, owner, kind, tail in expected]
assert q['first_members'] == q['members'][:24]
assert [(r['name'],r['original_id']) for r in q['first_members']] == [(e[0],key) for e,key in zip(expected,ids)]
cm = {r['symbol_id']:r for r in c['members']}
om = {r['symbol_id']:r for r in o['members']}
hm = {r['canonical_original_id']:r for r in h}
snapshots = {r['source']['path']:r['snapshot'] for r in json.loads((ROOT / 'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
shapes = {r['symbol_id']:r for r in load('declaration-shapes-v1.json')['rows']}
metas = {cfg:load('metadata-all/packet-v1-context-v5/current-' + cfg + '.json') for cfg in ['Debug','Release']}
originals = {cfg:load('metadata-all/packet-v1-context-v5/original-' + cfg + '.json') for cfg in metas}
for owner, name in [('CoxUDFs','CoxModelHandle'),('GEEUDFs','GeeHandle')]:
    full = 'BESHStatNG.WorksheetFunctions.' + owner
    support = next(s for s in c['support'] if s['owner'] == full and s['declaration'] == 'Private Class ' + name)
    assert all(any(t['type'] == full + '+' + name for t in m['types']) for m in metas.values())
    NESTED_CACHE_DECLARATIONS[full] = {name}


def binding(n, meta, original=None):
    node = original or n
    if node['parent_member']:
        parent = (om if original else cm)[node['parent_member']]
        p = bind(parent, meta, {**shapes, parent['symbol_id']:shapes[cm[hm[key]['current_ids'][0]]['parent_member']]})
        targets = [i['operand'][7:] for i in p['instructions'] if i['op'] == 'ldftn' and i.get('operand','').startswith('METHOD:')]
        methods = definitions(meta)
        targets = [methods[k] for k in targets if k in methods and methods[k]['member_name'].startswith('_Lambda$__')]
        assert len(targets) == 1, 'Exact Cox callback target ambiguous'
        return targets
    return [bind(node, meta, {**shapes,node['symbol_id']:shapes[n['symbol_id']]})]


rows = []
roots = {cfg:[] for cfg in metas}
oroots = {cfg:[] for cfg in metas}
for ordinal, key in enumerate(ids, 1):
    historical = hm[key]
    archived = om[historical['git_original_id']]
    oldtext, oldview = body((ROOT / snapshots[archived['path']]['path']).read_bytes(), archived)
    candidates = [cm[k] for k in historical['current_ids']]
    compiled = []
    if archived['kind'] == 'PropertyStatement':
        assert not candidates, 'Moved property unexpectedly gained a Windows member ID'
        owner, short = archived['owner'].rsplit('.',1)
        native_owner = owner + '+' + short
        portable_owner = 'BESHStatNG.ExcelCommon.' + ('UdfGeeComponent38' if short == 'GeeLsmEstimateComponent' else 'UdfGeeAtProfile38') + '`1'
        for cfg, meta in metas.items():
            original_methods = [m for m in originals[cfg]['methods'] if m['declaring_type'] == native_owner and m['member_name'] in ['get_' + archived['name'],'set_' + archived['name'],'.ctor']]
            assert len(original_methods) == 3
            inherited_methods = [m for m in meta['methods'] if m['declaring_type'] and m['declaring_type'].startswith(portable_owner) and m['member_name'] in ['get_' + archived['name'],'set_' + archived['name'],'.ctor']]
            native_type = next(t for t in meta['types'] if t['type'] == native_owner)
            assert portable_owner in native_type['base_type'] and inherited_methods
            native_constructors = [m for m in meta['methods'] if m['declaring_type'] == native_owner and m['member_name'] == '.ctor']
            compiled.append(dict(configuration=cfg,original=original_methods,current_inherited=inherited_methods,current_native_constructors=native_constructors,native_type=native_type))
            roots[cfg].extend(m['method'] for m in inherited_methods + native_constructors)
            oroots[cfg].extend(m['method'] for m in original_methods)
        current_info = dict(kind='HISTORICAL_PROPERTY_MOVED_TO_INHERITED_COMMON',native_owner=native_owner,portable_owner=portable_owner,current_ids=[],source=pin(ROOT / 'BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb'))
        fragments = []
    else:
        assert len(candidates) == 1
        n = candidates[0]
        now, view = body((ROOT / n['path']).read_bytes(), n)
        for cfg, meta in metas.items():
            ms = binding(n, meta)
            oldms = binding(n, originals[cfg], archived)
            compiled.append(dict(configuration=cfg,current=ms,original=oldms))
            roots[cfg].extend(m['method'] for m in ms)
            oroots[cfg].extend(m['method'] for m in oldms)
        current_info = dict(kind='CURRENT_WINDOWS_COUNTERPART',record=n,source=pin(ROOT / n['path']),complete_body=now,source_view=view)
        fragments = [f for f in u['fragments'] if f['member_id'] == n['symbol_id']]
    rows.append(dict(ordinal=ordinal,name=archived['name'],historical_id=key,historical_record=historical,original_record=archived,
        original_source=snapshots[archived['path']],complete_original_body=oldtext,original_view=oldview,current=current_info,
        original_fragments=[f for f in o['fragments'] if f['member_id']==archived['symbol_id']],current_fragments=fragments,
        compiled_bindings=compiled,status='OPEN_PRE_IMPLEMENTATION_NO_CLOSURE',
        portable_candidates=('Common typed profile/default/accessor ownership' if archived['kind']=='PropertyStatement' else
            'Existing Core engine/options/import/profile/math helpers plus reusable report matrix/layout preparation'),
        host_boundaries=['Excel-DNA argument interpretation/error values and final materialization','Session cache/handle identity and lifecycle','Workbook/formula address acquisition','Exception/Err/logging context'],
        scientific_boundary='Existing Core numerics and original branch/evaluation/output ordering preserved; no new oracle'))

graphs = {cfg:closure(meta,roots[cfg]) for cfg,meta in metas.items()}
oldgraphs = {cfg:closure(meta,oroots[cfg]) for cfg,meta in originals.items()}
needed = []
for n in cm.values():
    if n['parent_member'] or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:
        continue
    methods = [m for m in metas['Debug']['methods'] if (m['declaring_type'],m['member_name']) == (n['owner'],n['name']) and m['method'] in graphs['Debug']['methods']]
    if methods:
        needed.append(dict(source=n,actual_compiled=methods))
parents = {r['source']['symbol_id'] for r in needed}
children = set()
pending = list(parents)
while pending:
    parent = pending.pop()
    for n in cm.values():
        if n['parent_member'] == parent and n['symbol_id'] not in children:
            children.add(n['symbol_id']); pending.append(n['symbol_id'])
types = {m['declaring_type'].replace('+','.') for m in metas['Debug']['methods'] if m['method'] in graphs['Debug']['methods'] and m['declaring_type']}
support = [s for s in c['support'] if s['owner'] in types or any(t.startswith(s['owner'] + '.') for t in types)]
save('entry-compiled-dependency-catalog-v1.json',dict(graphs=graphs,original_graphs=oldgraphs,
    current_methods={cfg:[m for m in meta['methods'] if m['method'] in graphs[cfg]['methods']] for cfg,meta in metas.items()},
    current_metadata=[pin(PREVIOUS / ('metadata-all/packet-v1-context-v5/current-' + cfg + '.json')) for cfg in metas],
    original_metadata=[pin(PREVIOUS / ('metadata-all/packet-v1-context-v5/original-' + cfg + '.json')) for cfg in metas],original_queries=0))
save('entry-queue-receipt-v1.json',dict(status='AUTHENTICATED_FIRST24_OPEN_PRE_IMPLEMENTATION',rows=rows,
    full_ordered_OPEN_complements=q,required_native_parents=needed,required_callbacks=[cm[k] for k in sorted(children)],
    required_support_context=support,dependencies=pin(HERE / 'entry-compiled-dependency-catalog-v1.json'),
    entry=pin(HERE / 'entry-authentication-v2.json'),queue=pin(PREVIOUS / 'remaining-work-queue-v1.json'),
    production_edits=0,original_queries=0,
    obligations='Every parent/branch/inline/callback and outbound ownership must reconcile before closure; four moved property originals retain exact original owners and have no fabricated current IDs.'))
print(json.dumps(dict(first24=[r['name'] for r in rows],historical_OPEN=208,current_OPEN=197,support_OPEN=47,native_parents=len(needed),callbacks=len(children),reached={cfg:len(g['methods']) for cfg,g in graphs.items()})))
