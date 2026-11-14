"""Readable wave and F handovers; no owner acceptance or proposed execution."""
import json,sys
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3];V6=ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'
def load(p):return json.loads(p.read_bytes())
def write(p,t):
 with p.open('x',encoding='utf-8',newline='\n') as f:f.write(t)
def main():
 comparison=load(V6/'compatibility-comparison.json');ready=load(HERE/'readiness.json');rows=[]
 for config,r in comparison['configurations'].items():
  for c in r['cases']:rows.append(f"| {config} | {c['culture'] or 'Invariant'} | EXACT | Intercept, index0 | True1477–1479; False1480 propagated |")
 fdoc='''# Approval F: authenticated intercept-group gate PASS

**MMRM_INTERCEPT_GROUP_GATE_PASS + NOT_READY_04A38.** This packet is the narrow
ApprovalF compatibility gate; whole-batch work and the consolidated W1 decision
are documented in `../../completion/w1/owner-review.md`.

The verbatim2026-11-09 approval is in owner-authorization.json, separately from
A432/B12/C2/D4/E2. F adds exactly2 distinct cases/configuration, one per culture,
for four comparisons total. Every new capture is LATE_ARCHIVED_ENTRY_SUPPLEMENT.
The organizational final sentence of the quote grants no extra reference cases.

All4,695 original files and41 snapshots were freshly authenticated against pinned
Git ba1636f5356ac761509c2cfa16fc66b92430935d. Original MixedModelUDFs snapshot is
92,704bytes/SHA596071f9fdb00fb94585e929a6899494293d2dc7ee0064faadf098b0d95dc13d;
original MMRM import is38,595bytes/SHA5297574cab59e631cf880807903a206f76dfb1509818a1825c1ca6798332071a.
Raw and proved LF/CRLF source identities remain separate. Original Core/Common/
Windows were freshly built offline in Debug/Release from original sources and
cached dependencies. No current helper or substitute statistic was linked as
original. Independent reconstruction verified170 original compilation inputs
and36 observation-only hook instances. Actual module SHA/size/MVID, compiler and
package identities and commands are preserved. Native attempts were zero.

The exact public call is MMRM_LSMEANS(handle,group,alpha): a real fresh successful
MMRM_FIT handle, scalar System.String literal Intercept, and true Nothing alpha.
The original factory bytes exactly reproduce the immutable C–E factory. Each
run verifies nonnull cached Result,54 saved design rows/visits1/2/3 each18 and six
fixed-design names. Actual fitted intercept indices are observed, not inferred
from a fabricated handle. Before original model/query execution, .NET materialized
both cases/configuration and froze all argument/factory/source/culture/session
and prospective branch metadata. An inherited console phrase typed arrays is
explicitly qualified: F's actual group runtime type is System.String, not an array.

Input seal: ea476dcd4b1e60c4ddb19cc47a25e01f32a440cbe6ffb2f57928e6b086db8954.
Original-output seal:262229291122a20199982d8278f96e5d37a0531f4fdecf204c33c9db7e0748c0.
Original plain/repeat/trace ran independently and agreed before output sealing.
Current compilation/execution began afterward, supported by seal predecessors,
runtime timestamps and compiled-artifact chronology. Seals were not refreshed.

| Configuration | Culture | Comparison | Actual lookup | Original witness |
|---|---|---|---|---|
'''+ '\n'.join(rows)+'''

All four actual public results are scalar System.String:

```text
BESH.REGR.MMRM_LSMEANS error: the intercept column cannot be used as a grouping factor.
```

Original trace proves the scalar route, initialized three ByRef outputs, successful
index0/nameIntercept lookup, the1477/1478 true condition and1479 error assignment.
Immutable next statement1480 returnsFalse; actual public Not TryGetDesignColumnValues
arm proves propagation. Returned private resolvedName and values remain Nothing;
errorMessage contains the observed original text. No range-profile route, earlier
unknown-column guard, statistical report, exception or public argument mutation
substitutes for this witness. Culture and UI culture are both set/restored.
Raw GUID sessions remain; only HANDLE:MMRM:SESSION1 is symbolized. No tolerance,
whitespace/text/error/shape/bounds/Double/culture/source-row normalization is used.

Per configuration:2 unique cases,6 original executions (plain/repeat/trace),2
current executions; each has one real fit and one public query. All public result,
argument, session/design/visit and branch bytes agree. Case/query/input digests
are in compatibility-comparison.json; private ByRef observations and source paths
are in original-coverage.json. Reports returned:0. Gate tests54+16passed, public-copy
reader passed, and457 unchanged prior focused tests passed. All older failures
and NOT_READY decisions remain intact. This F gate had no compiler/capture/test
nonpass; later W1 setup/adverse-fixture failures are separately preserved and
qualified in the master dossier.

No production/existing test/project or owner-doc edit was made. The new W1 census
found two consolidated essential reference gaps, so W2/W3 did not start. Read
completion/w1/owner-decision-needed.md; no proposed input was materialized or run.
Full managed/Node/Python/strict/profile/MkDocs suites were NOT_RUN this evidence-only
round. Their inherited nonpasses are neither removed nor presented as new runs.

P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED; NOT_GOLDEN.
Native Excel/COM/UIA/RefEdit, R, Office/Mac manual, installer/signing/deployment,
owner acceptance and04A.39 remain unrun/unapproved. No stage/commit/merge/push.
'''
 write(V6/'owner-review.md',fdoc)
 doc='''# 04A38-W1: F gate passed; full survey yields one consolidated reference stop

**MMRM_INTERCEPT_GROUP_GATE_PASS + NOT_READY_04A38.** W1 executed its original
gate and whole-universe read-only census. **W2 and W3 have not started**, because
the master prompt requires an authority stop for two essential uncovered paths.
This is one consolidated decision request, not permission to execute either case
or a claim that three-wave completion was guaranteed.

The F gate is2/2EXACT Debug and2/2EXACT Release, both Invariant/sk-SK. Every case
uses a real original/current fit, scalar String Intercept and Nothing alpha. All
four original true1477–1479 guards executed, lookup was index0/Intercept, private
resolvedName/values remained Nothing, original error propagated, and no report or
exception occurred. Original input then original plain/repeat/trace output seals
preceded current compilation/execution. See the v6 owner-review, source/maps,
typed inputs, raw outcomes/ByRef observations and independent gate readers.

The fresh census—not inherited arithmetic—found:

| Population | Original | Current |
|---|---:|---:|
| UDF source files | 41 | 41 |
| Behavioral identities | 704 (703historical +1accepted native callback) | 719 |
| Raw support declarations | 347 | 347 |
| Support overlap / additional distinct | 30 /317 | 30 /317 |
| Statement/inline fragment union | 20,683 | 16,946 |
| Branch/short-circuit syntax nodes | 5,511 | 4,006 |
| Inline syntax nodes | 1,397 | 1,157 |
| Registered exports | 268 | 268 |

These syntax populations overlap and do not multiply semantic cases. The independent
reader checks actual UTF16 source spans/body bytes and all identities; it does not
equate syntax enumeration, method entry or a matching error substring with executed
branch coverage. The archived/Git lambda crosswalk resolves duplicate headers and
bodies using exact normalized spans, preserving both raw LF/CRLF identities.
Current relocation/merge/added callback identities are listed explicitly. They
remain W2 ownership obligations, not automatically closed aliases.

Actual compiled Windows IL was freshly read without executing methods or initializers.
All268 source signatures/defaults/attributes and compiled Debug exports/defaults/
attributes match the authenticated original baseline. Actual registered caller and
Core/Common edges are checked against resolved CIL, not source search. **444 original
members have some current portable edge; this is not444 completed bodies.**226
original bodies changed or reallocated since entry. **Zero historical whole bodies
are certified closed;704 remain OPEN,719 current members and317 distinct support
declarations still require W2 disposition.** Existing199/505 progress categories
are not represented as fresh completion counts. No new portable service or Windows
consumer was implemented in this read-only wave.

The original/current full syntax inventories, all704-member×evidence×target-owner
matrix, complete5,511-node branch adequacy matrix,16,946-current-fragment plan,
exact state/assignment/ByRef/culture/error/source-row crosswalk and coherent41-source
W2A/W2B schedule are versioned here. The source-family schedule includes every
canonical UDF source, not just MMRM/GEE. It covers import/header/group/nested/paired
rows, all model/profile boundaries, basic/regression preparation, calibration/ROC,
report/spill/error layouts and genuinely Windows registration/cache/marshalling.

The first conservative triage produced105 text/error candidates. Examining all
recorded result cells and exception/caught-error surfaces reduced that frontier
to87; all87 have explicit adequacy decisions. No old capture or expectation was
changed. For example, existing K-means supplied-center and shape-error cases were
not proposed as new cases merely because the worksheet return was an Excel error
cell while its detail was in caught_error_events. Static literal report layouts,
constructed-fit invariants and unchanged native acquisition were kept distinct
from actual late true-arm witnesses and mandatory reusable extraction.

Two source-specific paths still need an authentic original public oracle:

1. **GEE_SPEC_INTERCEPT_ROLE**: TryGetGeeLsmEstimateSpec, original1506–1509.
   Existing genuine helper entries:56/configuration; intercept-role guard outputs:0.
   ApprovalB validates at, not spec's fitted-intercept exclusion. Proposed same
   original GEE factory with2x3 spec Label/Weight/Intercept; L/Double1/Double1.
2. **MMRM_WIDE_MULTIROW_REJECTION**: TryGetMmrmLsmEstimateAtSpec,592–594.
   Existing genuine helper entries:26/configuration; target wide-multirow outputs:0.
   GEE's wide guard has no MMRM headerless probe; C/D/E/F do not exercise this arm.
   Proposed same original MMRM factory/spec, Nothing alpha,3x1at visit/Double1/Double2.

The single proposal is **four cases/configuration** (two kinds×two cultures), eight
configuration/culture results total. All future outputs/ByRef/error observations
remain UNKNOWN_NOT_CAPTURED. Neither runtime input packet exists and neither
query was run. Read owner-decision-needed.md and consolidated-reference-gaps.json
for canonical member IDs, source spans, precise types, factory/provenance, necessity,
shared gate requirements and authority. Owner W1.3.6 requires stopping before W2;
these reusable paths were not disguised as HOST_ONLY or deferred to04A.39.

Genuine pre-edit28,966/configuration remains separate. A432/B12/C2/D4/E2/F2 are
454 authorized late cases/configuration; no total inflates repeats. Accepted
04A.37 original6,912/late22,050 is separate. All source snapshots, old manifests,
old NOT_READY records, failed attempts and nonpass identities remain byte-preserved.
The v5 predecessor's1,249 hashes plus one null self-entry were freshly verified.
Live preservation checks4,695 tracked paths,4,663 entry-exact plus32 inherited
authorized transitions,278 pins,100 captures,16 owner docs and41 snapshots. The
approximately138GB retained TestResults history was not traversed, rehashed or pruned.

F tests54+16passed and457 unchanged prior focused tests passed. W1 independent
source/branch/caller/gap tests and final publication readers have separate raw
identity/exit receipts. Failed W1 attempts remain: missing assumed cached assets
(before compilation), ambiguous identical lambda nodes (before ledger publication),
and an initial adverse fixture lacking unchanged current source files (17 nominal
rejections/1ERROR; those rejections were not accepted as valid controls). Corrections
use the existing cached assets, exact normalized spans and a complete intact-verified
fixture; no source oracle, reader assertion or statistical expectation was weakened.

Full managed Debug/Release, Node/Python/strict/profile/MkDocs are NOT_RUN this
evidence-only round. Historical receipts remain managed20,556PASS/14inheritedFAIL
eachconfiguration; Python767PASS/178FAIL/11ERROR; Node916PASS; strict/profile/MkDocs
1/1/0. The wrapper was inspected but omitted for implicit restore/shared TestResults
and uncontrolled Legacy build effects. Safe isolated offline builds/readers ran.
W3 comprehensive validation and independent READY verifier remain unperformed.

Live branch codex/04A-38, HEAD/main/merge-base all
ba1636f5356ac761509c2cfa16fc66b92430935d; index empty, diff-check0. Complete entry
and closeout inventories are recorded. Only new v6/W1 evidence and ignored isolated
fixtures were added; old production/test/project/owner docs and evidence bytes were
not altered. No stage, commit, merge, push, reset, branch switch, install or04A.39.

Publication files have real SHA/size, canonical paths and one null self-entry.
All current41 sources are included for standalone census verification, including
unchanged sources absent from the old publication. Public-copy/ZIP byte checks
cannot establish live Git, private retained outputs or unarchived4,695-path state.
Any ZIP is a derivative with explicit inventory/exclusions and separate exact
filename/bytes/SHA/entry/CRC receipt, not a replacement for authoritative manifests.

P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED; NOT_GOLDEN.
No native Excel/COM/UIA/RefEdit, independent R, manual Office/Mac, installer, signing,
deployment, scientific promotion or owner acceptance ran. Future controlled defect
and native work remains within the unchanged04A.39–48 charter. Suitable separately
authorized manual smoke would check scalar intercept rejection and profile row
alignment in Excel in both cultures; none occurred here.
'''
 write(HERE/'owner-review.md',doc)
 write(HERE/'executive-summary.md','''MMRM_INTERCEPT_GROUP_GATE_PASS + NOT_READY_04A38.

W1: F2/2Debug+2/2Release EXACT, actual original1477–1479 true guards; full41-source
census and268-export comparison completed. Fresh704/719 behavioral,317 distinct
support,5,511 original branch nodes; no whole-body ownership closure claimed.
Two consolidated essential gaps require one new4-case/configuration decision:
GEE spec intercept-role rejection and MMRM wide multirow rejection. Proposed
inputs/outputs remain unmaterialized/UNKNOWN_NOT_CAPTURED. W2/W3 not started.

No production/existing test/project/owner-document edits, staging/commit/merge or
04A.39. Scientific/native gates unchanged. Read owner-review.md, readiness.json,
owner-decision-needed.md and the publication/command/validation receipts.
''')
 write(HERE/'NEXT_WAVE_HANDOVER.md','''Continue only04A.38, existingcodex/04A-38. HEAD/main/base=ba1636f5356ac761509c2cfa16fc66b92430935d.
ApprovalF is exhausted and passed; do not recapture it. W1 completed read-only
full census and stopped at two consolidated essential gaps in owner-decision-needed.md.
No ApprovalG exists. Do not materialize or execute either proposed case or start
W2 production work until a separate owner decision. No native/scientific authority.

Authoritative inputs: v6/input-seal.json ea476dcd4b1e60c4ddb19cc47a25e01f32a440cbe6ffb2f57928e6b086db8954;
original-output-seal262229291122a20199982d8278f96e5d37a0531f4fdecf204c33c9db7e0748c0.
Use verify_w1_v2.py and final publication reader/manifest. Earlier matrices/readers
and failed tooling/fixture attempts remain immutable; -v3 matrices are the latest
full-surface triage. W2's exact41-source priority schedule and all704 historical,
719current,317distinct support plus source/statement/inline IDs are machine-readable.
Whole-body closed0/open704; no production change or new compiled consumer was made.

After an authorized new reference gate passes, re-authenticate liveGit/preservation,
continue W2A imports/models/profile policies thenW2B everyfamily/report/ownership,
andW3 comprehensive validation. Do not infer READY from any narrow gate or existing
edge. Preserve all old evidence and original outcomes; no stage/commit/merge/push,
install/restore/native/04A.39. Before product edits create byte-preserved historical
views for old root-relative source-pin readers; keep their assertions unchanged
and record actual authorized transitions/live source-pin qualifications explicitly.
''')
 print('Versioned F and W1 owner dossiers, executive summary and exact authority-blocked handover written')
if __name__=='__main__':main()
