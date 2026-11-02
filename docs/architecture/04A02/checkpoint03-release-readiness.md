# Checkpoint 03 — source-only release readiness, revision 1

**SOURCE-ONLY REVIEW COMPLETE / OWNER DECISIONS PENDING. Official 04A.02 NOT ACCEPTED.** No R/Rscript process, parse/version lookup, independent reference calculation or numerical golden generation was executed. R/stats 4.5.1 remains a candidate version with no observed installation or real executable/runtime hash.

## Supersession and the output defect

The independent source review correctly identified that R `file(open="wx")` is not a documented exclusive-create mode. The [official connections mode reference](https://search.r-project.org/R/refmans/base/html/connections.html) supplied by the owner is the source-review basis; this is not a claim of an R 4.5.1 experiment. No network lookup was made in this checkpoint. A prior `file.exists()` check cannot close the check/open race or prove no-clobber behavior.

The v1 R files and `manifest-v1.json` remain **byte-identical historical proposals**. Their old SHA-256 values remain:

| Historical source, retained and publication-unsafe | SHA-256 |
|---|---|
| `anova-proposed.R` | `4d6475f1ac12fbbc37424e6910a8a80fe76265f0b5c8ea3be45b0ec51a369c74` |
| `kw-proposed.R` | `874a14699dc359be1f812e2e8b743b9b7a857aa10db91d7fc50e33cf76eef8d9` |

[Manifest v2](oracle-proposals/manifest-v2.json) explicitly supersedes them for future execution and pins [ANOVA v2](oracle-proposals/anova-proposed-v2.R), [KW v2](oracle-proposals/kw-proposed-v2.R), the Python operator, unchanged fixture bytes and dataset hashes. It records the reason and prior hashes; no old source is relabeled with a new hash. The runner refuses manifest v1. The old Checkpoint 02 statements about `wx` are historical and are superseded by this note, not silently rewritten.

## Ownership and publication protocol

`scripts/04a02/oracle_operator.py` implements a **proposed future operator boundary**, tested with dummy files and mocked processes only. R v2 accepts one runner-owned DCF path and writes one candidate JSON line to stdout. It receives no output path and never opens a candidate file. Direct R stdout or shell redirection is not an accepted publication protocol.

1. A future human operator must authenticate a separate case-specific domain-method decision and roadmap-owner execution decision. The code validates those records and requires an explicit execution CLI flag; it cannot authenticate human signatures cryptographically. The pending form is not authority.
2. Validate unchanged fixture/source pins, case/method/tail/finite policy, runtime/stats version, runner/script/manifest hashes, independently approved tolerances and ordinary-p range. Require separately reviewed runtime dependency closure and trusted output ACLs. No tolerance or runtime-hash default is invented.
3. Inspect ancestors for symlinks/junctions/reparse points. Hold deny-delete directory handles within explicit workspace/runtime/output trust anchors and deny-write/delete file handles for executable, declared runtime components, script and runner. Do not try to acquire privileged handles over unrelated profile directories. Trusted ancestor ACLs and the approved dependency closure are explicit external preconditions, not inferred from POSIX mode 0600 on Windows.
4. Atomically reserve a new execution directory with `mkdir`; never reuse one. Copy exact decision, proposal manifest and script into exclusively created files. Produce and round-trip-check the strict DCF. Hash verification uses actual open file contents, not merely an attestation string. R runs the private source copy; relevant source/DCF handles stay locked.
5. Capture raw stdout and stderr into **different** binary files created with `os.open(O_CREAT|O_EXCL|O_WRONLY|O_BINARY)`. Use `--vanilla`, explicit argv without a shell, controlled environment, no window and a 30-second timeout. No real process was invoked in these tests. An oversized stdout (>1 MiB), any stderr, nonzero exit, timeout, empty/partial/polluted JSON or schema/policy mismatch prevents publication. Capture-size admission is checked after process completion; this is not an OS disk quota or a sandbox for a hostile interpreter. The exact hash-pinned program and reviewed runtime must be trusted before execution.
6. Validate strict UTF-8 JSON, duplicate keys, nonfinite values, Unicode scalars, exact case/hash/data/options/provenance, distinct metrics, integer df/RowIds and the preapproved ordinary-p domain. This is a protocol/domain validator, **not** independent statistical validation or a Core-vs-R numerical comparator. No tolerance is used to make a result pass here.
7. Fully write and fsync a new retained staging file with O_EXCL. Add the final filename using `os.link` on the **same Windows NTFS filesystem**. Creating a hard link is atomic and does not replace an existing destination, including a case alias or concurrent winner. Both the staging and final names remain; no rename, deletion, symlink, copy fallback, overwrite or cleanup occurs. Unsupported hard links or permissions fail closed. Hard links are not symlinks/reparse replacements.
8. Publish `publication.json` through the same complete-file boundary **last**. A consumer must call the receipt/hash validator; a complete `candidate.json` without a valid receipt is not an admitted publication. The consumer verifies candidate/raw hashes and empty stderr, and always returns `CANDIDATE_ONLY_NOT_GOLDEN`.

Existing targets, failed links, partial staging, raw captures and failure records remain in place. If publication fails between the candidate link and receipt link, candidate bytes may be complete, but there is no valid completion receipt. No failed attempt is removed or overwritten, and retry needs a fresh directory/name. If disk or permissions prevent even a failure record, retain whatever exists and stop; never clean up to make room.

The no-clobber primitives, case collision, two-thread race, retained staging on flush/link failure and Windows input-file lock were exercised in **scratch-only Python tests**. They do not qualify an R runtime, arbitrary filesystem, hostile administrator, network share or every Windows ACL configuration. UNC/device/ADS, trailing dot/space, reserved-device, parent traversal and reparse paths are refused. Real R execution remains blocked pending owner decisions and controlled integration qualification.

## Scientific and schema source review

| Area | Source review finding | Qualification |
|---|---|---|
| ANOVA data | A `[1,2,3]`, B `[2,4,6]`, C `[3,5,7]`; alpha .05; seed N/A | Matches immutable C30 fixture |
| ANOVA methods | `stats::oneway.test(...,var.equal=TRUE)` and `stats::anova(stats::lm(...))`; equal-variance classical F, df 2/6, total 8; no Welch/post-hoc | Proposed, owner scientific approval pending; NOT RUNTIME QUALIFIED |
| KW data | A `[1,2,2]`, B `[2,4,6]`, C `[3,5,7]`; alpha .05; seed N/A | Matches immutable C30 fixture, with ties retained |
| KW inference | Average pooled ranks; raw H `12/[N(N+1)]*sum(Rj^2/nj)-3(N+1)`; C `1-sum(t^3-t)/(N^3-N)`; separately reviewed H/C; raw upper chi-square p; R `kruskal.test(groups)` supplies corrected H/p, df 2 | Four metric names preserved; no Dunn or exact/permutation claim; NOT RUNTIME QUALIFIED |
| Common result contract | ANOVA `FStatistic/PValue`; KW `HStatistic/TieCorrectedHStatistic/PValue/TieCorrectedPValue` | Source-backed mapping; no new expected values |
| Identity | Labels A/B/C and RowIds `[101,103,107]/[109,113,127]/[131,137,139]` | RowIds are provenance only, never passed to production GroupedNumericData or used as tie breakers |
| DCF | Canonical Python producer: one exact ordered set of ten nonempty ASCII fields, LF termination, no duplicates/second record/continuations | Python rejection/round-trip tested. R first checks field names/count/order/duplicates, then `read.dcf(fields=required, all=FALSE)` and exact one-row matrix semantics; NOT RUNTIME QUALIFIED |
| Hash truth | Python hashes actual executable, declared runtime files, script and runner under Windows locks; R echoes a bound digest and actual reported versions | Echo is not a hash calculation or a signature. Complete runtime closure/ACL review is required externally |
| Locale | R `options(warn=2,OutDec=".")`, checked `LC_NUMERIC=C`, `formatC(...digits=17,format="g",decimal.mark=".")` | Source only; real locale/startup behavior NOT RUNTIME QUALIFIED |
| JSON strings | Explicit Unicode-codepoint escapes for quote/backslash/control characters replace R `encodeString` conventions; reject NA, invalid Unicode and duplicate/empty keys | Python receiver tested on malformed Unicode/pollution; R serializer itself NOT RUNTIME QUALIFIED |
| Shapes | Named lists are objects; unnamed lists/vectors are arrays; dataset has explicit group-label/value/RowId arrays; metrics retain scalar/df/rank-array shapes | Python protocol tests pass with mocks; R shape production NOT RUNTIME QUALIFIED |
| Missing/degenerate/extreme cases | Frozen inputs finite/nonempty; zero within SS and C<=0 rejected; warnings become errors; boundary p rejected by R, and owner-approved ordinary-p range enforced by Python | Policies remain proposals; no unseen singular/tail behavior claimed tested |

No independent numerical result was computed. Existing NIST/nonparametric reference tests are context, not approval for these exact datasets. No method or tolerance is chosen after observing independent agreement. Ordinary tolerance interpretation remains `abs(error) <= max(atol,rtol*abs(reference))`, with per-case classes and written rationale; all proposed decision values remain null pending owner approval. Extreme-tail alternatives require a separately reviewed policy/source revision.

## Independent decisions and next evidence

Review the [PENDING decision form](checkpoint03-owner-decisions.pending.json). It contains separate per-case fields for method approval, decision IDs/roles/references/date, R/stats candidate version, actual executable/runtime and source/runner/manifest SHA values, formula/tail/finite/extreme policies, ordinary-p range and tolerance rationale. Human identity/date/signature fields remain null. It must not be edited into a retroactive signature: create a new reviewed case-specific decision record later.

| Decision | Current state |
|---|---|
| Scientific method approval | **PENDING / false** |
| Authorize an independent R/reference process | **NO / false** |
| Promote reviewed numerical values to golden | **NO / false** |
| Native workbook/chart/form capture | **NO / false** |

Even a future method approval does not authorize execution. An execution decision cannot authorize promotion or native capture. After a separately authorized successful run, a **new immutable post-run review record** must contain raw candidate/stdout/stderr hashes, exact version/hash preflight, script/decision IDs, JSON validation, exact per-field Core/Common comparison against preapproved tolerances, all exceptions and two independent review decisions. Only a later explicit promotion decision can create a new approved-reference record. The operator deliberately has no promotion code.

Native cases remain NOT RUN until each applicable control ID has mitigation, executed regression, explicit owner release and disposable-workbook authority. The unchanged [full coverage register](coverage-register-v1.json) and [restriction crosswalk](native-restriction-crosswalk-v1.json) retain ANOVA, normality, 2x2, PCA, Windows tables/charts/form states. The two synthetic fixtures do not narrow full 04A.02 acceptance. Gate C2 remains UNMET; 1,514 obligations, 802 affected captures and UNKNOWN_HIGH RefEdit remain restricted.

Original strict/profile checks remain current-checkout **1/1**, not green. The historical 24 failed identities were diagnosed, not repaired or reclassified. Supplemental diagnostic exit 2 is explanatory and cannot replace either acceptance checker. A future branch-aware verifier requires separate scope approval; no old checker/input/byte baseline is changed here.

**STOP for separately recorded owner/domain-owner decisions.** No reference process, golden promotion, native capture, next checkpoint, 04A.03, staging, commit, merge, push, installer, signing or deployment is authorized by this readiness document.
