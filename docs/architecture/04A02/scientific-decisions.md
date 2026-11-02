# Scientific reference decision sheet — PROPOSED, NOT APPROVED

Decision authority: Core inference/domain owner and roadmap owner. Neither case
has an approved independent reference, tolerance or promotion decision. No new
R/reference computation has been executed. All approval slots remain empty.

Proposed implementation: controlled R **4.5.1** with bundled stats of the same
version, offline installation to be separately authorized if needed. This is a
version candidate, not an assertion that it is installed or qualified. Before G2,
record executable/package versions, archive SHA-256, platform, source script and
script SHA-256, citations, generation time and owner decision in a new immutable
reference document. A different version requires an explicit decision. The live
R development manuals describe the relevant conventions but do not pin the
proposed executable: [oneway.test](https://stat.ethz.ch/R-manual/R-devel/library/stats/html/oneway.test.html)
and [kruskal.test](https://stat.ethz.ch/R-manual/R-devel/library/stats/html/kruskal.test.html).

| Decision | C30-SYN-ANOVA proposal | C30-SYN-KW proposal |
|---|---|---|
| Exact data/identity | Fixture values A/B/C and RowIds unchanged; provenance-only RowIds | Same; ties at value 2 explicitly retained |
| Model | Classical equal-variance one-way ANOVA; Welch OFF | Average pooled ranks; preserve raw and tie-corrected statistics separately |
| Settings | Alpha 0.05; all five optional flags OFF; no seed | Alpha 0.05; Dunn explicitly OFF despite production default ON; no seed |
| Statistic | F = [SS_between/(k−1)]/[SS_within/(N−k)] | H = 12/[N(N+1)] Σ(R_j²/n_j) − 3(N+1); C = 1 − Σ(t³−t)/(N³−N); corrected H = H/C |
| Degrees of freedom | Proposed exact structural df: between 2, within 6, total 8 (N=9, k=3); owner must approve | Proposed chi-square df k−1 = 2; owner must approve |
| Independent oracle/script plan | `oneway.test(response ~ factor(group, levels=c("A","B","C")), var.equal=TRUE)`; separately extract SS/MS/df with `anova(lm(...))` | `kruskal.test(list(A,B,C))` for corrected H/asymptotic p; separately scripted rank-sum H and correction formula for raw H/raw asymptotic p |
| P-value | Upper F tail with approved df | Propose upper chi-square tails of BOTH raw/corrected H. Exact/permutation/Monte Carlo is a separate unapproved case; no exact claim |
| Optional inference | No CIs, Welch or comparisons in this case | No Dunn, CIs or multiplicity inference in this case |
| Ordering | A, B, C and existing service table/cell order | A, B, C; rank ties never broken by provenance RowIds |

The R manual identifies `var.equal=TRUE` as classical F inference, while the
default is Welch. The KW manual describes an approximate chi-square distribution;
it is not an oracle for exact inference. The independent raw-H calculation must
be inspected separately; do not infer that a single corrected oracle validates
all four exposed service fields.

Proposed tolerance admission rules, subject to approval and recorded before
observing an independent comparison:

| Output class | Candidate rule | Reason/limit |
|---|---|---|
| Labels, RowIds, counts, df, order, flags, cell type/null/error metadata | Exact | Discrete contract, no numerical rounding allowance |
| SS/MS and rank sums | abs ≤ 1e-12, rel ≤ 2e-13 (either bound) | Small finite synthetic inputs; arithmetic accumulation allowance |
| F, H, corrected H | abs ≤ 5e-13, rel ≤ 5e-12 (either bound) | Accumulation and division; preserve distinct raw/corrected outputs |
| Ordinary p-values | abs ≤ 2e-14, rel ≤ 2e-10 (either bound) | Tail-function approximation differs from statistic arithmetic |
| Extreme/underflow p-values | No approved rule | Require log-tail/underflow policy and independent high-precision review; never blanket absolute pass |
| Semantic table numbers | Per underlying statistic/p-value class | Preserve numeric cells and p-value metadata independently |
| Rendered p-value text/number format/layout | Exact after reviewed formatting specification | Pending G3/native source-bound capture; no screenshot-based numeric oracle |
| Native chart coordinates | No tolerance yet | Units/host size/scale and independent capture protocol require approval |

Do not tune these bounds to make observed differences pass. ULP bounds are not
proposed without a platform-specific rationale. Record every deviation and keep
the production algorithm unchanged in this checkpoint.

Negative/singular numerical cases are a **separate future batch of cases**, not
variations of the two frozen inputs. Proposed policy for approval: reject missing,
nonfinite, empty or inconsistent inputs before oracle calls; specify singleton
groups, all-equal observations, zero within variance, C=0, insufficient groups,
overflow, warnings and undefined results explicitly. Do not silently impute,
drop rows, coerce errors to zero or accept a host fallback as a scientific rule.
Current negative tests validate fixture admission only, not production numerical
degeneracy behavior.

Existing `NistAnovaTests.vb`, NIST_ANOVA data and
`NonparametricReferenceTests.KruskalWallis_WithTies_MatchesReference` are unchanged
context with different datasets. They do not approve references for these cases.

- [ ] ANOVA method/df/edge policy, independent script/version and tolerances approved.
- [ ] KW raw/corrected/asymptotic policy, script/version and tolerances approved.
- [ ] Separate future semantic table specification approved.

Owner/role: pending. Decision reference/date: pending. Native-workbook authority:
NOT GIVEN; requires its own case-specific mitigation/regression/release decision.
