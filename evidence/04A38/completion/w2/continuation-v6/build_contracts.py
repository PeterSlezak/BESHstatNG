"""Reviewed whole-body contracts with explicit source, CIL and dependency inputs."""
from common import *
from source_views import body
import re
u=json.loads((HERE/'current-universe-families-v2.json').read_bytes())
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v3.json').read_bytes())['members']
inputs=json.loads((HERE/'authenticated-review-inputs.json').read_bytes())['members']
contexts={c:json.loads((HERE/('metadata-full/families-v2/current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
original_context={c:json.loads((HERE/('metadata-full/families-v2/original-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
reviews={
'TryReadIndependentNumericColumns':'Reset outputs before x then y acquisition. Reject null/shapes before both header callbacks, which run x then y. Filter entire x then entire y independently; retain unequal/empty groups. Assign both group arrays before lazy header text x then y, and assign names only after the initializer completes. Current-culture header Convert/Trim stays native. No second output reset; indexed callbacks retain the original numeric Object-copy domain.',
'TryGetIndependentNumericColumns':'Exact facade forwards both opaque arguments and the same ByRef output locations to the fully reviewed independent reader, without an added conversion, catch, reset or allocation.',
'TryReadPairedNumericColumns':'Reset outputs before physical-reference alignment; reject incompatible sheet/row extents before acquiring x then y. Reject null, non-column, unequal-length and asymmetric-header inputs in original order. Assign names before complete-pair extraction; zero pairs returns True with matrix Nothing. Existing Core RowsToMatrix consumes only the established two-element pair domain and preserves row/column order.',
'TryGetPairedNumericColumns':'Exact facade delegates physical alignment, acquisition, first-error/header/name and complete-case behavior with the caller ByRefs unchanged. No independent compaction or zipping.',
'ExtractCompletePairedNumericRows':'Already delegated correctly to WindowsSpreadsheetTransport.ExtractNumericPairs. Full transport/adapter source and 533 engine/adapter plus existing typed Common CIL contexts reviewed: copy lower bounds to zero ordinals, convert both original cells per row before shared ParsePaired, skip only whole incomplete pairs, preserve validation exceptions and list order. No new statistic or row identity.',
'TryReadCompleteNumericMatrixWithHeaders':'Original mat/names resets precede acquisition and dimension rejection. Preserve all-column header detection, original zero-based indexing after native normalization, lazy current-culture names before numeric filtering, row-major first-invalid short circuit and all-column complete cases. A no-reset prepared phase restores original acquisition-reentry behavior; existing public Common wrapper retains its original entry resets.',
'TryGetCompleteNumericMatrixWithHeaders':'Exact ByRef facade consumes the complete-matrix reader with no extra guards, conversion, exception context or result normalization.',
'TryGetSingleNumericColumn':'Reset values and empty detectedName before acquisition; reject non-column first. Detect header and convert its label before allocating/filtering values. Original finite-only row order is shared in ReadFiniteColumn, default List capacity retained; final values assignment follows all numeric callbacks. Native Get2D preserves source array bounds/ownership.',
'TryGetOptionalProbabilityThresholds':'Set thresholds Nothing before missing/scalar conversion. Missing succeeds; scalar finite conversion precedes probability-domain validation and clamping. Only after scalar failure acquire a rectangular vector, reject non-vector before callbacks, scan row or column in original order, stop at first bad cell and preserve reentrant output writes. Empty list succeeds without assignment. Sort and compare adjacent sorted INPUT values at strict1e-12 tolerance, not the last retained value; then assign a fresh array.',
'TryAppendProbabilityThreshold':'Host blank classification precedes finite conversion, which precedes existing Core [0,1] validity and clamping. On failure do not append or mutate the list; empty/DBNull/ExcelEmpty/error/string/Boolean eligibility remains native. CIL and original/native catches are unchanged.',
'TryGetOptionalThresholdVector':'Exact facade to optional threshold import preserves its caller output alias and all first-error, missing and scalar-versus-array behavior.',
'TryGetSingleThresholdFromArg':'Write supplied default before vector import. Failed import or multiple thresholds returns False without assigning threshold again; omitted/empty succeeds with current output state, a single threshold assigns element0. Custom converter mutations during import are not erased.',
'RMANOVA1':'Complete header/matrix import and dimension checks before names. Construct and compute unchanged OneWayRmANOVA BEFORE NormalizeText(correction), including converter side effects/exceptions. Apply original none/GG/HF/both aliases and correction order in Common, reject invalid after compute, wrap once/table0, native p-value preparation, exact real Catch/LoggedUdfError fallback. All engine fields/methods and native catch context unchanged.',
'ANOVA2_NESTED':'Review full three-column import including response-led header detection and case-insensitive grouping domain, then minimum3 rows and names. Construct/compute/wrap unchanged TwoWayNestedANOVA BEFORE selector conversion. Keep blank/both/all, main/classical, and Satterthwaite aliases; render table0 before table1 when stacking, same separator and error. No numerical or GUI/UDF correction.',
'RMANOVA1_MCP':'Complete matrix/names/alpha import in original order; normalize selector BEFORE model creation. Existing engine compute runs before invalid selector rejection. Preserve RM2/Tukey aliases, alpha and ALL call order RM2 then Tukey; use table1 for a single result, otherwise sequential native preparation/stack and final normalization. New stackAll is only a local transport flag; native real catch/logging remains original.',
'TTEST_UNPAIRED':'Independent import with unequal lengths, original null/two-group/minimum2 checks, then names and alpha. Existing UnpairedTtest computes and wraps BOTH models BEFORE selector conversion/rejection. Preserve pooled/Welch/both aliases, two output orders, host p-value/settings behavior and logged catch. Whole original/current engine and result context exact.',
'TTEST_PAIRED':'Already appropriately consumes Core PairedTtest: physical/header/complete-pair import then minimum2, names, constructor/compute/wrap/table0 and original native catch. No production change needed; full original/current body CIL is exact and all import/report children are independently reviewed.',
'TTEST_UNPAIRED_NI':'Independent data/minimum checks precede finite positive margin, alpha, variance conversion and explicit/detected names in original order. Existing Core non-inferiority fit remains native-called and exact. Common copies the entire22-row body without changing scalar kinds, CI helper calls, row labels or order; native BuildResultTable marks p-values/settings and catches original domain errors.',
'TTEST_UNPAIRED_EQUIV':'Independent data/minimum checks precede both margin conversions and ordering, alpha, variance mode and names. Keep fixed/default alpha semantics from TryParseAlpha, including existing missing-alpha behavior. Existing Core equivalence fit and its observed-difference-inside-margin exception remain exact. Common copies all25 body rows and CI helpers; native output/p-value/error handling unchanged.',
'MW_P_NORM':'Original first-sample error wins before second acquisition; each sample ignores nonnumeric independently and requires1. Existing Core MannWhitney constructor/Compute is already portable. Preserve normal p-value finite/[0,1] guard and direct #NUM mapping without adding a catch; exceptions propagate as before. No unnecessary production modification.',
'MW_P_EXACT':'Preserve independent acquisition/errors, minimum1, combined n<=50 BEFORE construction, original exact-availability test BEFORE side normalization, all side aliases and existing ClampProb policy. Core engine already portable and exact; invalid-side #VALUE and unavailable-exact #NUM remain distinct, with no new catch.',
'WILCOX_P_NORM':'Physical reference and source-row alignment are delegated through reviewed paired import; minimum1 precedes unchanged Core Wilcoxon computation. Return its normal p-value through the same ClampProb host policy; preserve original no-catch exception boundary and whole-array allocation behavior.',
'WILCOX_P_EXACT':'Aligned complete pairs/minimum1 precede unchanged Core computation; exact availability rejection occurs before side normalization. Preserve two/lower/upper aliases, exact component choice, ClampProb and distinct #NUM/#VALUE errors, with no new catch.',
'SPEARMAN_RHO':'Paired import/error/minimum3 and alpha parsing remain native and ordered. Shared split copies columns row-major into distinct Double arrays exactly; then the same Core SpearmanRho constructor and Compute(Nothing,alpha), returning correlCoef. No catch added and no caller-array mutation.',
'SPEARMAN_P':'Same complete pair/minimum3/alpha/split/unchanged Core Spearman path as coefficient sibling, but return its original pvalue directly, without introducing ClampProb or finite checks. Complete sibling body independently inspected.',
'KENDALL_TAU':'Paired import/minimum3 and alpha retain order; shared split preserves both arrays and row order. Internal Core KendallsTau stays in its existing Windows friend domain, with the same compute(Nothing,alpha) and correlCoef; no access or public API changes.',
'KENDALL_P':'Preserve the original minimum FOUR paired rows, despite coefficient sibling allowing3; do not fix this statistical/UI discrepancy. Same alpha, split and internal Core compute; return original pvalue with no extra validation or catch. New test explicitly checks the3-row #NUM boundary.',
'KW_STAT':'Permissive grouped import ignores nonnumeric cells per column and rejects fewer than2 retained groups. Shared KruskalGroupNames preserves original G+current-culture integer formatting BEFORE ParseKwType, then unchanged Core compute and H versus Hcor selection; no new catch.',
'KW_P':'Same group/name/selector ordering as statistic sibling, then original Core compute and selected Pvalue/Pvalue2 through existing ClampProb. Preserve selector ExcelError propagation and no catch; independently reviewed full p-value body.',
'FRIEDMAN_STAT':'Original complete-row import has no header-removal policy; require >=2 blocks and conditions. Shared FriedmanGroupNames retains interpolation formatting, compute unchanged Core BEFORE selector conversion, choose TestStatistics2 only for type2 else1. Real native catch maps exceptions to #VALUE.',
'FRIEDMAN_P':'Keep complete-row dimensions/names/compute-before-selector order. Type2 returns Pvalue2, other type Pvalue; reject only nonfinite as original, without adding [0,1] clamping. Original native catch remains; no inference correction.',
'KW_MCP':'Header-aware grouped import and group-count/names/alpha order unchanged. Common orchestrates the same KruskallWalis compute/MCP/wrap once, then native selects table1 and prepares settings/p-values. Original unlogged catch stays native and exact.',
'FRIEDMAN_MCP':'Header-aware complete matrix/minimum/names/alpha then NormalizeText BEFORE Core compute/MCP/wrap. Common contains unchanged engine orchestration. Native preserves Dunn table3, Conover table2 and ALL rendering2 then3 with one separator/final normalization; same invalid selector and unlogged catch.',
'ParseKwType':'Reset err before ordered Nothing/missing/empty and raw ExcelError handling. Real native Convert.ToString catch still maps failed conversion to Nothing; Common typed TryKwType preserves whitespace/default, all H/Hcor aliases and invalid result0/#VALUE timing. No global comparison-policy change.',
'ParseFriedmanType':'Original optional defaultToT1 and ordered missing checks stay native. Convert/Trim/ToUpperInvariant remains outside a local catch; the Common typed alias/default decision is unchanged. Preserve parent-level exception behavior and default1/2 semantics.',
'TryGetEquivalenceMargins':'Write both outputs NaN before lower finite conversion. Missing upper uses abs(lower), rejects zero, writes negative/positive limits in order. Explicit upper is converted only after lower succeeds; no ordering validation inside helper. Parent still handles ordering and original Core domain exceptions.',
'TryGetCompleteNumericMatrixCompleteCases':'Exact facade preserves all three ByRefs and delegates complete-row preparation, with no header policy or own reset/catch.',
'ExtractCompleteNumericMatrixCompleteCases':'Original reset order mat, rows, columns before acquisition, then reject fewer than2 columns. Prepared phase retains row-major first-invalid short circuit, original full column count even when rows<2, and early True with no matrix assignment below2 rows. Public Common API retains resets; native no-second-reset restores acquisition reentry.',
'GetNumericGroupsFromColumnsIgnoringNonNumeric':'Exact error-ByRef facade to the reviewed permissive group reader. No independent header detection, group reordering or copying.',
'ExtractNumericGroupsFromColumnsIgnoringNonNumeric':'Clear err before permissive coercion. Preserve ExcelError/empty/scalar classification and return failures before extraction. Reuse canonical grouped collector with start0/headerFalse; skip empty groups, preserve source-column ordering and default names, map extraction failure to #VALUE then return only groups.',
'CoerceToObjectMatrix':'Local mat initialized Nothing; forward allowScalarTrue and the caller err ByRef into the full host coercer. Return the same acquired matrix on success or Nothing on failure; no clone or extra normalization.',
'TryCoerceToObjectMatrix':'Correct host ownership: reset mat then err, map Nothing/missing/empty to1x1 ExcelEmpty, propagate raw ExcelError without acquisition, try native Get2D, return acquired array by reference, otherwise reject if scalar forbidden or construct original1x1 scalar cell. These operations interpret/marshal Excel-DNA types; reusable numeric policies are delegated elsewhere.',
'GetPairedNumericColumnsIgnoringNonNumeric':'Exact facade preserves both original opaque arguments and same nullable ExcelError output alias; all alignment/conversion/output behavior is in the separately reviewed callee.',
'ExtractPairedNumericColumnsIgnoringNonNumeric':'Clear err, validate physical alignment before coercing x then y, preserve first error and original shape checks. Existing complete-pair transport preserves rows; zero pairs returns Nothing. Use canonical Core RowsToMatrix on its guaranteed two-column list, with no alias, reorder or duplicate statistic.',
'GetNumericColumnIgnoringNonNumeric':'Exact facade to permissive numeric collector with unchanged err alias, empty-array result and exception propagation.',
'ExtractNumericColumnIgnoringNonNumeric':'Clear err before permissive coercion; propagate ExcelError/null and reject non-column before scanning. Shared finite collector uses original List(rows) capacity, source-row order, zero start and native TryGetDouble domain (already finite), so extra intrinsic finite predicates are unreachable rejections and add no callbacks. Same fresh output array; no input mutation.',
'BuildNamedMatrixOutput':'Original null matrix #N/A, dimensions, lazy includeHeader labels and invariant fallback ordinals, row-name fallback and Double cells are copied exactly in the established Common builder. Materializer consumes detached intent, native errors/p-value settings and blank normalization; all Common deep-copy/scalar validation and actual compiled edges reviewed, not just the builder call.',
'BuildCaseMatrixOutput':'Original null matrix OR rowIds #N/A and short-circuit order retained. Preserve native case IDs as Int32 rather than fabricated ordinals, short rowIds exception timing, column fallback labels, includeHeader coordinates, Double bits and blank normalization. Complete Common projection/model/materializer chain reviewed.',
'PrepareWrappedResultTableForUdf':'Null #N/A first; rows<=1 returns ordinary normalization directly. Existing Common WrappedTable computes includeHeader start1/2 and clamps at last source row. Host copies original zero-based row/column order and normalizes once, preserving small/zero-column and nonzero-bound failure behavior. Entire layout/slice constructors and CIL are reviewed.',
'MaterializePortableReport38':'Complete current-only adapter: null/fault checks before model access; exact detached ToOutputModel/array bounds, source reference replacement before typed cell checks, nullable numeric/error intent and exact ExcelError mappings. Preserve missing/out-of-range sourceValues exceptions and unknown fault/kind behavior. NormalizeBlanks then ordered p-value cells/settings, or PvalueRows, otherwise raw; no native cell is retained in Common. New current identity is independently supported, without inventing a historical ID.'}
def method_rows(data,owner,name):return [r for r in data['methods'] if r['declaring_type']==owner and r['member_name']==name]
contracts=[]
for row in inputs:
 h=row['historical'];n=row['current_record'];name=n['name'];new=not h or not h['proof']
 if new:assert name in reviews,name
 compiled=[]
 for config in ['Debug','Release']:
  candidates=method_rows(contexts[config],n['owner'],name)
  assert candidates,(name,n['owner'])
  olds=method_rows(original_context[config],n['owner'],name) if h else []
  compiled.append(dict(configuration=config,current_methods=[r['method'] for r in candidates],original_methods=[r['method'] for r in olds],
      current_metadata=pin(HERE/('metadata-full/families-v2/current-'+config+'.json')),original_metadata=pin(HERE/('metadata-full/families-v2/original-'+config+'.json'))))
 fragments=[r for r in u['fragments'] if r['member_id']==n['symbol_id']]
 oldfrags=[r for r in original['fragments'] if h and r['member_id']==h['git_original_id']]
 contracts.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],signature_key=n['signature_key'],name=name,path=n['path'],
     new_support=new,original_record=row['original_record'],current_record=n,original_source=row['original_source'],current_source=row['current_source'],
     original_whole_body=row['original_body'],current_whole_body=row['current_body'],original_source_view=row['original_view'],current_source_view=row['current_view'],
     review=reviews.get(name,'Complete carried contract rechecked against authenticated source, unchanged full CIL/type context and established v5 proof; no child closed solely from this description.'),
     carried_proof=h['proof'] if h else None,compiled=compiled,
     current_fragments=[dict(id=r['fragment_id'],branch=r['is_branch'],inline=r['is_inline'],token_sha256=r['token_sha256']) for r in fragments],
     original_fragments=[dict(id=r['fragment_id'],branch=r['is_branch'],inline=r['is_inline'],token_sha256=r['token_sha256']) for r in oldfrags],
     boundary='Mixed explicitly reviewed host acquisition/error/catch/output operations and actual existing Core/Common consumption; never generic HOST_ONLY',
     corpus='All24 exact frozen pairs in compatibility-families-v2.json; separate28966 preedit/458 late per configuration. Fixture-to-target links recorded separately; source/IL proof covers decomposition, not new original queries.',
     focused_tests=['focused-families-v2-Debug.json','focused-families-v2-Release.json'],original_queries=0))

# Actual native dependency edges come from complete CIL, not textual invocation guesses.
byowner={}
for n in u['members']:
 if n['parent_member'] is None:byowner.setdefault((n['owner'],n['name']),[]).append(n)
for c in contracts:
 deps=[]
 for config in ['Debug','Release']:
  definitions={r['method']:r for r in contexts[config]['methods']}
  for caller in c['compiled'][0 if config=='Debug' else 1]['current_methods']:
   for ins in definitions[caller]['instructions']:
    target=ins['operand']
    if ins['op'] not in ['call','callvirt','newobj'] or not isinstance(target,str) or not target.startswith('METHOD:'):continue
    full=target[7:];definition=definitions.get(full)
    source_candidates=byowner.get((definition['declaring_type'],definition['member_name']),[]) if definition else []
    deps.append(dict(configuration=config,caller=caller,op=ins['op'],target=full,
       current_source_candidates=[r['symbol_id'] for r in source_candidates],
       resolution='Exact complete CLR target; conservative complete reviewed source overload set where necessary'))
 c['compiled_dependency_edges']=deps

save('whole-body-contracts-v1.json',dict(status='REVIEWED_SOURCE_CIL_DEPENDENCY_CONTRACTS_NOT_READY',contracts=contracts,
   source_view_reader=pin(HERE/'source_views.py'),compiled_context=pin(HERE/'compiled-context-and-consumers.json'),type_fields=pin(HERE/'full-type-field-context.json'),
   source_compositions=[pin(HERE/p) for p in ['import-source-composition-v1.json','inference-source-composition-v1.json','rank-import-source-composition.json']],original_queries=0))

# Final source transition adds the new portable source separately from old paths.
plan=json.loads((HERE/'implementation-plan.json').read_bytes());trans=[]
for before in plan['source_before']:
 path=before['live']['path'];snap=HERE/'source-after/final-v1'/(path+'.txt');snap.parent.mkdir(parents=True,exist_ok=True)
 with snap.open('xb') as f:f.write((ROOT/path).read_bytes())
 trans.append(dict(path=path,before=before['live'],preimage=before['preimage'],after=pin(ROOT/path),snapshot=pin(snap)))
for path in plan['new_sources']:
 snap=HERE/'source-after/final-v1'/(path+'.txt');snap.parent.mkdir(parents=True,exist_ok=True)
 with snap.open('xb') as f:f.write((ROOT/path).read_bytes())
 trans.append(dict(path=path,before=None,preimage=None,after=pin(ROOT/path),snapshot=pin(snap)))
save('source-transitions-final-v1.json',dict(files=trans,original_queries=0,engines='UNCHANGED',public_UDF_contracts='UNCHANGED'))
print(json.dumps(dict(reviewed=len(contracts),new_historical=sum(r['new_support'] and r['original_id'] is not None for r in contracts),new_current_only=sum(r['new_support'] and r['original_id'] is None for r in contracts),source_paths=len(trans))))
