"""Complete source bodies, actual transitive CIL, callbacks and original pins."""
from common import *
from source_views import body
from collections import defaultdict
import re
STAGE='packet-v1';META='metadata-all/packet-v1-context-v2/'
u=json.loads((HERE/('current-universe-'+STAGE+'.json')).read_bytes())
prior=json.loads((PREVIOUS/'current-ownership-v1.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v1.json').read_bytes())['members']
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
contexts={c:json.loads((HERE/(META+'current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
originals={c:json.loads((HERE/(META+'original-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
bykey=defaultdict(list);byowner=defaultdict(list)
for n in u['members']:
    bykey[n['signature_key']].append(n)
    if n['parent_member'] is None:byowner[n['owner'],n['name']].append(n)
current_to_history={current:h for h in history for current in h['current_ids']}
oldbykey=defaultdict(list)
for n in prior['members']:oldbykey[n['signature_key']].append(n)
def inherited(n):
    return next((r for r in oldbykey[n['signature_key']] if r['body_token_sha256']==n['body_token_sha256'] and not r['review_status'].startswith('OPEN')),None)
def historical(n):
    old=next(r for r in oldbykey[n['signature_key']] if r['parent_member'] is None)
    return current_to_history.get(old['symbol_id'])
priority=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())['first_members']
assert len(priority)==24
roots=[]
for r in priority:
    h=next(h for h in history if h['canonical_original_id']==r['original_id'])
    mapped={k.split('@')[0] for k in h['current_ids']}
    ns=[n for n in u['members'] if n['signature_key'] in mapped]
    assert len(ns)==1
    roots.append(ns[0])

def semantic(row):return {k:v for k,v in row.items() if k!='raw_IL_sha256'}
def closure(config,keys):
    definitions={r['method']:r for r in contexts[config]['methods']};pending=list(keys);seen=set();external=set();edges=[]
    while pending:
        key=pending.pop()
        if key in seen:continue
        row=definitions.get(key)
        if row is None:external.add(key);continue
        seen.add(key)
        for i,ins in enumerate(row['instructions']):
            if ins['op'] not in ['call','callvirt','newobj','ldftn','ldvirtftn'] or not isinstance(ins['operand'],str) or not ins['operand'].startswith('METHOD:'):continue
            target=ins['operand'][7:];edges.append(dict(caller=key,instruction=i,op=ins['op'],target=target));pending.append(target)
    assert not any('@ BESHStatNG,' in e or '@ BESHStat.Core,' in e or '@ BESHStat.Excel.Common,' in e for e in external),'Missing internal transitive context'
    return dict(methods=sorted(seen),external_runtime_or_ExcelDna=sorted(external),edges=edges)

reviews={
 'TryGetTrimmedColumnObject':'Native col/name resets occur before Get2D. Preserve one-column rejection, bottom blank scan, null headerMode exception after acquisition, binary/text/numeric selector order, full-column reference policy and lazy header Convert.ToString/Trim. Only the final allocation/copy loop is extracted verbatim, with the same ByRef write before first cell access; no numeric or header callback is introduced.',
 'TryGetTrimmedNumericMatrixObject':'Reset mat then inferredNames before acquisition; retain zero-column/all-blank rejection, numeric-header inference, rows threshold, name array allocation and per-column X+current-culture ordinal before lazy nonblank header text. All names finish before output allocation/copy. Extract only the complete row-major copy loop, preserving zero ordinals, cell references and partial assignments on bounds failure.',
 'TryReadNumericColumn':'Entire original body remains exact: allocate the empty List BEFORE acquisition, preserve trimmed-reader side effects, reject zero rows, call native TryGetDouble once per retained row and append NaN for invalid values. This alignment-preserving host conversion loop deliberately differs from finite-only import; no eager conversion, filtering or second reset is added.',
 'TryReadNumericMatrix':'Entire original source/CIL exact: mat/rows/cols reset before first Get2D; retain second Get2D through trim reader, both shape guards and assignments, repeated native header scan before WindowsSpreadsheetTransport.NumericMatrix. Original offset and logging/Excel interpretation remain host-owned; original transport and typed bridge CIL plus all Core methods are exact.',
 'TryGetNumericColumn':'Exact original forwarding body and complete compiled dependency path: preserve opaque input and the caller List ByRef location without allocation, conversion or catch. Its trimmed acquisition, alignment/NaN loop and native converter are separately source/CIL/branch reviewed.',
 'TryGetRequiredOffsetPredictionInputs':'Exact original source and CIL. Reset offset then row count, missing check, numeric-column acquisition and cardinality guards in order. Assign the new array BEFORE HasOnlyFinite; failed finiteness retains that array with nRows zero. Negative finite offsets are intentionally accepted by this helper. No downstream domain correction is added.',
 'PrepareExistingObjectTableForUdf':'Preserve null #N/A, includeHeader early normalization and rows<=1 early normalization before any layout allocation. Existing Common ExistingTable fixes start1 only on the remaining branch; complete row-major copy, out(i-1,j), shape/index failure timing and blank normalization match the original. All ResultTable/Object-array overloads and native p-value dependencies are reviewed, with zero host writes moved.',
 'BuildSimpleNoteTable':'Original two-by-two string-only table is reproduced by the existing raw portable report. Label appears twice, literal Value is retained, Nothing strings remain Nothing under Raw preparation. Full projection/model cloning and native materializer paths are compiled/source reviewed; no ResultTable normalization or p-value formatting is introduced.',
 'SHAPIRO_WILK':'Whole entry source/CIL exact: single-column finite import precedes [3,5000] cardinality, existing ShapiroWilk with errText ByRef, null/statistic/p-value guards and exact title/labels. Entire unchanged Core engine and dependencies are authenticated, including existing scientific limitations. Existing logged catch and two-value formatting remain native.',
 'GRUBBS':'Whole entry source/CIL exact: single import/minimum3 before native alpha parse, unchanged Core Grubbs and both statistic finite guards. Existing report accesses fields in original order and retains whitespace conclusion fallback and numeric alpha. Logged catch and no additional p-value or statistical correction.',
 'ROSNER':'Whole entry source/CIL exact: single import/minimum15 before alpha parsing, unchanged Core Rosner and report caution flag x.Length<25. Preserve absent outlier handling, list order, original spacer/trailing cells and footnote. Existing logged catch maps all exceptions; no science or minimum-size correction.',
 'DAGOSTINO_PEARSON':'Whole original entry source/CIL exact, including original text literals, minimum9, errText ByRef, unchanged DAgostino engine and finite statistic/p-value guards. Native two-value report marks only the original p-value cell; logged catch timing and all Core numerical behavior are retained.',
 'ANDERSON_DARLING':'Whole original entry source/CIL exact: single import and minimum2, unchanged AndersonDarlingTEST, original null/finite guards, exact adjusted-statistic/p-value labels and logged catch. No precision, constant-data or formula correction.',
 'BOX_M':'Original group import and at-least-two guard stay native. Extract the complete g/p dimensions, cube then sample-size allocation, ordered groups/minimum2, unchanged covariance engine and row/column cube copy. Core helper directly invokes the exact target of the unchanged Windows MatCovar facade. BoxM engine call, finite guards, labels and real logged catch remain in their original order.',
 'FLIGNER_KILLEEN':'Whole original source/CIL exact: grouped numeric preparation, minimum two retained groups, unchanged FlignerKilleenTEST, original finite guards, two-sided label and logged catch. Shared import excludes the same cells and preserves column order; no hidden normalization.',
 'LEVENE':'Whole original entry source/CIL exact. Group import/cardinality precedes the already accepted callback-based Levene option parser, which retains mean/median aliases, defaults, ByRef writes and native AsString timing. Original LeveneTest engine, guards, labels and logged catch are unchanged; all option callback/field context is explicit.',
 'BARTLETT':'Whole original source/CIL exact: grouped import and minimum two groups, ordered per-group Nothing/minimum2 guards BEFORE the existing BartlettTEST engine. Preserve finite result checks, p-value label, native output settings and logged catch without adding common normalization.',
 'SQUARED_RANKS':'Whole original source/CIL exact: grouped import/minimum two, unchanged SquaredRanksTestVARIANCE, finite statistic/p-value guards and original report/catch. All Core transitive methods and field/closure contexts are compared, not merely the top-level call.',
 'MAUCHLY':'Whole original source/CIL exact: header-aware complete-case matrix import, minimum2 rows/3 columns, unchanged MauchlyTest, original finite guards, sphericity title/p-value and logged catch. Source-row filtering and names remain in the accepted import chain.',
 'SYMMETRY':'Whole original entry source/CIL exact: single import/minimum2 precedes native AsString and existing symmetry option parsing, then unchanged SymmetryTest, finite guards and selected title. Retain default MGG and CM aliases, invalid method #VALUE, native conversion/catch timing and report settings.',
 'BuildTwoValueTable':'Private source-domain proof: all real callers supply primitive Double statistic/p-value cells from unchanged TestResult fields. Existing TwoValueTable constructs the exact title/labels and p-value address; source ordinal replacement restores both raw values before native blank/p-value formatting. Full ResultTable/model/projection/materializer contexts are included. No arbitrary boxed custom-struct or global equivalence is asserted.',
 'BuildGrubbsTable':'Existing portable builder reproduces the complete original ResultTable initializer: Alpha, Critical statistic, Observed statistic, Result in order; whitespace special information becomes empty. Original header, NormalizeBlanks boundary, shape and native materializer/catch dependencies are complete; all field accesses remain explicit.',
 'BuildRosnerTable':'Existing portable builder contains the full original nOut/rows/body construction, ordered outlier values, zero-outlier blank cells, trailing spacer shape, title and conditional caution footnote. The same unchanged ResultTable engine produces its model before native normalization; no outlier sorting or scientific correction.',
 'SSIZE_TTEST_PAIRED':'Whole original entry source/CIL exact. Convert difference, sd, alpha, beta in that order before collective missing guard; then original nonzero/positive/open-unit checks, unchanged CalculatePairedTTest and integer NumberOfPairs report. Preserve all engine iterations, rounding, Core result type fields, defaults and original logged catch.',
 'MakeMetricValueTable':'Complete supporting historical identity from the machine queue, required by paired sizing. Existing portable report preserves metric/Value header, metric label and Int32 count, Raw preparation and shape. Full original/current body plus ResultTableOutputModel/projection/native materializer dependencies are authenticated.',
 'HasTextColumnHeaderForWholeColumnReference':'Complete required import dependency. Whole original source/CIL exact: one-column/lastRow guards precede originalArg type test, then ExcelReference row extent arithmetic inside the original catch. Preserve first-row zero and 1048576/1048575 thresholds, ordered blank checks and catch False. No native acquisition or live Excel execution is claimed.',
 'IsBinaryCell38':'Current-only support identity, never assigned an invented historical ID. Full body declares a local Int32 and forwards the exact boxed cell to the accepted native TryGetBinary01. Its sole original-parent relationship is the already reviewed HasBinaryColumnHeader callback; full source, ldftn caller, scalar-conversion CIL and field context are included.'}

all_graphs={};reviewed=set();compiled_by_source={}
for config in ['Debug','Release']:
    defs={r['method']:r for r in contexts[config]['methods']}
    starts=[r['method'] for r in defs.values() if any((r['declaring_type'],r['member_name'])==(n['owner'],n['name']) for n in roots)]
    graph=closure(config,starts);all_graphs[config]=graph
    for key in graph['methods']:
        r=defs[key]
        for n in byowner.get((r['declaring_type'],r['member_name']),[]):reviewed.add(n['symbol_id'])
for n in u['members']:
    if n['parent_member'] in reviewed:reviewed.add(n['symbol_id'])

contracts=[];om={n['symbol_id']:n for n in original['members']}
for n in u['members']:
    if n['symbol_id'] not in reviewed:continue
    if n['parent_member'] is not None:continue # Explicit compiled callback proof below.
    h=historical(n);new=not inherited(n)
    if new:assert n['name'] in reviews,n['name']
    if h:
        old=om[h['git_original_id']];oldsource=snapshots[h['path']]
        oldtext,oldview=body((ROOT/oldsource['path']).read_bytes(),old)
    else:old=None;oldsource=None;oldtext=None;oldview=None
    text,view=body((ROOT/n['path']).read_bytes(),n)
    compiled=[]
    for config in ['Debug','Release']:
        defs={r['method']:r for r in contexts[config]['methods']}
        candidates=[r['method'] for r in defs.values() if (r['declaring_type'],r['member_name'])==(n['owner'],n['name'])]
        assert candidates
        graph=closure(config,candidates)
        source_dependencies=sorted({v['symbol_id'] for key in graph['methods'] for v in byowner.get((defs[key]['declaring_type'],defs[key]['member_name']),[])})
        compiled.append(dict(configuration=config,roots=candidates,transitive=graph,source_dependencies=source_dependencies,metadata=pin(HERE/(META+'current-'+config+'.json')),original_metadata=pin(HERE/(META+'original-'+config+'.json'))))
    contracts.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],current_record=n,original_record=old,current_source=pin(ROOT/n['path']),original_source=oldsource,current_whole_body=text,original_whole_body=oldtext,current_source_view=view,original_source_view=oldview,new_support=new,carried_proof=h.get('proof') if h else None,review=reviews.get(n['name'],'Carried C6 source body is exact under its signature and full token identity. The complete current/original source and all real transitive CIL/field/callback dependencies are rechecked here, and the authenticated C6 reader is replayed against exact retained entry preimages. This entry does not close a body solely from a portable call.'),compiled=compiled,current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],original_fragments=[f for f in original['fragments'] if h and f['member_id']==h['git_original_id']],focused_Debug='focused-v2-packet-v1-Debug.json',Release_unit_tests='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
callbacks=[]
for parent in contracts:
    siblings=sorted([n for n in u['members'] if n['parent_member']==parent['current_id']],key=lambda r:r['span_start'])
    for index,n in enumerate(siblings):
        text,view=body((ROOT/n['path']).read_bytes(),n);bindings=[]
        for config in ['Debug','Release']:
            defs={r['method']:r for r in contexts[config]['methods']};pc=next(r for r in parent['compiled'] if r['configuration']==config)
            assert len(pc['roots'])==1
            targets=[i['operand'][7:] for i in defs[pc['roots'][0]]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs and defs[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
            assert len(targets)==len(siblings),(parent['current_record']['name'],len(targets),len(siblings))
            target=defs[targets[index]];context=next(t for t in contexts[config]['types'] if t['type']==target['declaring_type'])
            bindings.append(dict(configuration=config,parent_method=pc['roots'][0],target=target['method'],complete_method=target,declaring_type=context))
        callbacks.append(dict(current_id=n['symbol_id'],parent_id=parent['current_id'],current_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=view,configurations=bindings,new_current=False))
save('whole-body-contracts-v1.json',dict(status='COMPLETE_SOURCE_TRANSITIVE_CIL_BRANCH_INLINE_CONTRACTS_PARTIAL_NOT_READY',contracts=contracts,original_queries=0))
save('callback-proofs-v1.json',dict(rows=callbacks,new_current_callbacks=0,scope='Every anonymous source child of every reviewed parent is matched by ordinal to a real ldftn and complete method/exception/field context in both configurations. No compiler alias is introduced.'))
save('transitive-compiled-context-v1.json',dict(configurations=all_graphs,original_queries=0))
plan=json.loads((HERE/'implementation-plan-v2.json').read_bytes());transitions=[]
for row in plan['source_before']:
    path=row['live']['path'];after=pin(ROOT/path)
    if after==row['live']:continue
    target=HERE/'source-after/final-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write((ROOT/path).read_bytes())
    transitions.append(dict(path=path,before=row['live'],preimage=row['preimage'],after=after,snapshot=pin(target)))
save('source-transitions-final-v1.json',dict(files=transitions,original_queries=0,engines='NO_EXISTING_ENGINE_CHANGE',public_exports='268_UNCHANGED'))
save('proof-inputs-v1.json',dict(priority=priority,root_current_ids=[n['symbol_id'] for n in roots],metadata_prefix=META,reviewed_current=[n['symbol_id'] for n in u['members'] if n['symbol_id'] in reviewed],current_universe='current-universe-packet-v1.json',new_historical_ids=[c['original_id'] for c in contracts if c['new_support'] and c['original_id']],new_current_only=[c['current_id'] for c in contracts if c['new_support'] and not c['original_id']],support_candidate='AssumptionsUDFs',source_domain='All C6 LegacyUdfText38 policy/call-site sources unchanged except byte-preserving shifts; no new text call, no normalization or policy change. Eight out-of-domain counterexamples remain immutable.',original_queries=0))
print(json.dumps(dict(contracts=len(contracts),callbacks=len(callbacks),new_historical=sum(c['new_support'] and c['original_id'] is not None for c in contracts),new_current_only=sum(c['new_support'] and c['original_id'] is None for c in contracts),source_transitions=len(transitions))))
