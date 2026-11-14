from common import *
from collections import Counter
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
comparisons=json.loads((HERE/'compatibility-families-v2.json').read_bytes())['results']
inputs={r['inputs']['path']:r for r in comparisons if r['configuration']=='Debug'}
index={}
for path,r in inputs.items():
 for line in (ROOT/path).read_text(encoding='utf-8-sig').splitlines():
  if not line.strip():continue
  item=json.loads(line);target=item.get('target','')
  if target:index.setdefault(target.rsplit('.',1)[-1],[]).append(dict(corpus=r['corpus'],input=path,case_id=item.get('case_id',item.get('id')),culture=item.get('culture'),target=target))
rows=[]
for c in contracts:
 matches=index.get(c['name'],[])
 rows.append(dict(original_id=c['original_id'],current_id=c['current_id'],name=c['name'],direct_cases=matches,
    direct_case_count=len(matches),public_parent_coverage='Compiled dependency graph in whole-body-contracts-v1.json links internal helpers to the separately captured public callers; no direct fixture is invented for an internal identity.',
    focused_tests=c['focused_tests'],original_queries=0))
save('compatibility-fixture-crosswalk-v1.json',dict(rows=rows,captured_inputs=[r['inputs'] for r in inputs.values()],
    separate_preedit_per_configuration=28966,separate_late_per_configuration=458,original_queries=0,
    qualification='Source/whole-CIL composition provides decomposition proof; existing sealed cases and current-only regression probes provide runtime evidence. No new original fit/query/capture.'))
save('deferred-compatibility-constraints-v1.json',dict(previous=pin(PREVIOUS/'deferred-compatibility-constraints.json'),
 constraints=[dict(identity='KENDALL_P / KENDALL_TAU',finding='Original p-value body requires4 complete pairs; coefficient permits3. Preserved and explicitly tested; no correction.'),
 dict(identity='TTEST_UNPAIRED_NI / TTEST_UNPAIRED_EQUIV / TryParseAlpha',finding='Callers initialize alpha.025, but missing alpha still follows the original parser fixed.05 default. Preserve before separately authorized defect work.'),
 dict(identity='EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalence',finding='Observed difference outside supplied margins throws and native UDF returns #VALUE. New invalid fixture corrected, existing engine unchanged.'),
 dict(identity='IndependentTTableSelection / LegacyUdfText38',finding='Fullwidth WELCH differs by standard host culture; preserve sk-SK rejection and existing comparison policy. Eight raw null/ignorable synthetic differences remain adverse evidence.'),
 dict(identity='TryGetOptionalProbabilityThresholds',finding='Dedup compares each sorted input with its immediate previous input, not the last retained threshold. Preserve strict1e-12 chained tolerance.')],
 disposition='Deferred04A39–48 source/compatibility constraints only. No new numbered batch, statistical fix, original query or native release.'))
print('Existing direct fixture IDs and compiled-parent coverage crosswalk recorded')
