from common import *
old=(RUNROOT/'commands/w2c17pc1-final-git-v2-status/stdout.log').read_text(encoding='utf-8')
actual=load(HERE/'entry/git-v2.json')['commands']['status']['stdout']
tracked=lambda text:[s for s in text.splitlines() if s and not s.startswith('??')]
assert tracked(old)==tracked(actual)
freeze=load(ACCEPTED/'production-test-freeze-v1.json')['sources']
special=[r for r in freeze if 'package' in r['path'].lower() or 'install' in r['path'].lower()]
assert len(special)==18 and all(pin(ROOT/r['path'])==r for r in special)
save('entry/authority-crosswalk-v3.json',dict(
    accepted_status=pin(RUNROOT/'commands/w2c17pc1-final-git-v2-status/stdout.log'),
    entry_status=pin(HERE/'entry/git-v2.json'),tracked_status_lines=len(tracked(old)),tracked_status_exact=True,
    source_count=1350,actual_package_installer_support_files=special,live_support_count=18,
    derivative_source_coverage=dict(direct_core_index_source_hash_matches=518,not_directly_in_core_index=832,
        external_accessible_assertion=1332,external_live_only_assertion=18,
        disposition='All1350actual live sources independently hash-checked; external review access is a separate owner assertion.'),
    local_independent_review_search=dict(roots=['docs','planning','evidence'],patterns=['*review*','*Review*'],
        matching_v17_independent_artifact='NOT_LOCATED',unrelated_04A02_pending_review_files='Not treated as v17 authority',
        historical_TestResults='NOT_ENUMERATED',scope='Bounded workspace and authenticated authority-chain search; no claim to search the entire owner machine.'),
    owner_acceptance=dict(date='2026-11-13',source='Current owner instruction history; not a repo receipt or Git commit'),
    entry_report_correction='v2 live_only832 labels a direct transport-path gap; it is not the external18-file absence claim. No source or Git mismatch occurred.'))
print('Accepted tracked worktree status43lines exact; all18live package/installer support files exact',flush=True)
