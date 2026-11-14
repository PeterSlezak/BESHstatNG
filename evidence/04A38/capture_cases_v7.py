"""Retain v6 matrix with corrected optional-cleanup handle fixture and trace.

Coverage instrumentation is a separate reference build, never an expected-value
implementation. Both instrumented and uninstrumented results must agree.
"""
import hashlib
import json
from preflight import HERE, pin, save


def main():
    out = HERE / 'preedit/attempts/capture-v7'
    out.mkdir(exist_ok=False)
    cases = []
    for line in (HERE / 'preedit/attempts/capture-v6/cases.jsonl').read_text(encoding='utf-8').splitlines():
        c = json.loads(line)
        c.pop('case_id')
        c['input_factory_version'] = 'v7'
        c['case_id'] = c['target'] + '.' + hashlib.sha256(json.dumps(c, sort_keys=True, separators=(',', ':')).encode()).hexdigest()[:24]
        cases.append(c)
    with (out / 'cases.jsonl').open('x', encoding='utf-8', newline='\n') as f:
        for c in cases:
            f.write(json.dumps(c, ensure_ascii=False, sort_keys=True, separators=(',', ':')) + '\n')
    (out / 'Capture38.cs.txt').write_bytes((HERE / 'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v7/specification.json', dict(
        status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED', rows=len(cases),
        cases=pin((out / 'cases.jsonl').relative_to(HERE.parents[1]).as_posix()),
        generator=pin('evidence/04A38/capture_cases_v7.py'), recorder=pin('evidence/04A38/helpers/Capture38.cs'),
        correction='Handle variants for optional PS.CLEANUP construct a real handle instead of dereferencing its default null argument. Two invalid v6 BUILD_INPUT rows retained unchanged.',
        original_library='Uninstrumented entry v6; exact original source and entry dependencies.',
        coverage_library='Isolated trace-v1; reversible call-entry insertions only. Require exact outputs equal uninstrumented entry.',
        chronology='BEFORE_ANY_PRODUCT_EDIT', native='NOT_RUN', scientific='NOT_GOLDEN',
        readiness='Independent complete reachability and ownership gate remains closed.'))
    print('Prepared', len(cases), 'v7 pre-production cases; unsealed')


if __name__ == '__main__':
    main()
