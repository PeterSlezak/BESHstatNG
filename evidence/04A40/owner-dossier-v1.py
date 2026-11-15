from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'evidence/04A40'))
from common40_v3 import *
import re
lock();d=load(HERE/'final/readiness-v1.json');p=load(HERE/'python/parent-event-reconciliation-v2.json');g=load(HERE/'final/git-final-v1.json');di=load(HERE/'python/diagnostic-dispositions-v1.json')
pres=load(HERE/'preservation/historical-readback-v4.json');before=(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.15.md').read_text(encoding='utf-8-sig');after=(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md').read_text(encoding='utf-8-sig')
pattern=r'^\|\s*\*\*((?:\d{2}|04A)\.\d{2})\*\*'
old_ids=re.findall(pattern,before,re.M);new_ids=re.findall(pattern,after,re.M);assert old_ids==new_ids and len(set(new_ids))==226
save('planning/final-reconciliation-v1.json',dict(revision='2.17',accepted39_merge=g['accepted39_merge'],main=MAIN,official_batch_identities=226,identity_order_unchanged=True,
    preserved2_15=pin('planning/BESHStatNG_CrossPlatform_MasterPlan2.15.md'),preserved2_16=pin(HERE/'planning/masterplan-2.16-frozen-v1.md'),current=pin('planning/BESHStatNG_CrossPlatform_MasterPlan.md'),
    scope='Living status current39 acceptance/current40 only; prior descriptive records retained historically; no40 acceptance or41 start.'))
