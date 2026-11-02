# 04A.02 scientific method and release decision packet — PROPOSED

**S PENDING; R NO; P NO; N NO. No signature or affirmative decision has been supplied.** This packet consolidates the required decisions; it does not authorize a process, approve a method, select an installed runtime, promote a golden or waive native obligations. The separate [pending form](04A02-owner-closeout-decisions.pending.json) is unsigned. The historical C03 form and v1/v2 oracle sources/manifests remain unchanged.

| Field | C30-SYN-ANOVA — proposal only | C30-SYN-KW — proposal only |
|---|---|---|
| Exact groups A/B/C | [1,2,3] / [2,4,6] / [3,5,7] | [1,2,2] / [2,4,6] / [3,5,7] |
| RowIds | [101,103,107] / [109,113,127] / [131,137,139], provenance only | Same; never used to break ties |
| Alpha / seed | .05 / N/A | .05 / N/A |
| Method | Classical equal-variance one-way; Welch and all post-hoc OFF | Average pooled ranks, Dunn OFF; raw and corrected pairs distinct |
| Independent script | v2 `stats::oneway.test(..., var.equal=TRUE)` plus `stats::anova(stats::lm(...))` | v2 `stats::kruskal.test(groups)` corrected H/p; separately reviewed explicit raw-H/C/rank-sum formula and upper-tail pchisq |
| df / tail | F(2,6) upper tail, total df 8 | Chi-square(2) upper tails for raw H and H/C; NOT exact/permutation |
| Metrics | F, p, SSbetween/within/total, MSbetween/within, df | Raw H, corrected H, raw p, corrected p, C, rank sums |
| Runtime | R/stats 4.5.1 candidate, NOT located or runtime qualified | Same |
| Current reference / promotion | NOT RUN / NOT APPROVED | NOT RUN / NOT APPROVED |

Exact v2 script/runner/source-fixture hashes are in unchanged `oracle-proposals/manifest-v2.json`. It supersedes the unsafe v1 R `wx` publication design; the operator refuses v1. The v2 scripts emit one candidate JSON to stdout, never open an output file. The Python operator checks gates, versions, hashes, runtime locks and output namespaces; retains stdout/stderr separately; rejects warnings/mixed/partial/nonfinite output; publishes exclusive staging + no-replace NTFS link + completion receipt. Mock qualification is not real R/runtime/ACL qualification. No R command, including a parse or version probe, ran in this continuation.

## S — requested per-case domain-owner decisions

Provide a new dated record identifying case, domain owner, decision source, method/formula/tail, script/manifest hashes, exact runtime/stats version, input/order/alpha/options and these per-field policies. Existing NIST/Core reference cases are context for other datasets, not approval of these exact cases.

| Field class | Illustrative UNAPPROVED rule | Rationale reviewer must accept or replace before execution |
|---|---|---|
| Count/df, group/RowId order, options, types/null/error/labels | Exact | Discrete source contract; no rounding tolerance |
| SS/MS, rank sums | atol 1e-12, rtol 2e-13 | Small finite accumulation; separately justify each SS/MS/rank field |
| F, raw H, corrected H | atol 5e-13, rtol 5e-12 | Division/accumulation; correction does not validate raw field automatically |
| Ordinary p | atol 2e-14, rtol 2e-10 | Tail approximation budget distinct from statistic arithmetic; approve min/max domain separately |
| Tie C | PENDING per-field rule | Small correction denominator may amplify error; approve allowed domain and reject C<=0 |
| Semantic table numeric cells | Underlying approved per-field rule | Exact coordinates/types/p flags independent of numerical tolerance |
| Warning/error strings, table order/types | Exact after source contract review | Empty string differs from null/native Excel error |
| Rendered workbook p text/NumberFormat/highlight | PENDING native policy | Common preview is not observed Excel formatting |
| Extreme/underflow/boundary p, overflow, chart geometry | NO approved tolerance | Separate log-tail/high-precision/host-unit review; no generic absolute pass |

Finite comparison proposal: `abs(candidate-reference) <= max(atol, rtol*abs(reference))`. Every metric needs a policy ID and rationale approved **before** observing agreement. No tolerance was tuned to product output. Specify rejection for missing/nonfinite/empty/singleton/all-equal/zero within-variance/C=0/overflow, unsupported degeneracy and ambiguous ties. These edge cases are not newly approved production numerical behavior. Exact/permutation inference, CIs, Welch and multiple comparisons are outside these two proposals.

## R — separate roadmap-owner reference-execution decision

S approval alone cannot release R. Supply a case-specific positive roadmap decision with exact Rscript.exe SHA, R.dll and dependency closure/version/hashes, manifest/v2 script/operator hashes, trusted output ACL/ancestor review, approved ordinary-p range, policies and fresh run root. No supplied executable hash or runtime installation is invented here. No network/install is authorized. Review actual `oracle_operator.py` source/CLI rather than running R for discovery.

Future operator invocation, **documentation only / NOT EXECUTED**:

```text
<approved WinPython> -B scripts/04a02/oracle_operator.py --execute-approved-reference --decision <new-positive-case-decision.json> --manifest docs/architecture/04A02/oracle-proposals/manifest-v2.json --r-home <approved-hash-pinned-R-home> --evidence-root <fresh-exclusive-reviewed-root> --name <new-case-run>
```

Recheck authentic S+R immediately before that action. The historical pending form cannot be passed as approval. Capture raw stdout/stderr, exit/timeout, versions, hashes/environment/ACL preflight, operator receipt and complete candidate JSON. Failed/partial evidence stays retained; never retry the same destination. Real R string escaping/DCF/JSON/Windows dependency behavior remains NOT RUNTIME QUALIFIED until this authorized run.

## P — later promotion decision after independent comparison and two reviews

Compare actual Core/Common metrics **and every mapped numerical table cell** to the independent candidate, retaining reference method/version/script/hash and product/test assembly hashes. Record per-field candidate/reference/absolute and relative differences, approved tolerance/p-domain, outcome and exceptions; no algorithm/input/tolerance adjustment to force a pass. A successful comparison yields only `INDEPENDENT_REFERENCE_COMPARED_NOT_PROMOTED`.

Then obtain reviewer #1, a distinct independent reviewer #2, JSON/provenance/comparison/exception review and a separate dated owner P decision naming the exact immutable run/comparison hashes. Only then create a new versioned approved-golden bundle and its deterministic tests. Current candidate continuity comparisons have `reference=null`, `approved_golden=null`, no tolerance applied; they are not this independent comparison. No `04A02-reference-comparison-v1.json` or approved-golden bundle is created while R/P remain closed.

## N and final roadmap scope

Native release is independent of S/R/P. Use the [native procedure](04A02-native-disposable-workbook-procedure.md) and exact [G4 crosswalk](04A02-G4-native-capture-ready-v1.json). Every applicable control/obligation and shared UNKNOWN_HIGH RefEdit edge needs mitigation, executed regression and explicit owner release, plus **separate disposable-workbook/native execution authority**. Static source reachability cannot waive restrictions. See the [proposed restricted deferrals](04A02-proposed-restricted-deferrals.md) if the roadmap owner prefers a narrow recorded scope amendment; it is not granted by this document.

Minimum next action for each typed case: a new combined, case-specific **S review plus separate R authorization** record with exact policies/hashes/runtime/ACL evidence. P must remain a subsequent decision after comparison/two reviews. For each native case: exact N release after mitigation or a separately approved narrow roadmap amendment. Until then official 04A.02 is not ready for acceptance.
