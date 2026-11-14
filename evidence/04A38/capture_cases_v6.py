"""Append-only pre-production input correction and cache/lifecycle coverage.

All expected outputs still come solely from executed original source. This
generator specifies inputs, not outcomes; it does not seal the coverage gate.
"""
import hashlib
import itertools
import json
from preflight import HERE, pin, save


def main():
    out = HERE / 'preedit/attempts/capture-v6'
    out.mkdir(exist_ok=False)
    cases = []
    entry = json.loads((HERE / 'entry-members.json').read_bytes())

    def add(c):
        c = dict(c)
        c.pop('case_id', None)
        c['input_factory_version'] = 'v6'
        c['case_id'] = c['target'] + '.' + hashlib.sha256(
            json.dumps(c, sort_keys=True, separators=(',', ':')).encode()).hexdigest()[:24]
        cases.append(c)

    for line in (HERE / 'preedit/attempts/capture-v5/cases.jsonl').read_text(encoding='utf-8').splitlines():
        add(json.loads(line))
    for e, culture, variant in itertools.product(entry['exports'], ['', 'sk-SK'], ['uppercase', 'other_model']):
        if not any(a['name'] == 'handle' for a in e['arguments']):
            continue
        if e['effective_excel_name'].endswith('_DROP'):
            continue
        add(dict(target_kind='export', target=e['effective_excel_name'], culture=culture,
                 cohort='handle_identity_variants', handle_variant=variant, arguments={},
                 expected_stage='REAL_CREATED_HANDLE_THEN_ORIGINAL_QUERY; exact existing comparer'))
    for e, culture in itertools.product(entry['exports'], ['', 'sk-SK']):
        name = e['effective_excel_name']
        if not any(a['name'] == 'handle' for a in e['arguments']):
            continue
        if name.endswith('_DROP'):
            transaction = 'drop_repeat_query_miss'
        else:
            transaction = 'repeat_query'
        add(dict(target_kind='export', target=name, culture=culture,
                 cohort='handle_repeated_operations', transaction=transaction, arguments={},
                 expected_stage='ORIGINAL_SOURCE_REPEATED_OPERATIONS_WITH_SAME_CREATED_HANDLE'))
    for culture, comparer, handle in itertools.product(
            ['', 'sk-SK'], ['ordinal', 'ordinal_ignore_case'],
            ['null', 'missing', 'empty', 'error', 'blank', 'whitespace',
             'text:LM:known', 'text:lm:KNOWN', 'text:LM:absent', 'number:1']):
        add(dict(target_kind='helper', type='BESHStatNG.WorksheetFunctions.UdfCacheHelpers',
                 target='TryGetCachedHandle', generic_type='System.String', culture=culture,
                 cohort='isolated_actual_cache_helper', cache_comparer=comparer,
                 arguments={'handle': handle}, input_state=handle,
                 expected_stage='ORIGINAL_GENERIC_HELPER_WITH_EXPLICIT_ISOLATED_DICTIONARY; no lifecycle redesign'))
    for culture in ['', 'sk-SK']:
        add(dict(target_kind='export', target='BESH.REGR.LMM_TYPE3', culture=culture,
                 cohort='lmm_kr_type3', model_profile='kr_inference', arguments={},
                 expected_stage='REAL_ORIGINAL_KR_FIT_THEN_PUBLIC_TYPE3_QUERY'))
    assert len(cases) == len({c['case_id'] for c in cases})
    with (out / 'cases.jsonl').open('x', encoding='utf-8', newline='\n') as f:
        for c in cases:
            f.write(json.dumps(c, ensure_ascii=False, sort_keys=True, separators=(',', ':')) + '\n')
    (out / 'Capture38.cs.txt').write_bytes((HERE / 'helpers/Capture38.cs').read_bytes())
    save('preedit/attempts/capture-v6/specification.json', dict(
        status='PRE_EDIT_EXECUTION_ATTEMPT_NOT_SEALED', rows=len(cases),
        cases=pin((out / 'cases.jsonl').relative_to(HERE.parents[1]).as_posix()),
        generator=pin('evidence/04A38/capture_cases_v6.py'),
        recorder=pin('evidence/04A38/helpers/Capture38.cs'),
        chronology='BEFORE_ANY_PRODUCT_EDIT; prior pilot attempts retained byte-for-byte',
        corrections=['Use actual accepted listwise option, not unsupported remove alias.',
                     'Do not prepend numeric headers to raw text subject/group identity columns.',
                     'Add generic actual cache helper, real handle case/model variants and repeated operations.'],
        comparison='Per-configuration exact typed/shape/IEEE/string/error records; no tolerance or NaN normalization.',
        metadata_policy=pin('evidence/04A38/preedit/attempts/capture-v3/specification.json'),
        native='NOT_RUN; explicit unregistered wizard-false test double; coercion fails closed',
        scientific='NOT_GOLDEN',
        remaining_gate='Independent complete source/ownership/coverage audit must pass before a final freeze or product edit.'))
    print('Prepared', len(cases), 'v6 pre-production cases; unsealed')


if __name__ == '__main__':
    main()
