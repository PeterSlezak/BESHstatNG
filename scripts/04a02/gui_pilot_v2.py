"""C08 N0 supervisor and mockable N1 state-capture protocol; NO live adapter qualified.

No COM/UIA import, process enumeration, launch, click, write, kill or shutdown on
import/default execution. Callbacks in tests are synthetic. The protocol requires
external risk review before any future provider callback. Provider getters can
have side effects; before/after observations cannot prove absence of all changes.
"""
import argparse
import hashlib
import json
from pathlib import Path
import time

ROOT = Path(__file__).resolve().parents[2]
CASE = '04A02-2X2-NATIVE-01'
OBLIGATIONS = {
    'control:BESHStatNG.Ui82x2.RefEdit1_WorksheetData',
    'control:BESHStatNG.Ui82x2.RefEditOutput',
    'control:BESHStatNG.Ui82x2.btnHelp',
}
CONTEXT = ('pid', 'start_utc_ticks', 'session_id', 'owner_sid', 'hwnd', 'window_title',
           'workbook_path', 'workbook_sha256', 'xll_path', 'xll_sha256', 'excel_sha256', 'source_sha256')
STATE = ('file_sha256', 'sheet_identities', 'used_range_content', 'named_ranges', 'dirty_flag')


def require(value, message):
    if not value:
        raise ValueError(message)


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def prerequisites(plan):
    missing = []
    if plan.get('case_id') != CASE or plan.get('stage') != 'N1':
        missing.append('exact 2x2 N1 stage')
    if plan.get('owner_gui_permission') != 'OWNER_PERMITTED_CONDITIONAL':
        missing.append('owner conditional GUI authority')
    risk = plan.get('risk_review', {})
    for key in ('reviewer', 'source', 'date', 'shared_refedit_receipt', 'direct_handler_receipt',
                'helper_sha256', 'supervisor_sha256', 'exact_uia_ids_review', 'provider_side_effect_review',
                'shutdown_protocol', 'external_deadline_receipt'):
        if not risk.get(key):
            missing.append('independent risk review: ' + key)
    if risk.get('status') != 'INDEPENDENTLY_ACCEPTED_N1_ONLY':
        missing.append('independent risk-owner N1 acceptance')
    if set(risk.get('obligation_ids', [])) != OBLIGATIONS or len(risk.get('obligation_ids', [])) != 3:
        missing.append('exact three 2x2 obligations')
    for key in CONTEXT:
        if not plan.get('expected', {}).get(key):
            missing.append('owned instance context: ' + key)
    if plan.get('actions') != ['invoke_reviewed_ribbon_id', 'capture_initialized_state']:
        missing.append('N1 actions only')
    if type(plan.get('max_seconds')) is not int or not 0 < plan['max_seconds'] <= 15:
        missing.append('external deadline at most 15 seconds')
    if plan.get('measurements') != list(STATE):
        missing.append('measured workbook before/after protocol')
    return missing


def check_context(actual, expected):
    require(type(actual) is dict, 'Context must be inert data')
    require(all(type(actual.get(k)) is type(expected[k]) and actual.get(k) == expected[k] for k in CONTEXT),
            'Wrong/reused PID, session, window, owner, source, workbook or XLL')
    require(actual.get('unexpected_modal') is False and actual.get('other_workbooks') == [] and
            actual.get('security_prompt') is False and actual.get('unknown_provider') is False,
            'Unexpected workbook/modal/security/provider context')


def capture_protocol(plan, adapter, evidence, monotonic=time.monotonic):
    """For mocked N0 qualification; future native adapter needs separate hash review.

    Each callback may invoke provider code. Revalidate context around every
    callback; detect side effects that the measurement interface can observe.
    Failures preserve partial evidence. No retries, dismissals or automatic restore.
    """
    require(not prerequisites(plan), 'NO_GO: missing risk/stage/context evidence')
    evidence = Path(evidence)
    evidence.mkdir(exist_ok=False)
    started = monotonic()
    trace = []

    def retain(name, value):
        with (evidence / name).open('x', encoding='utf-8') as stream:
            json.dump(value, stream, ensure_ascii=True, allow_nan=False, indent=2)

    def context():
        require(monotonic() - started < plan['max_seconds'], 'Deadline expired; external supervisor must stop helper only')
        check_context(adapter.context(), plan['expected'])

    def call(name):
        context()
        value = getattr(adapter, name)()
        trace.append(name)
        context()
        return value

    try:
        before = call('workbook_state')
        require(set(before) == set(STATE) and all(before[k] is not None for k in STATE), 'Unobserved workbook postconditions')
        retain('before.json', before)
        call('invoke_reviewed_ribbon_id')
        controls = call('initialized_controls')
        require(type(controls) is list and 0 < len(controls) <= 1500, 'Missing/unbounded UIA state')
        seen = set()
        for row in controls:
            require(set(row) == {'automation_id', 'name', 'enabled', 'visible', 'value', 'toggle', 'window_state'},
                    'Unsupported initialized-state schema')
            require(row['automation_id'] in plan['risk_review']['exact_uia_ids_review'] and
                    row['automation_id'] not in seen, 'Unknown/duplicate AutomationId; no coordinate fallback')
            seen.add(row['automation_id'])
        retain('controls.json', controls)
        if plan.get('screenshot_allowed') is True:
            shot = call('screenshot')
            require(type(shot) is bytes and shot, 'Screenshot failed')
            with (evidence / 'screenshot.bin').open('xb') as stream:
                stream.write(shot)
        after = call('workbook_state')
        retain('after.json', after)
        require(before == after, 'Observable workbook change; preserve and halt')
        retain('result.json', dict(status='MOCK_PROTOCOL_COMPLETE_NOT_NATIVE_EVIDENCE', trace=trace,
            observation='No change in the supplied measurements; UIA cannot establish full sheet/UsedRange/names/dirty state without a separately reviewed measurement adapter.',
            unobservable_side_effects='Provider transient changes, unsaved or external effects outside the adapter remain unknown.',
            N2='NO_GO_REQUIRES_REVIEWED_N1_AND_SEPARATE_WORKBOOK_DESTINATION_ACTION_AUTHORITY'))
    except BaseException as error:
        retain('failure.json', dict(status='FAILED_RETAINED_NO_RETRY', trace=trace, error_type=type(error).__name__, message=str(error)))
        raise


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--plan', type=Path, required=True)
    parser.add_argument('--stage', choices=['N0', 'N1', 'N2'], default='N0')
    args = parser.parse_args()
    require(args.stage == 'N0', 'NO_GO: no independently reviewed native UIA adapter, direct RefEdit mitigation or exact stage GO')
    plan = json.loads(args.plan.read_bytes())
    print(json.dumps(dict(stage='N0', classification='HOST_FREE_DATA_ONLY', live_calls=0,
                          status='NO_GO', missing=prerequisites(plan))))


if __name__ == '__main__':
    main()
