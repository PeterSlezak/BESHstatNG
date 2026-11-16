"""Materialize observed recovery facts; readiness requires the later external seal."""
import sys
from pathlib import Path
ROOT=Path.cwd();sys.path.insert(0,str(ROOT/'evidence/04A47/recovery-v2'))
from recovery import *
STATUS='READY_FOR_OWNER_04A47_POSTRECOVERY_SAFE_SOFTWARE_REVIEW__NATIVE_GATE_NOT_AUTHORIZED'
def finish():
    source_lock();payload();auth=load(HERE/'payload-authentication-v1.json');py=load(HERE/'python-reconciliation-v2.json');assert not py['unresolved_diagnostic_events']
    inherited=load(HERE/'inherited-software-and-preservation-v1.json')
    sealed_private=load(PRIVATE/'sealed-private-pins-readback-v1.json');assert sealed_private['exact'] and sealed_private['binary_pins']==10
    save('sealed-private-readback-summary-v1.json',dict(classification='FRESH_READBACK_OF_SEALED_REFERENCES_NOT_A_BUILD',exact=True,private_pins=len(sealed_private['files']),binary_pins=sealed_private['binary_pins'],protected_report=pin(PRIVATE/'sealed-private-pins-readback-v1.json'),binaries_published=0))
    def historical(name):
        d=load(ROOT/name);out={}
        for p in d['parts']:
            exact(p['raw'])
            for r in load(ROOT/p['raw']['path'])['records']:out[r['id']]=r
        return out
    a=historical('evidence/04A46/python/parent-event-reconciliation-v2.json');b=historical('evidence/04A47/python/parent-event-reconciliation-v2.json');c={p['id']:p for p in load(PRIVATE/'focused-v1/raw-result.json')['records']}
    rows=[]
    for k in IDS:
        versions=[]
        for label,parents in [('accepted46',a),('v1',b),('focused_v2',c)]:
            r=parents[k];versions.append(dict(version=label,outcome=r['outcome'],runtime_seconds=r.get('seconds',r.get('end_seconds',0)-r.get('start_seconds',0)),events=[dict(outcome=e['outcome'],exception_class=e.get('exception_class','FileNotFoundError' if e['outcome']=='ERROR' and 'FileNotFoundError' in e['traceback'] else 'AssertionError'),message_sha256=sha(e['message'].encode()),traceback_sha256=e['traceback_sha256']) for e in r['events']]))
        rows.append(dict(id=k,source=next(p for p in auth['source_tests'] if p['path'].endswith(k.split('.')[0]+'.py')),versions=versions))
    save('focused-three-timelines-v2.json',dict(parents=rows,input=auth['payload'],old_errors_preserved=True,old24_FAIL_unwaived=True,classification='FRESH_TWO_PARENT_RUN_PLUS_AUTHENTICATED_HISTORICAL_READBACK'))
    before=(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.24.md').read_bytes();after=(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md').read_bytes();assert after.startswith(before)
    patch=git('diff','--','.gitignore','planning/BESHStatNG_CrossPlatform_MasterPlan.md')
    with (HERE/'owner-ignore-and-plan-transitions-v1.patch').open('xb') as f:f.write(patch)
    save('source-and-command-continuity-v1.json',dict(classification='NO_PRODUCT_OR_ORIGINAL_TEST_EDITS;NEW_RECOVERY_EVIDENCE_ONLY',source_freeze=pin(OLD/'matrix/source-freeze-v1.json'),source_pins_exact=True,original_six_sources=[pin(p['path']) for p in load(OLD/'matrix/source-freeze-v1.json')['new_tests']],project=pin('BESHStatNG.Test/BESHStatNG.Tests.vbproj'),plan_snapshot=pin('planning/BESHStatNG_CrossPlatform_MasterPlan2.24.md'),plan_successor=pin('planning/BESHStatNG_CrossPlatform_MasterPlan.md'),plan_original_bytes_prefix_exact=True,original_roadmap_ids=226,ignore=pin('.gitignore'),owner_ignore_approval=auth['ignore_policy_exception'],interpreter=auth['interpreter'],discovery=pin(HERE/'discovery-v1.json'),runner_path_only_provenance=pin(HERE/'runner-path-transitions-v1.json'),commands=[dict(alias='AUTHENTICATE_EXISTING_BYTES',script='recovery.py auth',exit_code=0),dict(alias='FOCUSED_TWO_UNCHANGED_PARENTS',script='recovery.py focused',driver_exit=0,unittest_exit=1,outcomes=['PASS','FAIL']),dict(alias='ISOLATED_DISCOVERY_AND_RETAINED_CONTROLS',script='campaign.py prepare',exit_code=0),dict(alias='ONE_FRESH_956_PARENT_CAMPAIGN',script='campaign.py execute',driver_exit=0,worker_unittest_exits=[0,0,1,1,1,0,1],postcheck=pin(PRIVATE/'python-input-postcheck-v2.json'),parent_outcomes=py['outcomes'])],safe_reproduction='Use the authenticated existing Python3.14.7 interpreter with -I -B from repository root. Prepare NEW versioned output paths first; listed commands are completed-run aliases, not permission to overwrite exclusive output directories.',managed_builds='NOT_RERUN',Node='NOT_RERUN',failed_attempts_retained=dict(preapproval_ignore_stop=pin(HERE/'authentication-blocker-v1.json'),isolated_driver_bootstrap='Initial helper import required explicit module path under -I;driver only corrected before full execution;no test source or assertion changed',first_diagnostic_review=pin(HERE/'python-reconciliation-v1.json'),full_campaigns=1),native='NOT_RUN',gates=original.GATES))
    save('readiness-v2.json',dict(status=STATUS,effective_only_with_matching_external_receipt=True,payload_authenticated=True,focused=['PASS','FAIL'],Python=py['outcomes'],fresh_complete_parents=956,source_guard_FAIL='04A05_ACTUAL_FAIL_UNWAIVED_OWNER_APPROVED_IGNORE_DIFF',unexplained_parent_transitions=0,unresolved_diagnostics=0,software_evidence='INHERITED_VERIFIED_BY_RECEIPT',native='NOT_RUN',actual_HWND_PID_XLL='NOT_VERIFIED',approval='NOT_APPROVED',MayRunNative=False,scientific='NOT_GOLDEN',discharges=0,next='04A48_NOT_AUTHORIZED',privacy_candidate_scope='ORIGINAL_UNCHANGED_PATHS_INCLUDING_NONIGNORED_PAYLOAD',public_payload_inclusion=False,prior_NOT_READY_and_loss_acceptance_preserved=True))
    text=f'''# 04A.47 post-recovery owner review v2 — 16 November 2026

`{STATUS}` is effective only with the matching external receipt confirming the final privacy, adverse-control and physical-delivery gates. This is a chronological software-evidence successor for owner review, not a new native acceptance or Git transition.

The owner had already accepted SAFE SOFTWARE with a then-valid historical-evidence-loss exception. The owner subsequently reported recovery, and the existing original payload was independently read and authenticated: **57,758,738 bytes**, SHA-256 `{EXPECTED['sha256']}`. The 344,343-byte original 04A.24 manifest matches SHA-256 `2d0bbea874e22b2d07c919d930e20377dcd31db14267a0df724a64ce760c241e`; its target record and source-pinned VB literal agree. All payload keys, 3,196 record count, six positive chart families and retained Core/preparation rejections validate. No payload byte was written, normalized, copied or regenerated.

The initial ignore-policy STOP is preserved. The owner explicitly approved the removed ignore rule. Accordingly the file is now **untracked and nonignored**; both unchanged scanners must include it in their complete candidate scans, while the public ZIP excludes it. It is not staged or automatically reproducible by a clean checkout. Secure backup/escrow destination and retention policy remain a separate owner choice; no backup was made.

| Original parent | Accepted46 | 04A47 v1 | Fresh focused v2 | Fresh full v2 |
|---|---|---|---|---|
| SPC23 six-chart payload contract | PASS | ERROR | PASS | PASS |
| SPC24 frozen-authority contract | FAIL | ERROR | FAIL | FAIL |

The SPC24 inherited AssertionError still concerns the original architecture README hash; its accepted46 message is exact and remains unwaived. No FileNotFoundError remains in either recovered parent. Exact IDs, module hashes, runtimes and message/trace fingerprints appear in `focused-three-timelines-v2.json`.

**One fresh complete campaign:** 956 distinct original parents, **833 PASS / 112 FAIL / 11 ERROR**, no lost/duplicate/skipped parent, peak three workers, all input pins unchanged. This is not a composite. Two outcome transitions restore the original payload availability. One additional PASS-to-FAIL transition is the unchanged `test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change`: line136 asserts that `.gitignore` has no diff, but the owner-approved deletion produces exactly `b'.gitignore' is not false`. This actual FAIL is retained, not waived. It explains the difference from the 834/111/11 reference aggregate. Four retained diagnostic events also change only their ignore SHA/path arguments; two change the append-only plan SHA. All {len(py['events'])} comparison-event keys (195 current events) are individually accounted, with no unresolved event. The 04A08 scope FAIL, earlier source-guard FAILs, 1,941 historical privacy findings per parent and W3 errors remain historical/unwaived.

Interpreter: Python {auth['interpreter']['version']}, binary SHA-256 `{auth['interpreter']['sha256']}`. Original focused modules retain SHA-256 `ef04954a158dd6c274eb40cdd20b54fa731b467ba5d91367a690993beb86bbb8` and `c1abd8affc76f30574a967d191ca147f70d9c7f12033f175d5acf481b8079461`. Raw commands, journals, tracebacks and driver failures remain protected locally. Reviewer-safe aliases and source/module pins are included; source assertions were never changed.

**Existing Windows evidence: INHERITED_VERIFIED_BY_RECEIPT**, not rerun. Debug 21,881 PASS/14 identical inherited FAIL; all 21,754 predecessor identities and 141 new PASS retained. Targeted Release 15,885 PASS/10 inherited FAIL with exact historical optimized traces; full Release NOT_RUN. Both configurations retain 268 exact exports and 29,424 exact frozen cases (28,966 original plus458 approved supplements). Six test-only sources/Compile entries, 101 source-derived methods, 141 new tests and147 observer cases/configuration remain unchanged. Node916 PASS is inherited/NOT_RERUN; strict/profile/MkDocs1/1/0 retain both NONPASS outcomes. No managed build was repeated.

The original v1 ZIP (6,988,940 bytes; SHA-256 `0fe14fe9558fadeb4040d2ffc5f28622c920add0bf418aaf9fb7ff7423d05bc8`) and receipt (141,137 bytes; SHA-256 `b328b8d298afed783b30cbe14ec98e021252c4a799f2171d2b8f9c92b615539d`) remain exact. Their NOT_READY status and833/110/13 campaign are never replaced. The new ZIP retains their exact member bytes as a sealed historical layer; its original root plan is the v1 plan. The actual current plan is separately named `recovery-v2/current-plan-successor-v1.md`, and exact Revision2.24 is preserved. No old authority is replayed onto current source.

The listed preservation readback covers tracked-entry transitions,468 checkpoint copies,8,984 historical paths,1,906 raw40 paths, recorded whole-store41–46 populations and the current original47 store. The 788 drifts remain recorded. The historical W3 union is INHERITED_QUALIFIED; continuous physical identity is NOT_VERIFIED. Canonical1169 issues/1514 obligations/802 restricted controls/2967 mappings remain unchanged, with zero discharges. The PSM orphan interrupt still has no proven live binding or mid-fit interruption.

The standalone v2 reader verifies the sealed v1 layer, original managed/ABI/compatibility evidence and new recovery semantics. Its external receipt binds every physical ZIP member including index, whole-file bytes/SHA, all original candidate/physical privacy scopes, real owner-local positive/negative controls, neutral replay and executed adverse controls. The receipt is separate and independently verifiable; no circular index self-hash or raw payload is included. Initial failed attempts and the first unresolved diagnostic review remain preserved with their successors.

Current branch `codex/04A-47`; HEAD/main/merge-base `{original.MAIN}`. Index SHA-256 `{auth['git']['index']['sha256']}` remains unchanged; staged empty and diff-check0. No earlier combined integration handover ran. New recovery docs/helpers, the exact plan snapshot and append-only plan note are the only Codex recovery edits; the approved ignore deletion and owner-provided payload are preserved. See the external receipt for the complete actual modified/untracked inventory.

Native Excel/COM/UIA/RefEdit, real HWND/PID/loaded-XLL attribution and independent Windows/COM approval remain NOT_RUN / NOT_VERIFIED / NOT_APPROVED; offline MayRunNative=False. AutoOpen-to-AutoUpdate remains unresolved before any future native launch. No production correction, scientific golden, native control discharge, commit, merge, push, installer or04A.48 work occurred. The owner may review this new delivery and separately decide any later Git transition.

`P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A48_NOT_AUTHORIZED`
'''
    for path in [ROOT/'docs/architecture/batch-04A-47-recovered-SPC23-owner-review-v2.md',HERE/'owner-review-v2.md']:
        with path.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
    print('Append-only post-recovery owner review, exact comparative tables and command/source continuity written',flush=True)
if __name__=='__main__':finish()
