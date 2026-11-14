"""Preserve raw outcome evidence and explain unordered inherited first failure."""
from common import *
from collections import Counter
from baseline_report import python_receipt
test='scripts/tests/test_04a22_spc.py'
renderer='BESHstatNG/src/Graphics/SpcControlChartExcel.vb'
source=(ROOT/test).read_text(encoding='utf-8')
assert 'for name in set(a)-changed:' in source
receipts=[]
for label in ['w2c4-validation-v1-python-scripts','w2c5-validation-v2-python-scripts']:
 p=RUNROOT/'commands'/label/'stderr.log';raw=p.read_bytes();text=raw.decode('utf-8-sig')
 parts=text.split('\n======================================================================\n')
 matches=[s for s in parts if s.startswith(('FAIL: test_all_retained_native_methods_and_appearance_remain_exact','ERROR: test_all_retained_native_methods_and_appearance_remain_exact'))]
 assert len(matches)==1
 receipts.append(dict(label=label,command=clean(json.loads((p.parent/'command.json').read_bytes())),raw_stderr=pin(p),exact_trace=clean(matches[0])))
assert 'PanelSupportsSpecificationLimits' in receipts[0]['exact_trace'] and 'AssertionError' in receipts[0]['exact_trace']
assert "KeyError: 'IncludeNullableInBounds'" in receipts[1]['exact_trace']
assert git('cat-file','blob',BASE+':'+test).replace(b'\r\n',b'\n')==(ROOT/test).read_bytes().replace(b'\r\n',b'\n')
assert git('cat-file','blob',BASE+':'+renderer).replace(b'\r\n',b'\n')==(ROOT/renderer).read_bytes().replace(b'\r\n',b'\n')
d=json.loads((PREVIOUS/'nonpasses-v3.json').read_bytes())
delta=d['python_occurrence_delta_from_C4']
assert len(delta['extra'])==len(delta['lost'])==1
assert delta['extra'][0]['identity']==delta['lost'][0]['identity'] and delta['extra'][0]['outcome']=='ERROR' and delta['lost'][0]['outcome']=='FAIL'
save('python-nonpass-investigation.json',dict(status='EXACT_INHERITED_UNORDERED_SPC_FIRST_FAILURE_EXPLAINED',
    identity=delta['extra'][0]['identity'],occurrences=1,receipts=receipts,source=pin(ROOT/test),renderer=pin(ROOT/renderer),
    source_and_renderer_match_accepted_HEAD=True,
    first_failure='Both traces stop at scripts/tests/test_04a22_spc.py:68. Iteration over set(a)-changed is unordered and varies between Python processes; one member is changed, another absent after earlier authorized SPC extraction.',
    cause='AssertionError for PanelSupportsSpecificationLimits versus KeyError for IncludeNullableInBounds. Same identity and occurrence; no new production source or test change in v5. Preserve ERROR as ERROR.',
    baseline=dict(passed=765,failed=181,errors=10),v5=dict(passed=765,failed=180,errors=11),
    separate_C5_UTF16_attempt=pin(PREVIOUS/'python-encoding-correction-final.json'),
    test_modifications=0,relabeling=0,original_queries=0))
print('Exact SPC identity, raw messages, first-failure location and source invariance documented')