text=f'''# Phase04A.40 owner review — {d['status']}

Host-neutral fit-option/request plumbing is implemented in existing Common policies for LM, GLM/NB2, ZIP, ordinal, multinomial, GEE, Cox and LMM. Eight typed helpers now serve nine Windows UDF parents. Formula/effects, MMRM and existing result preparation already used shared policies and were retained. No Core statistical algorithm, existing test source, exported signature, native UI patch, precision rule, array alias or original reference was changed.

The 200 regression/survival members are a different inventory from the retained192 input-policy members of39. Twelve family matrices contain actual GUI/Common/UDF member edges. Twenty canonical issue investigations retain original IDs, obligations and controls; three historical scope guards and one new privacy observation are recorded separately. The1169-entry39 register,1514 obligations,802 restricted controls and2967 crosswalk rows remain intact. No original obligation/control is discharged and no04A.48 exit disposition is assigned.

| Check | Actual corrected-final result |
|---|---|
| Debug compilation | Nine graph steps exit0, offline isolated outputs |
| Full Debug managed | 21,125 PASS /14 inherited FAIL;21,139 identities;12 added PASS;all14 messages/traces exact39 |
| Release compilation | Nine graph steps exit0 |
| Targeted Release | 3,108 PASS /2 inherited FAIL;3,110 identities;one trace differs from Debug;separate W3 Release comparison retained |
| Full Release suite | NOT_RUN;targeted family/compiled/compatibility coverage used |
| Exports |268 exact names/order/signatures/attributes in each configuration |
|40 current-main fixtures |48/configuration,statistical/input/exception contracts exact;four GEE elapsed-time fields explicitly excluded |
|39 fixtures |77/configuration,all exact fresh executions |
| Frozen compatibility |29,424/configuration exact current-only typed bytes and ordered identities;28,966 PRE_EDIT_FROZEN +458 separately labeled LATE_ARCHIVED_ENTRY_SUPPLEMENT |
| Python |{p['summary']['parent_outcomes']};956 exact parents;events{p['summary']['event_outcomes']} |
| Node |916 PASS fresh40 campaign,retained across correction with unchanged JS inputs;no Office/Mac/browser claim |
| Strict/profile/MkDocs |{load(HERE/'python/scanners-fresh-v2.json')['exits']};nonzero strict/profile remain nonpasses |

The first final Python campaign exposed five PASS-to-FAIL parents. Two batch defects were corrected without modifying existing tests: remove a misleading host-keyword comment and restore unnecessary GEE GUI-intent setter churn. Their focused replay passed. A new source freeze and one replacement full Debug/Python campaign per corrected state were scheduled; all initial failures, builds, fixtures and raw outcomes remain preserved. Node JS inputs were unchanged. This is not an attempt to erase a failed campaign.

The current repository candidate still fails privacy. My entry build receipt contains the local output directory: six original-rule occurrences in one field across content/decoded-JSON views, reported by both scanners, not twelve unique findings. The exact172325-byte original remains at `baseline/Debug-build-v1.json` and in a protected bit-exact copy. Its sanitized counterpart is `baseline/Debug-build-review-v2.json`; both original scanners find zero in that derivative. No scanner rule, scope, synthetic allowance or original FAIL is changed. Protected raw diagnostics must not be committed or published accidentally. This unresolved candidate representation is a blocker; publication privacy is evaluated separately against actual ZIP contents.

All54 previously reconciled diagnostic FAIL differences and956 parent identities are retained. The new crosswalk contains{len(di['rows'])} changed/current diagnostic records with raw fingerprints, exact test source/assertion sites and explicit unresolved argument differences. Three historical diff-scope FAILs reappear because the original guards reject any UDF edit; they remain FAIL and require applicability review. The two39 privacy FAIL parents each retain1941 findings, including earlier1599 +342 snapshots; W3's two privacy ERRORs remain unwaived. Current40 candidate snapshots are separately identified and do not replace that history.

C08 label/reference precision, C10/C11 requested/effective alpha/covariance headers, C12 supplied-start QIC/text clusters/prediction axes/weights/AR(1), survival and mixed-model candidates remain restricted. Four real supplied-start GEE fits retain their inherited exception. C13's earliest tied censor/event risk-set removal is source-pinned; grouped/stratified/no-event fixtures preserve current behavior. Historical R4.5.1/survival3.8.3 evidence is retained; fresh independent R is NOT_RUN because exact correction authority was not authenticated. No log-rank inference correction or native activation was performed. The old register's misattributed C13 source location is retained historically and corrected only by an additive current-source link.

Preservation verified the468-file checkpoint originals/copies and8984 listed protected paths. Four live evidence files were already LF-only committed renderings; exact CRLF originals remain protected and the prior owner line-ending authorization is recorded. Continuous precommit file-ID identity is NOT_VERIFIED;{pres['physical_identity_drift_count']} overlapping authority records differ in ID, without rewriting the old receipts. All1353 source/test/plan entry identities reconcile:1337 unchanged,15 authorized product files plus the living plan changed;two new test files added. The37484-file/154-archive W3 union is inherited qualified, not a fresh full-union audit. No historical TestResults traversal, deletion, original-engine execution, installer, XLL packaging, signing or deployment occurred.

Git branch `{g['branch']}`, HEAD/main/merge-base `{MAIN}` descend from owner39 commit `{g['accepted39_merge']}` and predecessor `{PREVIOUS}`. The owner BuildDate commit is preserved. The staged index is empty and its bytes match entry; `git diff --check` exits0. No Codex commit, stage, merge, reset, rebase, push or release occurred. Revision2.17 preserves all226 batch identities and the2.15/2.16 snapshots; methods.yml/Ribbon/Help availability is unchanged because external behavior is unchanged.

Review `final/readiness-v1.json`, `register/issue-delta-v4.json`, `python/diagnostic-dispositions-v1.json`, `ownership/family-parity-v3.json`, `matrix/actual-commands-v2.json` and the external publication delivery receipt. The standalone ZIP reader checks physical hashes/CRC and serialized case/test identities; full local evidence verification uses `verify40.py` on the owner checkout. Private raw logs, tracebacks and binary build trees are not required for the sanitized transport reader. Publication PASS, if proved externally, does not remove this NOT_READY verdict or accept the phase.

Smallest pending decision: approve an exact handling plan for the new raw entry receipt's Git-candidate representation while preserving its original bytes, and disposition the three historical diff-scope guards/remaining diagnostic argument changes. No broad scientific/native approval is requested. Independent review precedes owner acceptance;04A.41 is not authorized.

`P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A41_NOT_AUTHORIZED`
'''
for name in ['final/owner-review-v1.md','final/owner-review.md']:
    out=HERE/name;out.parent.mkdir(parents=True,exist_ok=True);out.open('x',encoding='utf-8',newline='\n').write(text)
save('final/next-owner-decisions-v1.json',dict(status=d['status'],blocking=d['blocking'],raw_artifact=pin(HERE/'baseline/Debug-build-v1.json'),safe_counterpart=pin(HERE/'baseline/Debug-build-review-v2.json'),
    smallest_decision='Authorize a versioned safe Git-candidate representation for this new raw entry receipt, preserving its protected original and all failed attempts; no source/test/scanner/historical oracle waiver.',
    separate_restricted_decisions=['C13 exact correction authority and independent R reference','C12 supplied-start/QIC science correction','Native UI/control gates04A45–47','Original obligations and04A48 exits'],
    no_automatic_acceptance_merge_next_batch=True,gates=GATES))
print('Owner dossier',d['status'],'226 batch IDs retained',flush=True)
