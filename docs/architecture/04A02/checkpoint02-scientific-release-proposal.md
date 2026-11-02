# Checkpoint 02 — independent scientific release proposal

**PREPARATION ONLY; scripts NOT EXECUTED. Official 04A.02 NOT ACCEPTED.**
No R installation/version lookup, R parse/run, SciPy run, external calculation or
reference output generation occurred in this checkpoint. Both frozen fixture
JSON records remain byte-identical, with `expected_numerical_reference=null`,
`reference_status=NOT_APPROVED` and `PENDING_ORACLE_APPROVAL`. Existing candidate
service observations are not independent references.

| Separate release gate | Current boolean | Evidence required before YES |
|---|---|---|
| Run independent R/reference scripts | **NO / false** | Written domain/owner decision for each method, exact executable/runtime/script hashes, version, tails, tolerances and rejection policy |
| Accept numerical values as golden | **NO / false** | A subsequent independent review of generated candidate values, differences, provenance and tolerances; a separately recorded promotion decision |
| Native workbook/chart/form capture | **NO / false** | Exact-ID mitigation and executed regression, explicit owner release, then separate disposable-workbook authority; current 802 restrictions remain |

The source proposals are [ANOVA](oracle-proposals/anova-proposed.R) and
[KW](oracle-proposals/kw-proposed.R). Their SHA-256 values and release booleans are
in [manifest v1](oracle-proposals/manifest-v1.json). They are review documents,
not build/test hooks. No approval DCF, executable hash or signature is supplied.
The scripts cannot approve or update a fixture or golden.

| Scientific decision | ANOVA proposal | KW proposal |
|---|---|---|
| Input | A `[1,2,3]`, B `[2,4,6]`, C `[3,5,7]` | A `[1,2,2]`, B `[2,4,6]`, C `[3,5,7]` |
| Identity/order | A, B, C; row arrays `[101,103,107]`, `[109,113,127]`, `[131,137,139]` | Identical group and provenance order; RowIds never break ties |
| Settings | Alpha 0.05; classical equal variance; no Welch/post-hoc; seed N/A | Alpha 0.05; no Dunn; average pooled ranks; seed N/A |
| Proposed independent calls | `stats::oneway.test(..., var.equal=TRUE)` and `stats::anova(stats::lm(...))` for SS/MS | `stats::kruskal.test(list(A,B,C))` for corrected H/asymptotic p; separately reviewed raw-H formula and `stats::pchisq(..., lower.tail=FALSE)` |
| Degrees of freedom/tail | Upper F tail with df `(2,6)`; total df 8 | Upper chi-square tail, df 2, for both H variants; no exact/permutation claim |
| Extra formula review | Classical SS/MS/F relationships; no locally generated expected constants | Average pooled ranks; H = `12/(N*(N+1))*sum(Rj^2/nj)-3*(N+1)`; C = `1-sum(t^3-t)/(N^3-N)`; keep H, H/C and both p-values distinct |
| Version candidate | R/stats **4.5.1**, not asserted installed | Same candidate; must be independently approved/pinned |

R bundled stats manuals for the **approved exact installation**, with their source
hashes, should be retained at future release. Existing moving development links
are contextual citations only: [oneway.test](https://stat.ethz.ch/R-manual/R-devel/library/stats/html/oneway.test.html),
[anova](https://stat.ethz.ch/R-manual/R-devel/library/stats/html/anova.html),
[kruskal.test](https://stat.ethz.ch/R-manual/R-devel/library/stats/html/kruskal.test.html).
No network lookup or download was performed in this checkpoint. NIST ANOVA and
Core nonparametric reference tests use other datasets and remain context only.

Proposed finite-quantity tolerance interpretation is
`abs(actual-reference) <= max(atol, rtol*abs(reference))`. These are candidates,
not approved admission thresholds:

| Quantity | atol | rtol | Proposed rationale |
|---|---:|---:|---|
| SS/MS and rank sums | 1e-12 | 2e-13 | Small finite inputs and arithmetic accumulation |
| F, raw H, corrected H | 5e-13 | 5e-12 | Accumulation/division, distinct statistics |
| Ordinary p-values | 2e-14 | 2e-10 | Tail evaluation has a different approximation budget |
| Counts, df, labels, RowIds, ordering, options | Exact | Discrete contract |
| Semantic table numbers | Underlying statistic's rule | Underlying statistic's rule | Cell type/p-value metadata/order remain exact |
| Extreme/underflow or boundary p-values; native geometry | **PENDING** | **PENDING** | Separate tail/log-domain/host-unit decision required |

The proposals fail on missing release fields, wrong R/stats version, malformed
hash/decision identifiers, nonfinite/degenerate input or result, unapproved tail
policy, warnings and existing output paths. Zero within-SS and C<=0 are rejected;
singleton/empty/new datasets are outside these fixed scripts. Boundary/underflow
p-values stop rather than using a generic absolute tolerance. These rejection
rules themselves require scientific approval before execution. A future policy
change requires a new reviewed script hash; do not edit a frozen approved script
or loosen tolerances to force agreement.

The scripts use only base/bundled R. They include a small writer for the fixed
primitive/list candidate schema, invariant decimal points and 17-digit numbers;
nonfinite outputs are rejected. The writer and exclusive `wx` output connection
are **source reviewed only**, not runtime-qualified. Their JSON and no-overwrite
behavior must be checked during the separately authorized controlled first run.
Do not represent this checkpoint as an executed R syntax/runtime test.

Machine-independent proposed output schema `04A02-independent-candidate-v1`:

- `comparison`: exact case ID, dataset hash/data/labels/provenance-only RowIds,
  source commit, alpha, seed N/A, invariant culture, method/options, distinct
  numerical field names and degrees of freedom. Always
  `status=PENDING_REVIEW_NOT_A_GOLDEN`.
- `provenance`: owner decision ID, tolerance policy ID, exact R/stats versions,
  Rscript executable/runtime/script SHA-256, citation and generated UTC time.
  UTC generation time and executable locations are excluded from numerical
  comparison keys; absolute machine paths are not written in this schema.

For ANOVA, fields include `FStatistic`, `PValue`, `SSBetween`, `SSWithin`,
`SSTotal`, `MSBetween`, `MSWithin`, `df`. For KW, preserve `HStatistic`, `PValue`,
`TieCorrectedHStatistic`, `TieCorrectedPValue`, tie correction and rank sums; the
separate formula H/C is explicitly an additional review field. Neither proposal
renders Common documents or native tables/charts. G3/G4 remain deferred.

After, and only after, a recorded run release, the operator must independently
compare actual SHA-256 for the chosen `Rscript.exe`, R runtime (including R.dll or
the approved platform equivalent) and the relevant source script against the
approved decision. The script requires `SHA256PreflightVerified=YES` as an
**operator attestation**, not cryptographic verification by R. It deliberately
does not shell out or install a hashing package. An attestation field must never
be filled merely to bypass the external hash check.

A future owner-supplied DCF would specify all these fields, with case-specific
TailPolicy `upper-F-2-6` or `upper-chi-square-2-raw-and-corrected` and
FiniteSingularPolicy `reject-unapproved-degenerate`. Default all release choices
to NO; no usable approval file is created by this checkpoint:

```text
RunIndependentReference: NO
PromoteNumericalGolden: NO
NativeWorkbookCapture: NO
OwnerDecision: PENDING
RVersion: 4.5.1
RscriptSHA256: PENDING
RRuntimeSHA256: PENDING
ScriptSHA256: PENDING
SHA256PreflightVerified: NO
TolerancePolicy: PENDING
TailPolicy: PENDING
FiniteSingularPolicy: PENDING
```

Future commands for review only — **NOT RUN**, no automatic hook:

```powershell
# Only after written release, verified hashes and a fresh authorized output root:
& $ApprovedRscript --vanilla docs/architecture/04A02/oracle-proposals/anova-proposed.R $ApprovedAnovaDecisionDcf $NewAnovaCandidateJson
& $ApprovedRscript --vanilla docs/architecture/04A02/oracle-proposals/kw-proposed.R $ApprovedKwDecisionDcf $NewKwCandidateJson
```

Generation produces independent **candidate** artifacts under a fresh run,
never an approved fixture rewrite. A later independent reviewer must reconcile
all differences with Core/Common candidates and inspect script/version/hash/tail
and tolerance provenance. Only a separate written promotion decision may create
a new immutable approved-reference record linked to the unchanged dataset.
Running a reference does not authorize promotion, native activity or 04A.03.
